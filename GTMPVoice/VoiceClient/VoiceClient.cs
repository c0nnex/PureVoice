
using ConcurrentCollections;
using GTMPVoice.VoiceClient.Model;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace GTMPVoice.VoiceClient
{
    public class VoiceClient : INetLogger
    {
        NetManager Client = null;
        EventBasedNetListener clientListener = new EventBasedNetListener();
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        private VoicePaketConfig _configuration;
        private VoicePaketSetup _connectionInfo;
        private System.Timers.Timer _timer;
        private DateTime _lastGTMPPing = DateTime.Now;
        private bool _needConnection = false;
        public event EventHandler<VoiceClient> OnDisconnected;
        private bool _GotWarning = false;

        public bool IsConnected
        {
            get
            {
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
            clientListener.PeerConnectedEvent += (p) => GTMPVoicePlugin.Log("PeerConnectedEvent {0}", p);
            clientListener.PeerDisconnectedEvent += ClientListener_PeerDisconnectedEvent;
            clientListener.NetworkErrorEvent += (p, e) => GTMPVoicePlugin.Log("NetworkErrorEvent {0} => {1}", p, e);
            clientListener.NetworkReceiveUnconnectedEvent += ClientListener_NetworkReceiveUnconnectedEvent;
            clientListener.NetworkLatencyUpdateEvent += ClientListener_NetworkLatencyUpdateEvent;
            Client = new NetManager(clientListener, 1);
            Client.UnsyncedEvents = true;
            Client.UnconnectedMessagesEnabled = true;
            _timer = new System.Timers.Timer();
            _timer.Interval = 30000;
            _timer.Elapsed += (s, e) =>
            {
                if (_needConnection)
                {
                    var Connection = Client.GetFirstPeer();
                    if ((Connection == null) || (Connection.ConnectionState != ConnectionState.Connected))
                        StartServerConection();
                }
            };
            Client.Start(4239);
            if (!_timer.Enabled)
                _timer.Start();
            GTMPVoicePlugin.Log("voiceClient waiting for Connectioninfo...");
        }

        public void Shutdown()
        {
            GTMPVoicePlugin.Log("Shutting down voiceClient");
            if (_timer.Enabled)
                _timer.Stop();
            Client.Stop();
        }

        private void ClientListener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            try
            {
                if (_needConnection && (DateTime.Now - _lastGTMPPing > TimeSpan.FromSeconds(10)))
                {
                    _needConnection = false;
                    var Connection = Client.GetFirstPeer();
                    if (Connection != null)
                        Client.DisconnectPeerForce(Connection);
                    Connection = null;
                    var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.DisconnectVoiceServer();
                    }
                    OnDisconnected?.Invoke(this, this);
                }
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }

        internal bool GTMPVoicePlugin_ConnectionEstablished(Connection e)
        {
            try
            {
                if (_connectionInfo == null)
                {
                    GTMPVoicePlugin.Log("GTMPVoicePlugin_ConnectionEstablished no info ({0})", e.GUID);
                    return false;
                }
                if (!e.IsInitialized)
                {
                    GTMPVoicePlugin.Log("GTMPVoicePlugin_ConnectionEstablished not initialized ({0})", e.GUID);
                    return false;
                }
                if (e.GUID == _connectionInfo.ServerGUID)
                {
                    if (e.IsVoiceEnabled)
                    GTMPVoicePlugin.Log("GTMPVoicePlugin_ConnectionEstablished Gotcha! ({0})", e.GUID);
                    e.UpdateSetup(_connectionInfo);
                    _netPacketProcessor.Send(Client, new VoicePaketConnectClient()
                    {
                        ClientGUID = _configuration.ClientGUID,
                        TeamspeakID = e.LocalClient.GUID,
                        TeamspeakClientID = e.LocalClientId
                    }, DeliveryMethod.ReliableOrdered);
                }
                else
                {
                    GTMPVoicePlugin.Log("GTMPVoicePlugin_ConnectionEstablished wrong server ({0})", e.GUID);
                }
                return true;
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
                return false;
            }
        }

        private void ClientListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            try
            {
                GTMPVoicePlugin.Log("PeerDisconnectedEvent {0} => {1}", peer, disconnectInfo.Reason);

                _needConnection = false;
                var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con != null)
                {
                    con.DisconnectVoiceServer();
                }
                OnDisconnected?.Invoke(this, this);
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }

        private void ClientListener_NetworkReceiveUnconnectedEvent(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            
            if ((remoteEndPoint.Host != "127.0.0.1") && (remoteEndPoint.Host != "[::1]"))
            {
                GTMPVoicePlugin.Log("Refused from {0}", remoteEndPoint);
                return;
            }
            if (messageType != UnconnectedMessageType.BasicMessage)
            {
                GTMPVoicePlugin.Log("Refused {1} from {0}", remoteEndPoint, messageType);
                return;
            }

            _configuration = new VoicePaketConfig();
            _configuration.Deserialize(reader);
            if (_configuration.Hello != "GTMPVOICE" || _configuration.VoiceVersion != 1)
            {
                GTMPVoicePlugin.Log("Invalid configuration from {0}", remoteEndPoint);
                return;
            }

            _lastGTMPPing = DateTime.Now;
            var Connection = Client.GetFirstPeer();
            if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                return;
            GTMPVoicePlugin.Log("Accept Configuration from {0}", remoteEndPoint);
            
            if (_configuration.ClientVersionRequired > GTMPVoicePlugin.PluginVersion)
            {
                if (!_GotWarning)
                {
                    _GotWarning = true;
                    MessageBox.Show($"{GTMPVoicePlugin.Name} veraltet. Version {_configuration.ClientVersionRequired} wird benötigt. Bitte aktualisieren.", "GT-MP Voice Plugin Fehler");
                }
                return;
            }
            _lastGTMPPing = DateTime.Now;
            if ((Connection != null) && (Connection.ConnectionState != ConnectionState.Disconnected))
                return;
            GTMPVoicePlugin.Log("process VoicePaketConfig {0}", _configuration);
            _needConnection = true;
            StartServerConection();
        }

        private void StartServerConection()
        {
            var cp = new VoicePaketConnectServer() { Secret = _configuration.ServerSecret, Version = Assembly.GetExecutingAssembly().GetName().Version, ClientGUID = _configuration.ClientGUID };
            var nw = new NetDataWriter();
            cp.Serialize(nw);
            var oldPeer = Client.GetFirstPeer();
            if (oldPeer != null)
            {
                Client.DisconnectPeerForce(oldPeer);
                Thread.Sleep(200);
            }
            Client.Connect(_configuration.ServerIP, _configuration.ServerPort, nw);
        }

        private void ClientListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            GTMPVoicePlugin.Log("conrequest ");
        }

        void onVoiceSetup(VoicePaketSetup args, NetPeer peer)
        {
            try
            {
                GTMPVoicePlugin.Log("VoiceSetup {0}", args);
                _connectionInfo = args;
                var con = GTMPVoicePlugin.GetConnection(args.ServerGUID);
                if ((con != null) && con.IsInitialized && con.IsConnected)
                {
                    if (!GTMPVoicePlugin_ConnectionEstablished(con))
                    {
                        GTMPVoicePlugin.Log("Disconnecting Peer (TS Server not ready)");
                        Client.DisconnectPeer(peer);
                    }
                }
                else
                {
                    GTMPVoicePlugin.Log("Disconnecting Peer (TS Server not connected)");
                    Client.DisconnectPeer(peer);
                }
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }

        void onVoiceConfigureClient(VoicePaketConfigureClient args, NetPeer peer)
        {
            var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
            if (con != null)
            {
                con.SetCanLeaveIngameChannel(args.CanLeaveIngameChannel);
                con.ConnectVoiceServer(args.Nickname);
            }
        }

        void onVoiceMute(VoicePaketMute args, NetPeer peer)
        {
            var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
            if (con == null)
                return;
            if (args.IsMute)
            {
                GTMPVoicePlugin.Log("Mute {0}", args.Name);
                if (args.Name == "_ALL_")
                    con.Mute("_ALL_");
                else
                    con.GetClient(args.Name)?.Mute();
            }
            else
            {
                GTMPVoicePlugin.Log("Unmute {0}", args.Name);
                con.GetClient(args.Name)?.Unmute();
            }
        }

        void onVoiceUpdate(VoicePaketUpdate args, NetPeer peer)
        {
            try
            {
                var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                //GTMPVoicePlugin.Log("UpdatePosition {0}: {1} {2} {3}", args.PlayerName, args.Position, args.PositionIsRelative, args.VolumeModifier);
                var cl = con.GetClient(args.PlayerName);
                if (cl == null)
                {
                    GTMPVoicePlugin.Log("UpdatePosition {0}: {1} {2} {3} NOT FOUND", args.PlayerName, args.Position, args.PositionIsRelative, args.VolumeModifier);
                    return;
                }
                cl.UpdatePosition(args.Position, args.VolumeModifier, args.PositionIsRelative);
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }

        }

        void onVoiceBatchMute(VoicePaketBatchMute data, NetPeer peer)
        {
            try
            {
                GTMPVoicePlugin.Log("BatchMute {0}", data.Mutelist.Count);
                var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                data.Mutelist.ForEach(d => con.GetClient(d)?.Mute());
                con.DoMuteList(data.Mutelist.ToArray());
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }


        void onVoiceBatchUpdate(VoicePaketBatchUpdate data, NetPeer peer)
        {
            try
            {
                
                var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                if (con == null)
                    return;
                var mute = new HashSet<ushort>( con.GetChannel(con.IngameChannel).AllClients);
                mute.Remove(con.LocalClientId);
                var unmute = new HashSet<ushort>();
                if (data.Data.Length > 0)
                {
                    data.Data.ForEach(d =>
                    {
                        mute.Remove(d.ClientID);
                        unmute.Add(d.ClientID);
                        con.GetClient(d.ClientID)?.UpdatePosition(d.Position, d.VolumeModifier, false, false);
                    });
                }
                if (mute.SetEquals(con._MutedClients) && unmute.SetEquals(con._UnmutedClients))
                    return;
                GTMPVoicePlugin.Log("BatchUpdate {0}", data.Data.Length);
                con._UnmutedClients = new ConcurrentHashSet<ushort>(unmute);
                con._MutedClients = new ConcurrentHashSet<ushort>(mute);
                con.DoMuteActions();
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }

        void onVoiceCommand(VoicePaketCommand args, NetPeer peer)
        {
            try
            {
                GTMPVoicePlugin.Log("VoiceCommand {0} => {1}", args.Command, args.Data);
                if (args.Command == "DISCONNECT")
                {
                    _needConnection = false;
                    var Connection = Client.GetFirstPeer();
                    Connection?.Disconnect();
                    Connection = null;
                    var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.DisconnectVoiceServer();
                    }
                    OnDisconnected?.Invoke(this, this);
                    return;
                }
                if (args.Command == "SETNAME")
                {
                    var con = GTMPVoicePlugin.GetConnection(_connectionInfo.ServerGUID);
                    if (con != null)
                    {
                        con.ConnectVoiceServer(args.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                GTMPVoicePlugin.Log(ex.ToString());
            }
        }

        public void SendTalkStatusChanged(bool isTalking)
        {
            var Connection = Client.GetFirstPeer();
            if ((Connection != null) && (Connection.ConnectionState == ConnectionState.Connected))
                _netPacketProcessor.Send(Connection, new VoicePaketTalking(isTalking), DeliveryMethod.ReliableOrdered);
        }

        public void WriteNet(ConsoleColor color, string str, params object[] args)
        {
            Debug.WriteLine(String.Format(str, args));
        }
    }
}
