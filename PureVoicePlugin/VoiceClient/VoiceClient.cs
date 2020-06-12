
using PureVoice.VoiceClient.Model;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using PureVoice.DSP;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PureVoice.VoiceClient
{
    public class VoiceClient : INetLogger
    {
        NetManager Client = null;
        NetManager ServerListener = null;

        EventBasedNetListener clientListener = new EventBasedNetListener();
        private VoicePaketProcessor _netPacketProcessor;
        private VoicePaketConfig _configuration;
        private VoicePaketSetup _connectionInfo;
        private Thread clientThread;
        private DateTime _lastPureVoicePing = DateTime.Now;
        private bool _needConnection = false;
        public event EventHandler<VoiceClient> OnDisconnected;
        private bool _GotWarning = false;
        private static bool _ShutDown = false;
        private SmartLock smartLock = new SmartLock("VoiceClient");
        private DateTime ServerConnectionTimeout = DateTime.MinValue;
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public bool IsConnected
        {
            get
            {
                if (Client == null)
                    return false;
                if (!Client.IsRunning)
                    return false;
                return Client.IsConnected();
            }
        }

        public bool IsConnecting
        {
            get
            {
                if (Client == null)
                    return false;
                if (!Client.IsRunning)
                    return false;
                return Client.GetPeersCount(ConnectionState.Outgoing) > 0;
                return false;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (Client != null)
                    return Client.IsRunning;
                return false;
            }
        }

        public VoiceClient()
        {

            NetDebug.Logger = this;
            _netPacketProcessor = new VoicePaketProcessor();
            _netPacketProcessor.Subscribe<VoicePaketSetup, NetPeer>(onVoiceSetup);
            _netPacketProcessor.Subscribe<VoicePaketCommand, NetPeer>(onVoiceCommand);
            _netPacketProcessor.Subscribe<VoicePaketUpdate, NetPeer>(onVoiceUpdate);
            _netPacketProcessor.Subscribe<VoicePaketMute, NetPeer>(onVoiceMute);
            _netPacketProcessor.Subscribe<VoicePaketBatchMute, NetPeer>(onVoiceBatchMute);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketBatchUpdate, NetPeer>(onVoiceBatchUpdate);
            _netPacketProcessor.Subscribe<VoicePaketConfigureClient, NetPeer>(onVoiceConfigureClient);
            _netPacketProcessor.Subscribe<VoicePaketUpdateDistortion, NetPeer>(onVoiceUpdateDistortion);

            clientListener.NetworkReceiveEvent +=
            (peer, reader, method) =>
            {
                try
                {
#if DEBUG
                    VoicePlugin.Log("Receive " + reader.PeekULong().ToString("X") + "\r\n" + reader.RawData.HexDump(reader.UserDataOffset, reader.UserDataSize));
#endif
                    _netPacketProcessor.ReadAllPackets(reader, peer);
                }
                catch (Exception ex)
                {
                    VoicePlugin.Log(ex.ToString());
                }
            };
            clientListener.PeerConnectedEvent += (p) => VoicePlugin.Log("PeerConnectedEvent {0}", p);
            clientListener.PeerDisconnectedEvent += ClientListener_PeerDisconnectedEvent;
            clientListener.NetworkErrorEvent += ClientListener_NetworkErrorEvent;
            clientListener.NetworkLatencyUpdateEvent += ClientListener_NetworkLatencyUpdateEvent;

        }

        public void AcceptConnections()
        {
            if (ServerListener == null)
            {
                clientThread = new Thread(VoiceClientThread) { IsBackground = true };
                clientThread.Start(CancellationTokenSource.Token);

                var evL = new EventBasedNetListener();
                evL.NetworkReceiveUnconnectedEvent += (ep, reader, messageType) => ServerListener_NetworkReceiveUnconnectedEvent(ep, reader, messageType);
                ServerListener = new NetManager(evL) { UnconnectedMessagesEnabled = true, UnsyncedEvents = true };
                ServerListener.Start(4239);
                WebListener.ConnectionRequestReceived += WebListener_ConnectionRequestReceived;
                WebListener.StartListen();
                VoicePlugin.Log("voiceClient waiting for Connectioninfo...");
                _ShutDown = false;
            }
        }

        private void VoiceClientThread(object token)
        {
            CancellationToken cancellationToken = (CancellationToken)token;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_needConnection)
                {
                    if (IsConnected && VoicePlugin.IsConnectedToVoiceServer)
                    {
                        if (DateTime.Now - _lastPureVoicePing > TimeSpan.FromSeconds(10))
                        {
                            VoicePlugin.Log("No ping from game for 10 Seconds. assuming dead. Disconnecting");
                            WeGotDisconnected();
                        }
                    }
                    else
                    {
                        if (!IsConnecting)
                            StartServerConnection();
                        else
                        {
                            if (DateTime.Now > ServerConnectionTimeout)
                            {
                                VoicePlugin.Log("Timeout connecting to server. Retrying....");
                                StartServerConnection();
                            }
                        }
                    }
                }
                Client?.PollEvents();
                Thread.Sleep(100);
            }
        }

        private void WebListener_ConnectionRequestReceived(object sender, VoicePaketConfig e)
        {
            try
            {
                if (_ShutDown)
                    return;
                _lastPureVoicePing = DateTime.Now;


                if (IsConnected)
                {
                    VoicePlugin.VerboseLog("Already connected");
                    return;
                }

                VoicePlugin.VerboseLog("Accept Configuration via Web");
                if (e.ClientVersionRequired > VoicePlugin.PluginVersion)
                {
                    if (!_GotWarning)
                    {
                        _GotWarning = true;
                        MessageBox.Show($"{VoicePlugin.Name} outdated. Version {e.ClientVersionRequired} is required. Please update.", "PureVoice Plugin Outdated");
                    }
                    return;
                }
                if (VoicePlugin.GetConnection(e.ServerGUID) == null)
                {
                    VoicePlugin.VerboseLog("Not connected to target TS Server");
                    return;
                }
                _lastPureVoicePing = DateTime.Now;
                //VoicePlugin.Log("process VoicePaketConfig {0}", _configuration);
                _configuration = e;
                _needConnection = true;
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        public void Shutdown()
        {
            _ShutDown = true;
            VoicePlugin.Log("Shutting down voiceClient");
            CancellationTokenSource.Cancel();
            Client?.Stop();
            ServerListener?.Stop();
            WebListener.StopListen();
        }

        private void CreateClientConnection()
        {
            Disconnect();
            Client = new NetManager(clientListener);
            Client.UnsyncedEvents = true;
            Client.UnconnectedMessagesEnabled = false;
            Client.Start();
        }

        public void Disconnect()
        {
            smartLock.Lock(() =>
           {
               if (Client != null)
               {
                   var tcx = new CancellationTokenSource();
                   Task.Run(() =>
                    {
                        var cl = Client;
                        Client = null;
                        cl?.DisconnectAll();
                        cl?.Stop(false);
                        cl = null;
                        VoicePlugin.IsConnectedToVoiceServer = false;
                    });
                   tcx.CancelAfter(1000);
               }
           });
        }

        private void ClientListener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            try
            {
                if (_needConnection)
                {
                    if (DateTime.Now - _lastPureVoicePing > TimeSpan.FromSeconds(10))
                    {
                        VoicePlugin.Log("No ping from game for 10 Seconds. assuming dead. Disconnecting");
                        WeGotDisconnected();
                    }
                }
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        internal bool VoicePlugin_ConnectionEstablished(Connection e)
        {
            try
            {
                if (_connectionInfo == null)
                {
                    VoicePlugin.Log("ConnectionEstablished no info ({0})", e.GUID);
                    return false;
                }
                e.Init();
                if (!e.IsInitialized)
                {
                    VoicePlugin.Log("ConnectionEstablished not initialized ({0})", e.GUID);
                    return false;
                }
                if (e.GUID == _connectionInfo.ServerGUID)
                {
                    if (e.IsVoiceEnabled)
                        VoicePlugin.Log("ConnectionEstablished Gotcha! ({0})", e.GUID);
                    e.UpdateSetup(_connectionInfo);

                    foreach (var item in _connectionInfo.DistortionSettings)
                    {
                        VoicePlugin.Log($"Distortion {item}");
                        VoiceFilterChain fChain = new VoiceFilterChain();

                        foreach (var filterItem in item.Filters)
                        {
                            if (String.IsNullOrEmpty(filterItem))
                                continue;
                            var parts = filterItem.Split(new string[] { "##" },StringSplitOptions.RemoveEmptyEntries);
                            var filterClass = GetArg(parts,0, "Presets.Default");
                            var filterSettings = GetArg(parts, 1, "");

                            var type = Type.GetType("PureVoice.DSP." + filterClass, false);
                            if (type == null)
                            {
                                VoicePlugin.VerboseLog($"Class PureVoice.DSP.{filterClass} not found, using Standard");
                                type = typeof(PureVoice.DSP.Presets.Default);
                            }
                            try
                            {
                                // TODO: Ask User for permission if type is outside baseplugin and not authenticode signed
                                object itemObject = Activator.CreateInstance(type);

                                if (itemObject is VoiceFilterChain)
                                {
                                    fChain = (VoiceFilterChain)itemObject;
                                    fChain.Congfigure(filterSettings);
                                }
                                if (itemObject is IVoiceFilter)
                                {
                                    var iFilter = (IVoiceFilter)itemObject;
                                    fChain.AddFilter(iFilter.WithConfiguration(filterSettings));
                                }
                            }
                            catch(Exception ex)
                            {
                                VoicePlugin.Log("Problem instanciating FilterChain " + filterClass + ": " + ex.Message);
                                continue;
                            }
                        }
                        e.AddVoiceFilter(item.VoiceDistortion, fChain);
                    }

                    _netPacketProcessor.Send(Client, new VoicePaketConnectClient()
                    {
                        ClientGUID = _configuration.ClientGUID,
                        TeamspeakID = e.LocalClient.GUID,
                        TeamspeakClientID = e.LocalClientId,
                        TeamspeakClientName = e.LocalClient.Name,
                        MicrophoneMuted = e.LocalClient.IsMicrophoneMuted,
                        SpeakersMuted = e.LocalClient.IsSpeakersMuted
                    }, DeliveryMethod.ReliableOrdered);
                }
                else
                {
                    VoicePlugin.Log("ConnectionEstablished wrong server ({0})", e.GUID);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
                return false;
            }
        }

        private void ClientListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            VoicePlugin.Log("PeerDisconnectedEvent {0} => {1}", peer, disconnectInfo.Reason);
            WeGotDisconnected();
        }

        private void ClientListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            VoicePlugin.Log("PeerDisconnectedEvent {0} => {1}", endPoint, socketErrorCode);
            WeGotDisconnected();
        }

        private void WeGotDisconnected()
        {
            try
            {
                VoicePlugin.VerboseLog("WeGotDisconnected");
                _needConnection = false;
                if (_connectionInfo != null)
                {
                    var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.DisconnectVoiceServer();
                    }
                }
                OnDisconnected?.Invoke(this, this);
                Disconnect();
                VoicePlugin.IsConnectedToVoiceServer = false;
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        public static T GetArg<T>(IEnumerable<object> args, int index, T defaultValue = default(T))
        {
            var tmpList = args.ToList();
            if ((args == null) || (index >= tmpList.Count))
                return defaultValue;
            try
            {
                T tmp = (T)Convert.ChangeType(tmpList[index], typeof(T), CultureInfo.InvariantCulture);
                return tmp;
            }
            catch { return defaultValue; }
        }

        private void ServerListener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            try
            {
                if (_ShutDown)
                    return;
                if ((remoteEndPoint.Address.ToString() != "127.0.0.1") && (remoteEndPoint.Address.ToString() != "[::1]"))
                {
                    VoicePlugin.Log("Refused from {0}", remoteEndPoint);
                    return;
                }
                if (messageType != UnconnectedMessageType.BasicMessage)
                {
                    VoicePlugin.Log("Refused {1} from {0}", remoteEndPoint, messageType);
                    return;
                }
                string hello = null;
                int voiceVersion;

                if (reader.PeekChar() != 'P')
                    return;

                if (reader.AvailableBytes > 1024)
                    return;

                var inStr = BitConverter.ToString(reader.GetRemainingBytes());
                var vals = inStr.Split('|');

                hello = GetArg(vals, 0, "XXXX");
                voiceVersion = GetArg(vals, 1, 0);
                if (voiceVersion != 1)
                {
                    VoicePlugin.Log("Refused v {1} from {0}", remoteEndPoint, voiceVersion);
                    return;
                }
                switch (hello)
                {
                    case "PUREVOICE":
                        {
                            _configuration = new VoicePaketConfig();
                            _configuration.ServerIP = GetArg(vals, 2, "127.0.0.1");
                            _configuration.ServerPort = GetArg(vals, 3, 0);
                            _configuration.ServerSecret = GetArg(vals, 4, "geheim");
                            _configuration.ServerGUID = GetArg(vals, 5, "geheim");
                            _configuration.ClientGUID = GetArg(vals, 6, "unnbekannt");
                            Version.TryParse(GetArg(vals, 7, "0.0.0.0"), out _configuration.ClientVersionRequired);
                            break;
                        }
                    case "PUREVOICECOMMAND":
                        {
                            VoicePlugin.Log("Command from {0}", remoteEndPoint);
                            if (IsConnected)
                            {
                                if ((Client != null) && (Client.ConnectedPeersCount != 0))
                                    _netPacketProcessor.Send(Client, new VoicePaketCommand() { Command = GetArg(vals, 2, "IGNORE"), Data = GetArg(vals, 3, "IGNORE") }, DeliveryMethod.ReliableOrdered);
                            }
                            return;
                        }
                    default:
                        return;
                }

                _lastPureVoicePing = DateTime.Now;


                if (IsConnected)
                {
                    VoicePlugin.VerboseLog("Already connected");
                    return;
                }
                VoicePlugin.Log("Accept Configuration from {0}", remoteEndPoint);
                if (_configuration.ClientVersionRequired > VoicePlugin.PluginVersion)
                {
                    if (!_GotWarning)
                    {
                        _GotWarning = true;
                        MessageBox.Show($"{VoicePlugin.Name} outdated. Version {_configuration.ClientVersionRequired} is required. Please update.", "PureVoice Plugin Outdated");
                    }
                    return;
                }
                if (VoicePlugin.GetConnection(_configuration.ServerGUID) == null)
                {
                    VoicePlugin.VerboseLog("Not connected to target TS Server");
                    return;
                }
                _lastPureVoicePing = DateTime.Now;
                //VoicePlugin.Log("process VoicePaketConfig {0}", _configuration);
                _needConnection = true;
                StartServerConnection();
            }
            catch (Exception ex)
            {
                VoicePlugin.Log("Invalid Serverpaket from {0}: {1}", remoteEndPoint, ex.Message);
            }
        }

        private void StartServerConnection()
        {
            try
            {
                VoicePlugin.Log("Trying to connect to " + _configuration.ServerIP + ":" + _configuration.ServerPort);
                var cp = new VoicePaketConnectServer() { Secret = _configuration.ServerSecret, Version = Assembly.GetExecutingAssembly().GetName().Version, ClientGUID = _configuration.ClientGUID };
                var nw = new NetDataWriter();
                cp.Serialize(nw);
                CreateClientConnection();
                ServerConnectionTimeout = DateTime.Now.AddSeconds(30);
                Client.Connect(_configuration.ServerIP, _configuration.ServerPort, nw);
            }
            catch { }

        }

        private void ClientListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            VoicePlugin.Log("conrequest ");
        }

        void onVoiceSetup(VoicePaketSetup args, NetPeer peer)
        {
            try
            {
                VoicePlugin.Log("VoiceSetup {0}", args);
                _connectionInfo = args;
                var con = VoicePlugin.GetConnection(args.ServerGUID);
                if ((con != null) && con.IsConnected)
                {
                    if (!VoicePlugin_ConnectionEstablished(con))
                    {
                        VoicePlugin.Log("Disconnecting Peer (TS Server not ready)");
                        WeGotDisconnected();
                    }
                    else
                    {
                        VoicePlugin.IsConnectedToVoiceServer = true;
                    }
                }
                else
                {
                    VoicePlugin.Log("Disconnecting Peer (TS Server not connected)");
                    WeGotDisconnected();
                }
                ServerConnectionTimeout = DateTime.MaxValue;
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        void onVoiceConfigureClient(VoicePaketConfigureClient args, NetPeer peer)
        {
            var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
            if (con != null)
            {
                con.SetCanLeaveIngameChannel(args.CanLeaveIngameChannel);
                con.ConnectVoiceServer(args.Nickname);
            }
        }

        private void onVoiceUpdateDistortion(VoicePaketUpdateDistortion args, NetPeer arg2)
        {
            try
            {
                VoicePlugin.Log("UpdateDistortion {0}: {1} play {2}", args.ClientID, args.VoiceDistortion, args.VoiceSound);
                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                Client cl = con.GetClient(args.ClientID);
                if (cl == null)
                {
                    VoicePlugin.Log("UpdateDistortion {0}: NOT FOUND", args.ClientID);
                    return;
                }
                cl.SetVoiceDistortion(args.VoiceDistortion);
                if (!String.IsNullOrEmpty(args.VoiceSound) && (args.VoiceSound != "NONE"))
                {
                    con.PlaySound(args.VoiceSound, args.ClientID == 0 || args.ClientID == con.LocalClientId);
                }
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        void onVoiceMute(VoicePaketMute args, NetPeer peer)
        {
            var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
            if (con == null)
                return;
            if (args.IsMute)
            {
                VoicePlugin.VerboseLog("Mute {0}", args.ClientID);
                if (args.ClientID == 0)
                    con.Mute("_ALL_");
                else
                    con.GetClient(args.ClientID)?.Mute();
            }
            else
            {
                VoicePlugin.VerboseLog("Unmute {0}", args.ClientID);
                con.GetClient(args.ClientID)?.Unmute();
            }
        }

        void onVoiceUpdate(VoicePaketUpdate args, NetPeer peer)
        {
            try
            {
                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                Client cl = con.GetClient(args.ClientID);
                if (cl == null)
                {
                    VoicePlugin.Log("UpdatePosition {0}: {1} {2} {3} NOT FOUND", args.ClientID, args.Position, args.PositionIsRelative, args.VolumeModifier);
                    return;
                }
                cl.UpdatePosition(args.Position, args.VolumeModifier, args.SignalQuality, args.VoiceDistortion, args.PositionIsRelative);
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }

        }

        void onVoiceBatchMute(VoicePaketBatchMute data, NetPeer peer)
        {
            try
            {
                VoicePlugin.VerboseLog("BatchMute {0}", data.Mutelist.Count);
                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                data.Mutelist.ForEach(d => con.GetClient(d)?.Mute());
                con.DoMuteList(data.Mutelist.ToArray());
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }


        void onVoiceBatchUpdate(VoicePaketBatchUpdate data, NetPeer peer)
        {
            try
            {

                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                var mute = new HashSet<ushort>(con.GetChannel(con.IngameChannel).AllClients);
                mute.Remove(con.LocalClientId);
                var unmute = new HashSet<ushort>();
                if (data.BatchData.Count > 0)
                {
                    data.BatchData.ForEach(d =>
                    {
                        if (d.ClientID != 0)
                        {
                            mute.Remove(d.ClientID);
                            unmute.Add(d.ClientID);
                            VoicePlugin.VerboseLog(() => $"BatchUpdate {d.ClientID} v {d.VolumeModifier} d {d.VoiceDistortion} q {d.SignalQuality} p {d.Position}");
                            con.GetClient(d.ClientID)?.UpdatePosition(d.Position, d.VolumeModifier, d.SignalQuality, d.VoiceDistortion, false, false);
                        }
                    });
                }
                if (mute.SetEquals(con._MutedClients) && unmute.SetEquals(con._UnmutedClients))
                    return;
                VoicePlugin.VerboseLog("BatchUpdate {0}", data.BatchData.Count);
                con._UnmutedClients = new ConcurrentHashSet<ushort>(unmute);
                con._MutedClients = new ConcurrentHashSet<ushort>(mute);
                con._MutedClients.TryRemove(con.LocalClientId);
                con.DoMuteActions();
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        void onVoiceCommand(VoicePaketCommand args, NetPeer peer)
        {
            try
            {
                VoicePlugin.Log("VoiceCommand {0} => {1}", args.Command, args.Data);
                if (args.Command == "DISCONNECT")
                {
                    _needConnection = false;
                    Client?.DisconnectAll();
                    var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.DisconnectVoiceServer();
                    }
                    OnDisconnected?.Invoke(this, this);
                    return;
                }
                if (args.Command == "SETNAME")
                {
                    var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.ConnectVoiceServer(args.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        public void SendTalkStatusChanged(bool isTalking)
        {
            if (!IsConnected)
                return;
            if ((Client != null) && (Client.IsConnected()))
                _netPacketProcessor.Send(Client, new VoicePaketTalking(isTalking), DeliveryMethod.ReliableOrdered);
        }

        internal void SendSpeakersStatusChanged(bool isMuted)
        {
            if (!IsConnected)
                return;
            if ((Client != null) && (Client.IsConnected()))
                _netPacketProcessor.Send(Client, new VoicePaketSpeakersState(isMuted), DeliveryMethod.ReliableOrdered);
        }

        internal void SendMicrophoneStatusChanged(bool isMuted)
        {
            if (!IsConnected)
                return;
            if ((Client != null) && (Client.IsConnected()))
                _netPacketProcessor.Send(Client, new VoicePaketMicrophoneState(isMuted), DeliveryMethod.ReliableOrdered);
        }

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            if (level < NetLogLevel.Trace)
                VoicePlugin.Log(str, args);
        }
    }
}
