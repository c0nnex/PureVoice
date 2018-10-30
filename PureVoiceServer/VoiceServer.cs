using PureVoice.VoiceClient;
using PureVoice.VoiceClient.Model;
using LiteNetLib;
using LiteNetLib.Utils;
#if !NETCORE
using NLog;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace PureVoice.Server
{
    public delegate void VoiceClientConnectedDelegate(string clientGUID, string teamspeakID, ushort teamspeakClientID, long connectionID, string clientName, bool micMuted, bool speakersMuted);
    public delegate void VoiceClientOutdatedDelegate(string clientGUID, Version hisVersion, Version ourVersion);
    public delegate void VoiceClientDisconnectedDelegate(long connectionId);
    public delegate void VoiceClientTalkingDelegate(long connectionId, bool isTalking);
    public delegate void VoiceClientMuteStatusChangedDelegate(long connectionId, bool isMuted);
    public delegate void VoiceClientCommandDelegate(long connectionId, string command, string data);

    public enum LogLevel
    {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR
    }

    public class VoiceServer : INetEventListener, INetLogger, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Version _MinSupportedClientVersion = Version.Parse("0.2.840.625");

        private NetManager _server;
        private readonly VoicePaketProcessor _netPacketProcessor = new VoicePaketProcessor();

        private readonly string _secret;
        private readonly Version _requiredClientVersion = new Version(0, 0, 0, 0);

        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private readonly int _port = 4244;
        private readonly string _serverGUID;
        private readonly ulong _defaultChannel;
        private readonly ulong _ingameChannel;
        private readonly string _ingameChannelPassword;
        private readonly bool _enableLipSync;
        private readonly int _maxOutQueueSize = 10;

        private ConcurrentDictionary<long, NetPeer> _connectedPeers = new ConcurrentDictionary<long, NetPeer>();

        /// <summary>
        /// Event when a new client connected to us
        /// </summary>
        public event VoiceClientConnectedDelegate VoiceClientConnected;
        /// <summary>
        /// Event when a client with an outdaten plugin version connected
        /// </summary>
        public event VoiceClientOutdatedDelegate VoiceClientOutdated;
        /// <summary>
        /// A client discronnected
        /// </summary>
        public event VoiceClientDisconnectedDelegate VoiceClientDisconnected;
        /// <summary>
        /// The "isTalking" State of a client changed (only if LipSync is enabled)
        /// </summary>
        public event VoiceClientTalkingDelegate VoiceClientTalking;
        /// <summary>
        /// The Mute-State of the mircophjone of a client changed
        /// </summary>
        public event VoiceClientMuteStatusChangedDelegate VoiceClientMicrophoneStatusChanged;
        /// <summary>
        /// The Mute-State of the speakers of a client changed
        /// </summary>
        public event VoiceClientMuteStatusChangedDelegate VoiceClientSpeakersStatusChanged;
        /// <summary>
        /// Received a Command from the TS3 plugin
        /// </summary>
        public event VoiceClientCommandDelegate VoiceClientCommand;

        private HashSet<ulong> _AllowedOutGameChannels = new HashSet<ulong>();

        /// <summary>
        /// Create a new VoiceServer
        /// </summary>
        /// <param name="port">UDP Port to listen on</param>
        /// <param name="secret">Our sharted secret</param>
        /// <param name="serverGUID">a unique identifier of the Server (for TS3 the TS3-Server-Instance Identifier)</param>
        /// <param name="requiredClientVersion">minimum plugin version required</param>
        /// <param name="defaultChannel">a default channel to join if client cannot rejoin previous channel</param>
        /// <param name="ingameChannel">the ingame channel id</param>
        /// <param name="ingameChannelPassword">(oprion) password for the ingame channel.Leave empty for none</param>
        /// <param name="enableLipSync">enable talking-change events fopr lipsync</param>
        public VoiceServer(int port, string secret, string serverGUID, Version requiredClientVersion, ulong defaultChannel,
            ulong ingameChannel, string ingameChannelPassword, bool enableLipSync)
        {
            _server = new NetManager(this, 400)
            {
                ReuseAddress = true,
                UnsyncedEvents = true,
            };
            _requiredClientVersion = requiredClientVersion;
            if (_requiredClientVersion < _MinSupportedClientVersion)
                _requiredClientVersion = _MinSupportedClientVersion;
            _secret = secret;
            _port = port;
            _serverGUID = serverGUID;
            _defaultChannel = defaultChannel;
            _ingameChannel = ingameChannel;
            _ingameChannelPassword = ingameChannelPassword;
            _enableLipSync = enableLipSync;

            IVoicePaketModel.Init();
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketCommand, NetPeer>(OnVoiceCommand);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketTalking, NetPeer>(OnVoiceTalking);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketMicrophoneState, NetPeer>(OnMicrophoneStateChanged);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketSpeakersState, NetPeer>(OnSpeakerStateChanged);
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketConnectClient, NetPeer>(OnVoiceClientConnect);

            if (!_server.Start(port))
            {
                throw new InvalidOperationException("Starting VoiceServer failed");
            }
            Logger.Info($"VoiceServer started on port {port}");
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 5000;
            timer.Start();
        }

        public static void SetMinLogLevel(LogLevel logLevel)
        {
            Logger?.SetMinLogLevel(logLevel);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_server.PeersCount > 0)
                Logger.Trace("Connected Clients: {0}/{1}/{2}", _server.GetPeersCount(ConnectionState.Connected), _server.PeersCount, _connectedPeers.Count);

            foreach (var kvp in _connectedPeers.ToList())
            {
                var peer = kvp.Value;
                if (peer.ConnectionState != ConnectionState.Connected)
                {
                    VoiceClientDisconnected?.Invoke(kvp.Key);
                    _connectedPeers.TryRemove(kvp.Key, out var unused);
                    continue;
                }

                if (peer.PacketsCountInReliableOrderedQueue > _maxOutQueueSize)
                {
                    Logger.Debug($"{peer.EndPoint} {peer.Ping} => {peer.PacketsCountInReliableOrderedQueue} queuesize exceeded disconnecting");
                    VoiceClientDisconnected?.Invoke(peer.ConnectId);
                    _connectedPeers.TryRemove(peer.ConnectId, out var unused);
                    _server.DisconnectPeerForce(peer);
                }
            }
        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.DisconnectAll();
                _server.Stop();
            }
            _server = null;
        }

        /// <summary>
        /// Register channelids that are allowed to join when being in game (e.g. Support channels)
        /// </summary>
        /// <param name="channelIds"></param>
        public void RegisterOutGameChannels(params ulong[] channelIds)
        {
            _AllowedOutGameChannels = new HashSet<ulong>(channelIds);
        }

        internal void SendToAll(INetSerializable data)
        {
            if (_server != null && _server.IsRunning && _server.PeersCount > 0)
            {
                Logger.Trace("Sending {0}", data);
                _netPacketProcessor.SendKnown(_server, data, DeliveryMethod.ReliableOrdered);
            }
        }

        internal void SendTo(NetPeer peer, INetSerializable data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (_server != null && _server.IsRunning && _server.PeersCount > 0)
            {
                // Logger.Debug("Sending {0}", data);
                _netPacketProcessor.SendKnown(peer, data, DeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>
        /// Mute a client on anaother client
        /// </summary>
        /// <param name="targetId">the client where to mute someone</param>
        /// <param name="playerName">the name of the player to mute</param>
        public void MutePlayer(long targetId, string playerName)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    SendTo(peer, new VoicePaketMute()
                    {
                        Name = playerName,
                        IsMute = true
                    });
                }
            }
        }

        /// <summary>
        /// Send a voice update to a client
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="data">new voice data</param>
        public void SendUpdate(long targetId, IEnumerable<VoiceLocationInformation> data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    var p = new VoicePaketBatchUpdate();

                    List<BatchDataRecord> list = new List<BatchDataRecord>();
                    foreach (var item in data)
                    {
                        list.Add(new BatchDataRecord() { ClientID = item.ClientID, Position = item.Position, VolumeModifier = item.VolumeModifier,
                            ClientName = item.ClientID == 0 ? item.Name  : String.Empty });
                    }
                    p.Data = list.ToArray();
                    //Logger.Debug("SendUpdate {0} {1} Records => {2}",targetId, data.Count(),p.Data.Length);
                    SendTo(peer, p, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        /// <summary>
        /// Send a voice update to a client
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="data">new voice data</param>
        public void SendUpdate(long targetId, VoiceLocationInformation data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    // Logger.Debug($"SendUpdate {targetId} {data.Name} {data.Position}");
                    SendTo(peer, new VoicePaketUpdate()
                    {
                        PlayerName = data.Name,
                        Position = data.Position,
                        PositionIsRelative = data.IsRelative,
                        VolumeModifier = data.VolumeModifier,
                    }, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        /// <summary>
        ///  Send a voice update to a client
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="playerName">affected client</param>
        /// <param name="position">3D position</param>
        /// <param name="volumeModifier">vol modifier (-10.0 ... +10.0)</param>
        /// <param name="positionIsRelative">position is relative to the target client</param>
        public void SendUpdate(long targetId, string playerName, TSVector position, float volumeModifier, bool positionIsRelative)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    SendTo(peer, new VoicePaketUpdate()
                    {
                        PlayerName = playerName,
                        Position = position,
                        PositionIsRelative = positionIsRelative,
                        VolumeModifier = volumeModifier,
                    }, DeliveryMethod.Sequenced);
                }
            }
        }

        /// <summary>
        /// Set client configuration
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="nickName">new nick name</param>
        /// <param name="canLeaveIngameChannel">if the client is allowed to leave the ongame channel</param>
        public void ConfigureClient(long targetId, string nickName, bool canLeaveIngameChannel)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    SendTo(peer, new VoicePaketConfigureClient()
                    {
                        Nickname = nickName,
                        CanLeaveIngameChannel = canLeaveIngameChannel
                    });
                }
            }
        }

        /// <summary>
        /// Send a voice command to the client
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="command">command</param>
        /// <param name="data">optional command data</param>
        public void SendCommand(long targetId, string command, string data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    SendTo(peer, new VoicePaketCommand() { Command = command, Data = data });
                }
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            var connectPaket = new VoicePaketConnectServer();
            connectPaket.Deserialize(request.Data);
            if (connectPaket.Secret != _secret)
            {
                Logger.Warn("Unauthorized connect from {0} : Wrong secret", request.RemoteEndPoint);
                request.Reject();
                return;
            }
            if (connectPaket.Version < _requiredClientVersion)
            {
                Logger.Warn("Unauthorized connect from {0} : Wrong Version {1}", request.RemoteEndPoint, connectPaket.Version);
                request.Reject();
                VoiceClientOutdated?.Invoke(connectPaket.ClientGUID, connectPaket.Version, _requiredClientVersion);
                return;
            }
            Logger.Debug("Connection accepted {0}", request.RemoteEndPoint);
            request.Accept();
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Logger.Warn("NetworkError {0} : {1}", endPoint, socketErrorCode);
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Logger.Debug("LatencyUpdate {0} : {1}", peer.EndPoint, latency);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    _netPacketProcessor.ReadAllPackets(reader, peer);
                }
                catch (ParseException ex)
                {
                    Logger.Warn("OnNetworkReceive {0}: {1} Data {2}", peer.EndPoint.ToString(), ex.Message, reader.Data.HexDump());
                    peer.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "OnNetworkReceive {0}", peer.EndPoint.ToString());
                    peer.Disconnect();
                }
            }
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnPeerConnected(NetPeer peer)
        {
            Logger.Debug("Client connected: {0}", peer.EndPoint);
            _connectedPeers[peer.ConnectId] = peer;

            var setupPaket = new VoicePaketSetup()
            {
                DefaultChannel = _defaultChannel,
                IngameChannel = _ingameChannel,
                IngameChannelPassword = _ingameChannelPassword,
                ServerGUID = _serverGUID,
                EnableLipSync = _enableLipSync,
                AllowedOutGameChannels = _AllowedOutGameChannels.ToArray()
            };
            _netPacketProcessor.Send(peer, setupPaket, DeliveryMethod.ReliableOrdered);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (_connectedPeers.TryRemove(peer.ConnectId, out var unused))
            {
                _server.DisconnectPeerForce(peer);
                Thread.Sleep(200);
                Logger.Debug("Client disconnected: {0} : {1}", peer.EndPoint, disconnectInfo.Reason);
                try
                {
                    VoiceClientDisconnected?.Invoke(peer.ConnectId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }
        }

        void OnVoiceCommand(VoicePaketCommand args, NetPeer peer)
        {
            Logger.Debug("VoiceCommand {0}", args.Command);
            VoiceClientCommand?.Invoke(peer.ConnectId, args.Command, args.Data);
        }

        void OnVoiceClientConnect(VoicePaketConnectClient args, NetPeer peer)
        {
            try
            {
                VoiceClientConnected?.Invoke(args.ClientGUID, args.TeamspeakID, args.TeamspeakClientID, peer.ConnectId, args.TeamspeakClientName, args.MicrophoneMuted, args.SpeakersMuted);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        void OnVoiceTalking(VoicePaketTalking args, NetPeer peer)
        {
            try
            {
                if (_enableLipSync)
                    VoiceClientTalking?.Invoke(peer.ConnectId, args.IsTalking);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        private void OnSpeakerStateChanged(VoicePaketSpeakersState args, NetPeer peer)
        {
            try
            {
                VoiceClientSpeakersStatusChanged?.Invoke(peer.ConnectId, args.IsMuted);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        private void OnMicrophoneStateChanged(VoicePaketMicrophoneState args, NetPeer peer)
        {
            try
            {
                VoiceClientMicrophoneStatusChanged?.Invoke(peer.ConnectId, args.IsMuted);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        void INetLogger.WriteNet(ConsoleColor color, string str, params object[] args)
        {
            if (color == ConsoleColor.Red)
                Logger.Warn(str, args);
            else
                Logger.Debug(str, args);
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
#endregion
    }
}
