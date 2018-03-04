using GTMPVoice.VoiceClient;
using GTMPVoice.VoiceClient.Model;
using LiteNetLib;
using LiteNetLib.Utils;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace GTMPVoice.Server
{
    public delegate void VoiceClientConnectedDelegate(string clientGUID,string teamspeakID,ushort teamspeakClientID, long connectionID);
    public delegate void VoiceClientOutdatedDelegate(string clientGUID, Version hisVersion, Version ourVersion);
    public delegate void VoiceClientDisconnectedDelegate(long connectionId);
    public delegate void VoiceClientTalkingDelegate(long connectionId,bool isTalking);

    public class VoiceServer : INetEventListener, INetLogger, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        private readonly VoiceIdentificationMethod _voiceIdentificationMethod = VoiceIdentificationMethod.Nickname;

        private ConcurrentDictionary<long, NetPeer> _connectedPeers = new ConcurrentDictionary<long, NetPeer>();

        public event VoiceClientConnectedDelegate VoiceClientConnected;
        public event VoiceClientOutdatedDelegate VoiceClientOutdated;
        public event VoiceClientDisconnectedDelegate VoiceClientDisconnected;
        public event VoiceClientTalkingDelegate VoiceClientTalking;

        private HashSet<ulong> _AllowedOutGameChannels = new HashSet<ulong>();

        public VoiceServer(int port, string secret, string serverGUID, Version requiredClientVersion, ulong defaultChannel, 
            ulong ingameChannel, string ingameChannelPassword,bool enableLipSync)
        {
            _server = new NetManager(this, 400)
            {
                ReuseAddress = true,
                UnsyncedEvents = true,
            };
            _requiredClientVersion = requiredClientVersion;
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
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketConnectClient, NetPeer>(OnVoiceClientConnect);
            if (!_server.Start(port))
            {
                throw new InvalidOperationException("Starting VoiceServer failed");
            }
            Logger.Debug($"VoiceServer started on port {port}");
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 5000;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Debug("Connected Clients: {0}/{1}",_server.GetPeersCount(ConnectionState.Connected), _server.PeersCount);
            /*
            foreach (var peer in _connectedPeers.Values)
            {
                Logger.Debug($"{peer} {peer.Ping} => {peer.PacketsCountInReliableOrderedQueue}");
            }*/
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

       

        public void RegisterOutGameChannels(params ulong[] channelIds)
        {
            _AllowedOutGameChannels = new HashSet<ulong>(channelIds);
        }
        
        internal void SendToAll(INetSerializable data)
        {
            if (_server != null && _server.IsRunning && _server.PeersCount > 0)
            {
                Logger.Debug("Sending {0}", data);
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
                       list.Add(new BatchDataRecord() { ClientID = item.ClientID, Position = item.Position, VolumeModifier = item.VolumeModifier });
                    }
                    p.Data = list.ToArray();
                    //Logger.Debug("SendUpdate {0} {1} Records => {2}",targetId, data.Count(),p.Data.Length);
                    SendTo(peer, p, DeliveryMethod.Sequenced);
                }
            }
        }

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
                    },DeliveryMethod.Sequenced);
                }
            }
        }


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

        public void SendCommand(long targetId, string command, string data)
        {
            if (_connectedPeers.TryGetValue(targetId,out var peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    SendTo(peer, new VoicePaketCommand() { Command = command, Data = data });
                }
            }
        }

        /*        public void SendUnconnected(INetSerializable data, string host,int port)
                {
                    if (_server != null && _server.IsRunning)
                    {
                        Logger.Debug("Sending {0} to {1}:{2}", data,host,port);
                        _netPacketProcessor.SendKnownTo(_server, data, host,port);
                    }
                }
        */
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
            Logger.Debug("NetworkError {0} : {1}", endPoint, socketErrorCode);
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
                catch (Exception ex)
                {
                    Logger.Warn(ex, "OnNetworkReceive {0}",peer.ToString());
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
                VoiceClientDisconnected?.Invoke(peer.ConnectId);
            }
        }

        void OnVoiceCommand(VoicePaketCommand args, NetPeer peer)
        {
            Logger.Debug("VoiceCommand {0}", args.Command);
        }

        void OnVoiceClientConnect(VoicePaketConnectClient args,NetPeer peer)
        {
            VoiceClientConnected?.Invoke(args.ClientGUID, args.TeamspeakID, args.TeamspeakClientID, peer.ConnectId);
        }

        void OnVoiceTalking(VoicePaketTalking args, NetPeer peer)
        {
            VoiceClientTalking?.Invoke(peer.ConnectId, args.IsTalking);
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
