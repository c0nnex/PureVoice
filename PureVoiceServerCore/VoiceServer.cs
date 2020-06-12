using PureVoice.VoiceClient;
using PureVoice.VoiceClient.Model;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Net;
using System.Net.Sockets;

namespace PureVoice.Server
{
    public delegate void VoiceClientConnectedDelegate(string clientGUID, string teamspeakID, ushort teamspeakClientID, string clientName, bool micMuted, bool speakersMuted);
    public delegate void VoiceClientOutdatedDelegate(string clientGUID, Version hisVersion, Version ourVersion);
    public delegate void VoiceClientDisconnectedDelegate(ushort clientId);
    public delegate void VoiceClientTalkingDelegate(ushort clientId, bool isTalking);
    public delegate void VoiceClientMuteStatusChangedDelegate(ushort clientId, bool isMuted);
    public delegate void VoiceClientCommandDelegate(ushort clientId, string command, string data);


    public enum VoiceClientDistortion : byte
    {
        None = 0,
        Radio,
        LongRangeRadio,
        Phone,
        Megaphone,
        Muffle,
        PitchUp,
        PitchDown,
        Echo
    }

    public enum VoiceClientSound : byte
    {
        None = 0,
        Radio_On = 1,
        Radio_Off = 2,
        SR_Start = 10,
        SR_End = 11,
        SR2_Start = 20,
        SR2_End = 21,
        LR_Start = 30,
        LR_End = 31,
        LR2_Start = 40,
        LR2_End = 41,
        PhoneNetworkDialIn = 90,
    }


    public class VoiceServer : INetEventListener, INetLogger, IDisposable
    {
        private static ILogger Logger;
        private static readonly Version _MinSupportedClientVersion = Version.Parse("0.3.23.537");

        private NetManager _server;
        private readonly VoicePaketProcessor _netPacketProcessor;

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

        private ConcurrentDictionary<ushort, NetPeer> _connectedPeers = new ConcurrentDictionary<ushort, NetPeer>();

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


        private VoicePaketSetup setupPaket = new VoicePaketSetup();

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
            ulong ingameChannel, string ingameChannelPassword, bool enableLipSync, ILogger logger = null)
        {
            if (logger != null)
                Logger = logger;

            _server = new NetManager(this)
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

            //IVoicePaketModel.Init();
            _netPacketProcessor = new VoicePaketProcessor();
            _netPacketProcessor.Subscribe<VoicePaketCommand, NetPeer>(OnVoiceCommand);
            _netPacketProcessor.Subscribe<VoicePaketTalking, NetPeer>(OnVoiceTalking);
            _netPacketProcessor.Subscribe<VoicePaketMicrophoneState, NetPeer>(OnMicrophoneStateChanged);
            _netPacketProcessor.Subscribe<VoicePaketSpeakersState, NetPeer>(OnSpeakerStateChanged);
            _netPacketProcessor.Subscribe<VoicePaketConnectClient, NetPeer>(OnVoiceClientConnect);
            setupPaket = new VoicePaketSetup()
            {
                DefaultChannel = _defaultChannel,
                IngameChannel = _ingameChannel,
                IngameChannelPassword = _ingameChannelPassword,
                ServerGUID = _serverGUID,
                EnableLipSync = _enableLipSync,

            };
        }

        /// <summary>
        /// Start accepting voice connections
        /// </summary>
        public void Start()
        {
            if (!_server.Start(_port))
            {
                throw new InvalidOperationException("Starting PureVoice VoiceServer failed");
            }
            Logger?.Info($"PureVoice VoiceServer started on port {_port}");
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 5000;
            timer.Start();
        }

#if NETCORE
        public static void SetMinLogLevel(LogLevel logLevel)
        {
            Logger?.SetMinLogLevel(logLevel);
        }
#endif
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_server == null)
                return;
#if DEBUG
            if ((_server != null) && (_server.ConnectedPeersCount > 0))
                Logger?.Trace("Connected Clients: {0}/{1}/{2}", _server.GetPeersCount(ConnectionState.Connected), _server.ConnectedPeersCount, _connectedPeers.Count);
