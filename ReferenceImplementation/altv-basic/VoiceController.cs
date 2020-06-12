using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Refs;
using PureVoice;
using PureVoice.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VoiceSupport
{
    public delegate void VoiceLoggingDelegate(LogLevel logLevel, string logMessage);

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Verbose,
        Debug,
        Trace
    }
    public class VoiceController
    {

        static System.Timers.Timer teamspeakTimer;
        static ConcurrentDictionary<int, Dictionary<string, VoiceLocationInformation>> PlayerHears = new ConcurrentDictionary<int, Dictionary<string, VoiceLocationInformation>>();
        static PureVoice.Server.VoiceServer _voiceServer;

        public static string VoiceServerIP = "";
        public static int VoiceServerPort = 4500;
        public static string VoiceServerSecret = "";
        public static string VoiceServerGUID = "";
        public static ulong VoiceDefaultChannel = 1;
        public static ulong VoiceIngameChannel = 115;
        public static int VoiceClientPort = 4239;
        public static string VoiceIngameChannelPassword = "egal";
        public static Version VoiceServerPluginVersion = new Version(0, 0, 0, 0);
        public static bool VoiceEnableLipSync = true;
        public static float VoiceMaxRange = 50.0f;
        public static bool VoiceForceVoice = true;

        public static event VoiceLoggingDelegate OnVoiceLogging;


        public VoiceController() : base()
        {

        }

        public virtual T GetVoiceSetting<T>(string name, T defaultValue)
        {
            return defaultValue;
        }


        public void StartVoice()
        {

            VoiceDefaultChannel = GetVoiceSetting<ulong>("voice_defaultchannel", VoiceDefaultChannel);
            VoiceIngameChannel = GetVoiceSetting<ulong>("voice_ingamechannel", VoiceIngameChannel);
            VoiceIngameChannelPassword = GetVoiceSetting<string>("voice_ingamechannelpassword", VoiceIngameChannelPassword);
            VoiceServerGUID = GetVoiceSetting<string>("voice_serverguid", VoiceServerGUID);
            VoiceServerIP = GetVoiceSetting<string>("voice_server", VoiceServerIP);
            VoiceServerPort = GetVoiceSetting<int>("voice_port", VoiceServerPort);
            VoiceServerSecret = GetVoiceSetting<string>("voice_secret", VoiceServerSecret);
            VoiceClientPort = GetVoiceSetting<int>("voice_clientport", VoiceClientPort);
            Version.TryParse(GetVoiceSetting<string>("voice_minpluginversion", VoiceServerPluginVersion.ToString()), out VoiceServerPluginVersion);
            VoiceEnableLipSync = GetVoiceSetting<bool>("voice_enablelipsync", VoiceEnableLipSync);
            VoiceMaxRange = GetVoiceSetting<float>("voice_maxrange", VoiceMaxRange);
            VoiceForceVoice = GetVoiceSetting<bool>("voice_forcevoice", VoiceForceVoice);

            _voiceServer = new PureVoice.Server.VoiceServer(VoiceServerPort, VoiceServerSecret, VoiceServerGUID, VoiceServerPluginVersion,
                VoiceDefaultChannel, VoiceIngameChannel, VoiceIngameChannelPassword, VoiceEnableLipSync);
            _voiceServer.VoiceClientConnected += _voiceServer_VoiceClientConnected;
            _voiceServer.VoiceClientDisconnected += _voiceServer_VoiceClientDisconnected;
            _voiceServer.VoiceClientOutdated += _voiceServer_VoiceClientOutdated;
            _voiceServer.VoiceClientMicrophoneStatusChanged += _voiceServer_VoiceClientMicrophoneStatusChanged;
            _voiceServer.VoiceClientSpeakersStatusChanged += _voiceServer_VoiceClientSpeakersStatusChanged;

            _voiceServer.EnableDistortion(VoiceClientDistortion.Radio, "Presets.ShortRangeRadio");
            _voiceServer.EnableDistortion(VoiceClientDistortion.Phone, "Presets.Phone");
            _voiceServer.EnableDistortion(VoiceClientDistortion.LongRangeRadio, "Presets.LongRangeradio");
            _voiceServer.EnableDistortion(VoiceClientDistortion.Megaphone, "Presets.Megaphone", "delay=100;amplify=4.0");

            if (VoiceEnableLipSync)
            {
                _voiceServer.VoiceClientTalking += _voiceServer_VoiceClientTalking;
            }

            teamspeakTimer = new System.Timers.Timer(200);
            teamspeakTimer.AutoReset = true;
            teamspeakTimer.Elapsed += (sender, args) =>
                {
                    try
                    {
                        UpdateTeamspeak();
                    }
                    catch (System.Threading.ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        OnVoiceLogging?.Invoke(LogLevel.Error, ex.ToString());
                    }
                };
            teamspeakTimer.Start();

            Alt.OnPlayerDead += Alt_OnPlayerDead;
            Alt.OnPlayerConnect += Alt_OnPlayerConnect;
            Alt.OnPlayerDisconnect += Alt_OnPlayerDisconnect;

        }


        private void Alt_OnPlayerConnect(IPlayer player, string reason)
        {
            PlayerHears[player.Id] = new Dictionary<string, VoiceLocationInformation>();
            Connect(player);
        }

        private void Alt_OnPlayerDisconnect(IPlayer player, string reason)
        {
            PlayerHears[player.Id] = new Dictionary<string, VoiceLocationInformation>();
            if (player.GetData<ushort>("VOICE_ID", out var vId))
                _voiceServer.SendCommand(vId, "DISCONNECT", "");
            player.DeleteData("VOICE_ID");
            player.DeleteData("VOICE_TS_ID");
        }

        private void Alt_OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            PlayerHears.TryRemove(player.Id, out var notused);
            if (player.GetData<ushort>("VOICE_ID", out var vId))
                _voiceServer.MutePlayer(vId, 0);
        }



        private IPlayer GetPlayerByConnectionId(List<IPlayer> all, ushort connectionId)
        {
            return all.FirstOrDefault(p => (p.GetData<ushort>("VOICE_ID", out var vId) && vId == connectionId));
        }

        private string GetTeamspeakID(IPlayer streamedPlayer)
        {
            if (streamedPlayer.GetData<string>("VOICE_TEAMSPEAK_IDENT", out var tId))
                return tId;
            return String.Empty;
        }

        private ushort GetTeamspeakClientID(IPlayer streamedPlayer)
        {
            if (streamedPlayer.GetData<ushort>("VOICE_TS_ID", out var tId))
                return tId;
            return 0;
        }

        // LipSync
        private void _voiceServer_VoiceClientTalking(ushort connectionId, bool isTalking)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = GetPlayerByConnectionId(threadSafeList.AllPlayers, connectionId);
                if (p != null)
                {
                    var pPos = p.Position;
                    var pls = threadSafeList.AllPlayers.Where(c => c.Position.Distance(pPos) < 20).ToList();
                    if (isTalking)
                        pls.ForEach(pt => pt.Emit("LIPSYNC", p, true));
                    else
                        pls.ForEach(pt => pt.Emit("LIPSYNC", p, false));
                }
            }
        }

        private void _voiceServer_VoiceClientOutdated(string clientGUID, Version hisVersion, Version ourVersion)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = threadSafeList.AllPlayers.FirstOrDefault(c => c.HardwareIdHash.ToString() == clientGUID);
                if (p != null)
                {
                    p.Kick("Please update your TS3 PureVoice Plugin.");
                }
            }
        }

        private void _voiceServer_VoiceClientDisconnected(ushort connectionId)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = GetPlayerByConnectionId(threadSafeList.AllPlayers, connectionId);
                if (p != null)
                {
                    p.DeleteData("VOICE_ID");
                    p.DeleteData("VOICE_TS_ID");
                    p.DeleteData("VOICE_TEAMSPEAK_IDENT");
                    if (VoiceForceVoice)
                        p.SetData("VOICE_TIMEOUT", DateTime.Now.AddMinutes(1));
                    Alt.Emit("PUREVOICE_CLIENT_DISCONNECTED", p);
                }
            }
        }

        private void _voiceServer_VoiceClientSpeakersStatusChanged(ushort connectionId, bool isMuted)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = GetPlayerByConnectionId(threadSafeList.AllPlayers, connectionId);
                if (p != null)
                {
                    OnVoiceLogging?.Invoke(LogLevel.Debug, $"{p.Name} Speakers muted {isMuted}");
                    Alt.Emit("PUREVOICE_CLIENT_SPEAKERSTATUS", p, isMuted);
                }
            }
        }

        private void _voiceServer_VoiceClientMicrophoneStatusChanged(ushort connectionId, bool isMuted)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = GetPlayerByConnectionId(threadSafeList.AllPlayers, connectionId);
                if (p != null)
                {
                    OnVoiceLogging?.Invoke(LogLevel.Debug, $"{p.Name} Mic muted {isMuted}");
                    Alt.Emit("PUREVOICE_CLIENT_MICROPHONESTATUS", p, isMuted);
                }
            }
        }

        private void _voiceServer_VoiceClientConnected(string clientGUID, string teamspeakID, ushort teamspeakClientID, string clientName, bool micMuted, bool speakersMuted)
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                var p = threadSafeList.AllPlayers.FirstOrDefault(c => c.HardwareIdHash.ToString() == clientGUID);
                if (p != null)
                {
                    OnVoiceLogging?.Invoke(LogLevel.Debug, String.Format("VoiceConnect {0} {1} {2} {3}", p.Name, teamspeakID, teamspeakClientID, clientGUID));
                    _voiceServer.ConfigureClient(teamspeakClientID, p.Name, false);
                    p.SetData("VOICE_ID", teamspeakClientID);
                    p.SetData("VOICE_TS_ID", teamspeakClientID);
                    p.SetData("VOICE_TEAMSPEAK_IDENT", teamspeakID);
                    Alt.Emit("PUREVOICE_CLIENT_CONNECTED", p, micMuted, speakersMuted);
                }
            }
        }


        /* Teamspeak Handling Functions */
        public static void Connect(IPlayer player)
        {
            player.Emit("PUREVOICE", VoiceServerIP, VoiceServerPort, VoiceServerSecret, player.HardwareIdHash.ToString(), VoiceServerPluginVersion.ToString(), VoiceClientPort);
            if (VoiceForceVoice)
                player.SetData("VOICE_TIMEOUT", DateTime.Now.AddMinutes(1));
        }

        public void UpdateTeamspeak()
        {
            using (var threadSafeList = new AltVPlayerList())
            {
                threadSafeList.AllPlayers.ForEach(p => { try { UpdateTeamspeakForUser(p, threadSafeList.AllPlayers); } catch (Exception ex) { OnVoiceLogging(LogLevel.Error, ex.ToString()); } });
            }
        }


        public void UpdateTeamspeakForUser(IPlayer player, List<IPlayer> allPlayers)
        {
            var playerPos = player.Position;
            var playerRot = player.Rotation;
            var rotation = Math.PI / 180 * (playerRot.Yaw * -1);
            var playerVehicle = player.Vehicle;

            if (!player.HasData("VOICE_ID"))
            {
                if (player.GetData<DateTime>("VOICE_TIMEOUT", out var dt) && dt < DateTime.Now)
                {
                    Alt.Emit("PUREVOICE_CLIENT_NOVOICE", player);
                    player.Kick("Please install the TS3 PureVoice Plugin");
                }
                return;
            }
            player.GetData<ushort>("VOICE_ID", out var targetId);

            var playersIHear = new Dictionary<string, VoiceLocationInformation>();

            // Players near me
            var inRangePlayers = allPlayers.Where(cl => (cl != player) && (cl.Position.Distance(playerPos) <= VoiceMaxRange) && (cl.Dimension == player.Dimension) && cl.HasData("VOICE_TS_ID")).ToList();

            if (inRangePlayers != null)
            {
                foreach (var streamedPlayer in inRangePlayers)
                {
                    var n = GetTeamspeakID(streamedPlayer);
                    if (streamedPlayer == player)
                    {
                        continue;
                    }

                    var streamedPlayerPos = streamedPlayer.Position;
                    var distance = playerPos.Distance(streamedPlayerPos);
                    var range = 5; // VoiceRange of streamed player in meters, maybe make this a player-Data?
                    var volumeModifier = 0f;

                    if (distance <= range)
                    {
                        if (Math.Abs(streamedPlayerPos.Z - playerPos.Z) > range)
                        {
                            continue;
                        }

                        var subPos = streamedPlayerPos - playerPos;

                        var x = subPos.X * Math.Cos(rotation) - subPos.Y * Math.Sin(rotation);
                        var y = subPos.X * Math.Sin(rotation) + subPos.Y * Math.Cos(rotation);

                        if (distance > 2)
                        {
                            volumeModifier = (float)(distance / range) * -10f;
                        }
                        if (volumeModifier > 0)
                        {
                            volumeModifier = 0;
                        }
                        volumeModifier = Math.Min(10.0f, Math.Max(-10.0f, volumeModifier));
                        if (!playersIHear.ContainsKey(n))
                            playersIHear[n] = new VoiceLocationInformation(GetTeamspeakClientID(streamedPlayer));
                        playersIHear[n].Update(new TSVector((float)(Math.Round(x * 1000) / 1000), (float)(Math.Round(y * 1000) / 1000), 0), (float)(Math.Round(volumeModifier * 1000) / 1000), false);
                    }
                }
            }


            // Players in same vehicle are always near
            if (playerVehicle != null)
            {
                var others = allPlayers.Where(p => (p != player) && p.Vehicle == playerVehicle).ToList();
                if (others != null)
                {
                    others.ForEach(p =>
                    {

                        var tsId = GetTeamspeakID(p);
                        if (!playersIHear.ContainsKey(tsId))
                            playersIHear[tsId] = new VoiceLocationInformation(GetTeamspeakClientID(p));
                        playersIHear[tsId].Update(new TSVector(0, 0, 0), 4, true);
                    });
                }
            }


            /* Radio Distortion */
            var rMode = player.GetRadioMode();
            if ((rMode != RadioModes.OFF && rMode != RadioModes.MUTE)) // !player.IsAdmin(1) && 
            {
                var radioVolume = player.GetSoundChannelVolume(SoundChannel.SOUND_CHANNEL_RADIO) * 4.0f;
                var radioChannels = RadioController.GetActiveChannels(player);
                radioChannels.ForEach(rc =>
                 {
                     rc.Speaking.ForEach(p =>
                      {
                          if (!p.IsDead() && (p != player))
                          {
                              if (debugVoice)
                              {
                                  logger.Debug($"{preface} Radio {p.Name}");
                              }
                              var tData = playersIHear.GetOrAdd(p.GetTeamspeakID(), new VoiceLocationInformation(p.GetTeamspeakClientID()));
                              tData.Update(new TSVector(-10, 0, 0), funkVolume, true);
                              tData.SignalQuality = rc.HasQualityChange ? RadioController.CalcSignalQuality(playerPos.DistanceTo(p.VoicePosition) / 1000.0f) : 1.0f;
                              tData.VoiceDistortion = (byte)rc.Distortion; // Converted from VoiceClientDistortion
                              logger.Warn($"Radio SignalQaul {player.Name} => {p.Name} {tData.VoiceDistortion} {tData.SignalQuality}");
                          }
                      });
                 });
            }

            
            PlayerHears[player.Id] = playersIHear;

            _voiceServer.SendUpdate(targetId, playersIHear.Count > 0 ? playersIHear.Values.ToList() : new List<VoiceLocationInformation>());
        }

        // ThreadSafe PlayerList
        private class AltVPlayerList : IDisposable
        {
            public List<IPlayer> AllPlayers;

            private List<PlayerRef> allRefs;

            public AltVPlayerList()
            {
                AllPlayers = Alt.Server.GetPlayers().ToList();
                allRefs = new List<PlayerRef>();
                AllPlayers.ForEach(p => { allRefs.Add(new PlayerRef(p)); });
            }

            public void Dispose()
            {
                allRefs.ForEach(p => p.Dispose());
                AllPlayers = null;
                allRefs = null;
            }
        }

    }

}
