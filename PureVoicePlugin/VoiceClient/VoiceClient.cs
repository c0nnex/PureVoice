
using ConcurrentCollections;
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

namespace PureVoice.VoiceClient
{
    public class VoiceClient : INetLogger
    {
        NetManager Client = null;
        NetManager ServerListener = null;

        EventBasedNetListener clientListener = new EventBasedNetListener();
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        private VoicePaketConfig _configuration;
        private VoicePaketSetup _connectionInfo;
        private System.Timers.Timer _timer;
        private DateTime _lastPureVoicePing = DateTime.Now;
        private bool _needConnection = false;
        public event EventHandler<VoiceClient> OnDisconnected;
        private bool _GotWarning = false;
        private static bool _ShutDown = false;
        private SmartLock smartLock = new SmartLock("VoiceClient");

        public bool IsConnected
        {
            get
            {
                if (Client == null)
                    return false;
                if (!Client.IsRunning)
                    return false;
                var p = Client?.GetFirstPeer();
                if (p != null)
                    return p.ConnectionState != ConnectionState.Disconnected;
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


        }

        public void AcceptConnections()
        {
            NetDebug.Logger = this;

            _netPacketProcessor.SubscribeNetSerializable<VoicePaketSetup, NetPeer>(onVoiceSetup);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketCommand, NetPeer>(onVoiceCommand);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketUpdate, NetPeer>(onVoiceUpdate);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketMute, NetPeer>(onVoiceMute);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketBatchMute, NetPeer>(onVoiceBatchMute);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketBatchUpdate, NetPeer>(onVoiceBatchUpdate);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketConfigureClient, NetPeer>(onVoiceConfigureClient);

            clientListener.NetworkReceiveEvent +=
                (peer, reader, method) =>
                {
                    _netPacketProcessor.ReadAllPackets(reader, peer);
                };
            clientListener.PeerConnectedEvent += (p) => VoicePlugin.Log("PeerConnectedEvent {0}", p);
            clientListener.PeerDisconnectedEvent += ClientListener_PeerDisconnectedEvent;
            clientListener.NetworkErrorEvent += (p, e) => VoicePlugin.Log("NetworkErrorEvent {0} => {1}", p, e);
            clientListener.NetworkLatencyUpdateEvent += ClientListener_NetworkLatencyUpdateEvent;
            
            _timer = new System.Timers.Timer();
            _timer.Interval = 100;
            _timer.Elapsed += (s, e) =>
            {
                if (_needConnection)
                {
                    if (IsConnected && DateTime.Now - _lastPureVoicePing > TimeSpan.FromSeconds(10))
                    {
                        _needConnection = false;
                        var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                        if (con != null)
                        {
                            con.DisconnectVoiceServer();
                        }
                        OnDisconnected?.Invoke(this, this);
                        Disconnect();
                        return;
                    }
                    
                    if (!IsConnected)
                    {
                        StartServerConnection();
                        return;
                    }
                }
                if (IsConnected)
                    Client?.PollEvents();
            };
            
            if (!_timer.Enabled)
                _timer.Start();

            var evL = new EventBasedNetListener();
            evL.NetworkReceiveUnconnectedEvent += (ep, reader, messageType) => ServerListener_NetworkReceiveUnconnectedEvent(ep, reader, messageType);
            ServerListener = new NetManager(evL) { UnconnectedMessagesEnabled = true,UnsyncedEvents = true };
            ServerListener.Start(4239);
            WebListener.ConnectionRequestReceived += WebListener_ConnectionRequestReceived;
            WebListener.StartListen();
            VoicePlugin.Log("voiceClient waiting for Connectioninfo...");
            _ShutDown = false;
        }

        private void WebListener_ConnectionRequestReceived(object sender, VoicePaketConfig e)
        {
            try
            {
                if (_ShutDown)
                    return;
                _lastPureVoicePing = DateTime.Now;

                VoicePlugin.Log("Accept Configuration via Web");
                if (IsConnected)
                {
                    VoicePlugin.Log("Already connected");
                    return;
                }

                if (e.ClientVersionRequired > VoicePlugin.PluginVersion)
                {
                    if (!_GotWarning)
                    {
                        _GotWarning = true;
                        MessageBox.Show($"{VoicePlugin.Name} outdated. Version {e.ClientVersionRequired} is required. Please update.", "PureVoice Plugin Outdated");
                    }
                    return;
                }
                _lastPureVoicePing = DateTime.Now;
                //VoicePlugin.Log("process VoicePaketConfig {0}", _configuration);
                _configuration = e;
                _needConnection = true;
                // StartServerConnection();
            }
            catch(Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        public void Shutdown()
        {
            _ShutDown = true;
            VoicePlugin.Log("Shutting down voiceClient");
            if (_timer.Enabled)
                _timer.Stop();
            Client?.Stop();
            ServerListener?.Stop();
        }

        private void CreateClientConnection()
        {
            Disconnect();
            Client = new NetManager(clientListener, 1);
            Client.UnsyncedEvents = false;
            Client.UnconnectedMessagesEnabled = false;
            Client.Start();
        }

        public void Disconnect()
        {
            smartLock.Lock(() =>
           {
               if (Client != null)
               {
                   var cl = Client;
                   Client = null;
                   cl?.DisconnectAll();
                   cl?.Stop();
                   cl = null;
               }
           });
        }

        private void ClientListener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            try
            {
                if (_needConnection && (DateTime.Now - _lastPureVoicePing > TimeSpan.FromSeconds(10)))
                {
                    _needConnection = false;
                    var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.DisconnectVoiceServer();
                    }
                    OnDisconnected?.Invoke(this, this);
                    Disconnect();
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
            try
            {
                VoicePlugin.Log("PeerDisconnectedEvent {0} => {1}", peer, disconnectInfo.Reason);

                _needConnection = false;
                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con != null)
                {
                    con.DisconnectVoiceServer();
                }
                OnDisconnected?.Invoke(this, this);
                Disconnect();
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        private void ServerListener_NetworkReceiveUnconnectedEvent(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            try
            {
                if (_ShutDown)
                    return;
                if ((remoteEndPoint.Host != "127.0.0.1") && (remoteEndPoint.Host != "[::1]"))
                {
                    VoicePlugin.Log("Refused from {0}", remoteEndPoint);
                    return;
                }
                if (messageType != UnconnectedMessageType.BasicMessage)
                {
                    VoicePlugin.Log("Refused {1} from {0}", remoteEndPoint, messageType);
                    return;
                }

                var hello = reader.GetString();
                var voiceVersion = reader.GetInt();
                if (voiceVersion != 1)
                {
                    VoicePlugin.Log("Refused v {1} from {0}", remoteEndPoint, voiceVersion);
                    return;
                }
                if ((hello == "GTMPVOICECOMMAND") || (hello == "PUREVOICECOMMAND"))
                {
                    VoicePlugin.Log("Command from {0}", remoteEndPoint);
                    if (IsConnected)
                    {
                        var Connection = Client?.GetFirstPeer();
                        if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                            _netPacketProcessor.Send(Connection, new VoicePaketCommand() { Command = reader.GetString(), Data = reader.GetString() } , DeliveryMethod.ReliableOrdered);
                    }
                    return;
                }
                if ((hello != "GTMPVOICE") && (hello != "PUREVOICE"))
                {
                    VoicePlugin.Log("Invalid configuration from {0}", remoteEndPoint);
                    return;
                }
                _configuration = new VoicePaketConfig();
                _configuration.Deserialize(reader);

                _lastPureVoicePing = DateTime.Now;

                VoicePlugin.Log("Accept Configuration from {0}", remoteEndPoint);
                if (IsConnected)
                {
                    VoicePlugin.Log("Already connected");
                    return;
                }

                if (_configuration.ClientVersionRequired > VoicePlugin.PluginVersion)
                {
                    if (!_GotWarning)
                    {
                        _GotWarning = true;
                        MessageBox.Show($"{VoicePlugin.Name} outdated. Version {_configuration.ClientVersionRequired} is required. Please update.", "PureVoice Plugin Outdated");
                    }
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
            var cp = new VoicePaketConnectServer() { Secret = _configuration.ServerSecret, Version = Assembly.GetExecutingAssembly().GetName().Version, ClientGUID = _configuration.ClientGUID };
            var nw = new NetDataWriter();
            cp.Serialize(nw);
            CreateClientConnection();
            Client.Connect(_configuration.ServerIP, _configuration.ServerPort, nw);
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
                if ((con != null) && con.IsInitialized && con.IsConnected)
                {
                    if (!VoicePlugin_ConnectionEstablished(con))
                    {
                        VoicePlugin.Log("Disconnecting Peer (TS Server not ready)");
                        Disconnect();
                    }
                }
                else
                {
                    VoicePlugin.Log("Disconnecting Peer (TS Server not connected)");
                    Disconnect();
                }
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

        void onVoiceMute(VoicePaketMute args, NetPeer peer)
        {
            var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
            if (con == null)
                return;
            if (args.IsMute)
            {
                VoicePlugin.Log("Mute {0}", args.Name);
                if (args.Name == "_ALL_")
                    con.Mute("_ALL_");
                else
                    con.GetClient(args.Name)?.Mute();
            }
            else
            {
                VoicePlugin.Log("Unmute {0}", args.Name);
                con.GetClient(args.Name)?.Unmute();
            }
        }

        void onVoiceUpdate(VoicePaketUpdate args, NetPeer peer)
        {
            try
            {
                var con = VoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                Client cl = con.GetClient(args.PlayerName);
                if (cl == null)
                {
                    VoicePlugin.Log("UpdatePosition {0}: {1} {2} {3} NOT FOUND", args.PlayerName, args.Position, args.PositionIsRelative, args.VolumeModifier);
                    return;
                }
                cl.UpdatePosition(args.Position, args.VolumeModifier, args.PositionIsRelative);
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
                VoicePlugin.Log("BatchMute {0}", data.Mutelist.Count);
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
                var mute = new HashSet<ushort>( con.GetChannel(con.IngameChannel).AllClients);
                mute.Remove(con.LocalClientId);
                var unmute = new HashSet<ushort>();
                if (data.Data.Length > 0)
                {
                    data.Data.ForEach(d =>
                    {
                        if (d.ClientID == 0)
                        {
                            d.ClientID = con.GetClientId(d.ClientName);
                        }
                        if (d.ClientID != 0)
                        {
                            mute.Remove(d.ClientID);
                            unmute.Add(d.ClientID);
                            con.GetClient(d.ClientID)?.UpdatePosition(d.Position, d.VolumeModifier, false, false);
                        }
                    });
                }
                if (mute.SetEquals(con._MutedClients) && unmute.SetEquals(con._UnmutedClients))
                    return;
                VoicePlugin.Log("BatchUpdate {0}", data.Data.Length);
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
                    var Connection = Client.GetFirstPeer();
                    Connection?.Disconnect();
                    Connection = null;
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
            var Connection = Client?.GetFirstPeer();
            if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                _netPacketProcessor.Send(Connection, new VoicePaketTalking(isTalking), DeliveryMethod.ReliableOrdered);
        }

        internal void SendSpeakersStatusChanged(bool isMuted)
        { 
            if (!IsConnected)
                return;
            var Connection = Client?.GetFirstPeer();
            if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                _netPacketProcessor.Send(Connection, new VoicePaketSpeakersState(isMuted), DeliveryMethod.ReliableOrdered);
        }

        internal void SendMicrophoneStatusChanged(bool isMuted)
        {
            if (!IsConnected)
                return;
            var Connection = Client?.GetFirstPeer();
            if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                _netPacketProcessor.Send(Connection, new VoicePaketMicrophoneState(isMuted), DeliveryMethod.ReliableOrdered);
        }

        public void WriteNet(ConsoleColor color, string str, params object[] args)
        {
            VoicePlugin.Log(str, args);
        }
    }
}