#endif
            foreach (var kvp in _connectedPeers.ToList())
            {
                var peer = kvp.Value;
                if (peer.ConnectionState != ConnectionState.Connected)
                {
                    VoiceClientDisconnected?.Invoke(kvp.Key);
                    _connectedPeers.TryRemove(kvp.Key, out var unused);
                    continue;
                }
                /*
                if (peer.PacketsCountInReliableOrderedQueue > _maxOutQueueSize)
                {
                    Logger?.Debug($"{peer.EndPoint} {peer.Ping} => {peer.PacketsCountInReliableOrderedQueue} queuesize exceeded disconnecting");
                    VoiceClientDisconnected?.Invoke(peer.GetClientID());
                    _connectedPeers.TryRemove(peer.GetClientID(), out var unused);
                    _server?.DisconnectPeerForce(peer);
                }*/
            }
        }

        public void Stop()
        {
            timer.Stop();
            if (_server != null)
            {
                _server.DisconnectAll();
                _server.Stop();
            }
            _server = null;
        }

        /// <summary>
        /// Enables a given Voice Distortion
        /// </summary>
        /// <param name="distortion"></param>
        /// <param name="highpassCutoff"></param>
        /// <param name="lowpassCutoff"></param>
        /// <param name="gain"></param>
        /// <param name="enableNoise"></param>
        /// <param name="filterSettings"></param>
        public void EnableDistortion(VoiceClientDistortion distortion, string filterName = "Presets.Default", string filterSettings = null)
        {
            setupPaket.GetDistortionSettings().RemoveAll(vs => vs.VoiceDistortion == (byte)distortion);
            var nSet = new VoiceDistortionSetting((byte)distortion);
            nSet.AddFilter(filterName, filterSettings);
            setupPaket.GetDistortionSettings().Add(nSet);
        }

        /// <summary>
        /// Enables a given Voice Distortion
        /// </summary>
        /// <param name="distortion"></param>
        /// <param name="highpassCutoff"></param>
        /// <param name="lowpassCutoff"></param>
        /// <param name="gain"></param>
        /// <param name="enableNoise"></param>
        /// <param name="filterSettings"></param>
        public void EnableDistortion(byte distortion, string filterName = "Presets.Default", string filterSettings = null)
        {
            setupPaket.GetDistortionSettings().RemoveAll(vs => vs.VoiceDistortion == (byte)distortion);
            var nSet = new VoiceDistortionSetting(distortion);
            nSet.AddFilter(filterName, filterSettings);
            setupPaket.GetDistortionSettings().Add(nSet);
        }

        /// <summary>
        /// Adds a Filter/Effect to a distortion
        /// </summary>
        /// <param name="distortion"></param>
        /// <param name="filterName"></param>
        /// <param name="filterSettings"></param>
        public void AddDistortionFilter(VoiceClientDistortion distortion, string filterName = "Filters.Default", string filterSettings = null)
        {
            var curSetup = setupPaket.GetDistortionSettings().FirstOrDefault(d => d.VoiceDistortion == (byte)distortion);
            if (curSetup == null)
            {
                EnableDistortion(distortion, filterName, filterSettings);
                return;
            }
            curSetup.AddFilter(filterName, filterSettings);
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
            if (_server != null && _server.IsRunning && _server.ConnectedPeersCount > 0)
            {
                Logger?.Trace("Sending {0}", data);
                _netPacketProcessor.SendNetSerializable(_server, data, DeliveryMethod.ReliableOrdered);
            }
        }

        internal void SendTo(NetPeer peer, INetSerializable data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (_server != null && _server.IsRunning && _server.ConnectedPeersCount > 0)
            {
                // Logger?.Debug("Sending {0}", data);
                _netPacketProcessor.SendNetSerializable(peer, data, DeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>
        /// Mute a client on anaother client
        /// </summary>
        /// <param name="targetId">the client where to mute someone</param>
        /// <param name="playerName">the name of the player to mute</param>
        public void MutePlayer(ushort targetId, ushort clientId)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    _netPacketProcessor.Send(peer, new VoicePaketMute()
                    {
                        ClientID = clientId,
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
        public void SendUpdate(ushort targetId, IEnumerable<VoiceLocationInformation> data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    if (data.Count() == 0 && !peer.GetPeerData().LastBatchContainedData) // No send as last update did not conatain any data
                        return;
                    peer.GetPeerData().LastBatchContainedData = data.Count() != 0;
                    var p = new VoicePaketBatchUpdate();

                    List<BatchDataRecord> list = new List<BatchDataRecord>();
                    foreach (var item in data)
                    {
                        list.Add(new BatchDataRecord() { ClientID = item.ClientID, Position = item.Position, VolumeModifier = item.VolumeModifier, SignalQuality = item.SignalQuality, VoiceDistortion = item.VoiceDistortion });
                    }
                    p.BatchData = list;
                    //Logger?.Trace("SendUpdate {0} {1} Records => {2}",targetId, data.Count(),p.Data.Length);
                    _netPacketProcessor.SendNetSerializable(peer, p, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        /// <summary>
        /// Send a voice update to a client
        /// </summary>
        /// <param name="targetId">target client</param>
        /// <param name="data">new voice data</param>
        public void SendUpdate(ushort targetId, VoiceLocationInformation data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    // Logger?.Debug($"SendUpdate {targetId} {data.Name} {data.Position}");
                    _netPacketProcessor.Send(peer, new VoicePaketUpdate()
                    {
                        ClientID = data.ClientID,
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
        public void SendUpdate(ushort targetId, ushort clientId, TSVector position, float volumeModifier, bool positionIsRelative)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    _netPacketProcessor.Send(peer, new VoicePaketUpdate()
                    {
                        ClientID = clientId,
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
        public void ConfigureClient(ushort targetId, string nickName, bool canLeaveIngameChannel)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    _netPacketProcessor.Send(peer, new VoicePaketConfigureClient()
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
        public void SendCommand(ushort targetId, string command, string data)
        {
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    _netPacketProcessor.Send(peer, new VoicePaketCommand() { Command = command, Data = data });
                }
            }
        }

        public void PlayClientSound(ushort targetId, VoiceClientSound playSound = VoiceClientSound.None)
        {
            PlayClientSound(targetId, playSound.ToString());
        }

        public void PlayClientSound(ushort targetId, string playSound = "")
        {
            Logger.Debug($"PlayClientSound target {targetId} playSound {playSound}");
            if (_connectedPeers.TryGetValue(targetId, out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    _netPacketProcessor.Send(peer, new VoicePaketUpdateDistortion() { ClientID = targetId, VoiceDistortion = 0, VoiceSound = playSound });
                }
            }
        }

        public void SetDistortion(IEnumerable<ushort> listeners, ushort targetId, VoiceClientDistortion voiceDistortion, VoiceClientSound playSound = VoiceClientSound.None, bool playForTarget = true)
        {
            SetDistortion(listeners, targetId, voiceDistortion, playSound.ToString(), playForTarget);
        }

        public void SetDistortion(IEnumerable<ushort> listeners, ushort targetId, VoiceClientDistortion voiceDistortion, string playSound = "", bool playForTarget = true)
        {
            var foundTarget = false;
            Logger.Debug($"SetDistortion listeners {String.Join(",", listeners)} target {targetId} distortion {voiceDistortion} playSound {playSound}");
            foreach (var item in listeners)
            {
                if (item == targetId)
                    foundTarget = true;
                if (_connectedPeers.TryGetValue(item, out var peer))
                {
                    if (peer.ConnectionState == ConnectionState.Connected)
                    {
                        _netPacketProcessor.Send(peer, new VoicePaketUpdateDistortion() { ClientID = targetId, VoiceDistortion = (byte)voiceDistortion, VoiceSound = playSound });
                    }
                }
            }
            if (!String.IsNullOrEmpty(playSound) && playForTarget && !foundTarget)
            {
                PlayClientSound(targetId, playSound);
            }
        }


        public void OnConnectionRequest(ConnectionRequest request)
        {
            var connectPaket = new VoicePaketConnectServer();
            connectPaket.Deserialize(request.Data);
            if (connectPaket.Secret != _secret)
            {
                Logger?.Warn("Unauthorized connect from {0}/{1} : Wrong secret", request.RemoteEndPoint, connectPaket.ClientGUID);
                request.Reject();
                return;
            }
            if (connectPaket.Version < _requiredClientVersion)
            {
                Logger?.Warn("Unauthorized connect from {0}/{1} : Wrong Version {2}", request.RemoteEndPoint, connectPaket.ClientGUID, connectPaket.Version);
                request.Reject();
                VoiceClientOutdated?.Invoke(connectPaket.ClientGUID, connectPaket.Version, _requiredClientVersion);
                return;
            }
            Logger?.Debug("Connection accepted {0}/{1}", request.RemoteEndPoint, connectPaket.ClientGUID);
            request.Accept();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            Logger?.Warn("NetworkError {0} : {1}", endPoint, socketErrorCode);
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Logger?.Debug("LatencyUpdate {0} : {1}", peer.EndPoint, latency);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    _netPacketProcessor.ReadAllPackets(reader, peer);
                }
                catch (ParseException ex)
                {
                    Logger?.Warn("OnNetworkReceive {0}: {1} Data {2}", peer.EndPoint.ToString(), ex.Message, reader.RawData.HexDump());
                    peer.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger?.Warn(ex, "OnNetworkReceive {0}", peer.EndPoint.ToString());
                    peer.Disconnect();
                }
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnPeerConnected(NetPeer peer)
        {
            Logger?.Debug("Client connected: {0}", peer.EndPoint);


            setupPaket.AllowedOutGameChannels = _AllowedOutGameChannels.ToArray();

            _netPacketProcessor.Send(peer, setupPaket, DeliveryMethod.ReliableOrdered);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (_connectedPeers.TryRemove(peer.GetClientID(), out var unused))
            {
                _server.DisconnectPeerForce(peer);
                Thread.Sleep(200);
                Logger?.Debug("Client disconnected: {0} : {1}", peer.EndPoint, disconnectInfo.Reason);
                try
                {
                    VoiceClientDisconnected?.Invoke(peer.GetClientID());
                }
                catch (Exception ex)
                {
                    Logger?.Warn(ex);
                }
            }
            else
            {
                Logger?.Warn("unknown Client disconnected: {0} : {1}", peer.EndPoint, disconnectInfo.Reason);
            }
        }

        void OnVoiceCommand(VoicePaketCommand args, NetPeer peer)
        {
            Logger?.Debug("VoiceCommand {0}", args.Command);
            VoiceClientCommand?.Invoke(peer.GetClientID(), args.Command, args.Data);
        }

        void OnVoiceClientConnect(VoicePaketConnectClient args, NetPeer peer)
        {
            try
            {
                Logger?.Trace("OnVoiceClientConnect");
                peer.SetClientID(args.TeamspeakClientID);
                _connectedPeers[args.TeamspeakClientID] = peer;
                VoiceClientConnected?.Invoke(args.ClientGUID, args.TeamspeakID, args.TeamspeakClientID, args.TeamspeakClientName, args.MicrophoneMuted, args.SpeakersMuted);
            }
            catch (Exception ex)
            {
                Logger?.Warn(ex);
            }
        }

        void OnVoiceTalking(VoicePaketTalking args, NetPeer peer)
        {
            try
            {
                if (_enableLipSync)
                    VoiceClientTalking?.Invoke(peer.GetClientID(), args.IsTalking);
            }
            catch (Exception ex)
            {
                Logger?.Warn(ex);
            }
        }

        private void OnSpeakerStateChanged(VoicePaketSpeakersState args, NetPeer peer)
        {
            try
            {
                VoiceClientSpeakersStatusChanged?.Invoke(peer.GetClientID(), args.IsMuted);
            }
            catch (Exception ex)
            {
                Logger?.Warn(ex);
            }
        }

        private void OnMicrophoneStateChanged(VoicePaketMicrophoneState args, NetPeer peer)
        {
            try
            {
                VoiceClientMicrophoneStatusChanged?.Invoke(peer.GetClientID(), args.IsMuted);
            }
            catch (Exception ex)
            {
                Logger?.Warn(ex);
            }
        }

        void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
        {
            if (level == NetLogLevel.Error)
                Logger?.Warn(str, args);
            else
                Logger?.Debug(str, args);
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


    public class VoiceLiteData
    {
        public ushort ClientID = 0; // Teamspeakcleint ID of peer
        public bool LastBatchContainedData = false; // For optimizing performance
    }

    public static class VoiceLiteNetExtensions
    {
        public static VoiceLiteData GetPeerData(this NetPeer peer)
        {
            if (peer.Tag == null)
                peer.Tag = new VoiceLiteData();

            return peer.Tag as VoiceLiteData;
        }

        public static ushort GetClientID(this NetPeer peer)
        {
            if (peer.Tag == null)
                return 0;
            return peer.GetPeerData().ClientID;
        }
        public static void SetClientID(this NetPeer peer, ushort id)
        {
            peer.GetPeerData().ClientID = id;
        }
    }
}
