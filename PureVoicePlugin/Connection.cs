﻿using PureVoice.DSP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TeamSpeakPlugin;

namespace PureVoice
{
    internal class Connection : TeamSpeakBase
    {
        public delegate Client GetClientDelegate(string nameOrGuid);

        public ulong ID;
        public string ServerHashString;
        private string gUID;
        public ConnectionStatusEnum Status = ConnectionStatusEnum.STATUS_DISCONNECTED;
        public ConcurrentDictionary<ushort, Client> ClientMap = new ConcurrentDictionary<ushort, Client>();
        public ConcurrentDictionary<ulong, Channel> ChannelMap = new ConcurrentDictionary<ulong, Channel>();

        public ushort LocalClientId = 0;
        public string LocalClientName = "";
        public Client LocalClient = null;

        public ulong DefaultChannel => _voiceSetup.DefaultChannel;
        public ulong IngameChannel => _voiceSetup.IngameChannel;
        public string IngameChannelPassword => _voiceSetup.IngameChannelPassword;
        public bool EnableLipSync => _voiceSetup.EnableLipSync;
        public bool CanLeaveIngameChannel { get; private set; }
        public HashSet<ulong> AllowedOutGameChannels = new HashSet<ulong>();

        public bool IsVoiceEnabled = false;

        public bool IsConnected => ID != 0 && Status == ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED;

        public string GUID { get => gUID; set { gUID = value; ServerHashString = VoicePlugin.Hash(value).ToString("X"); } }

        public bool IsInitialized = false;

        private VoiceClient.Model.VoicePaketSetup _voiceSetup = new VoiceClient.Model.VoicePaketSetup();

        public ConcurrentHashSet<ushort> _MutedClients = new ConcurrentHashSet<ushort>();
        public ConcurrentHashSet<ushort> _UnmutedClients = new ConcurrentHashSet<ushort>();

        private Timer _MuteTimer = new Timer();
        private Dictionary<byte, VoiceFilterChain> _VoiceFilters = new Dictionary<byte, VoiceFilterChain>();



        public Connection(ulong id)
        {
            ID = id;
            _MuteTimer.Interval = 5000;
            _MuteTimer.Elapsed += _MuteTimer_Elapsed; ;

        }

        private void _MuteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var c = GetChannel(IngameChannel);
            if (c != null)
            {
                c.UpdateClientList();
                var ct = _MutedClients.Count;
                c.AllClients.ForEach(cl => ConditionalMute(cl));
                if (_MutedClients.Count != ct)
                    DoMuteActions();
            }
        }

        private void ConditionalMute(ushort cl)
        {
            if (cl != LocalClientId && !_UnmutedClients.Contains(cl))
                _MutedClients.Add(cl);
        }

        public Channel GetChannel(ulong id)
        {
            return ChannelMap.GetOrAdd(id, _ => new Channel(this, id));
        }

        public Client GetClient(ushort id)
        {
            return ClientMap.GetOrAdd(id, _ => new Client(this, id));
        }

        public Client GetClient(string guid)
        {
            Client retVal = GetClientByGUID(guid);
            if (retVal == null)
            {
                Log("GetClient {0} => NOT FOUND", guid);
            }
            return retVal;
        }

        public ushort GetClientId(string guid)
        {
            Client retVal = GetClientByGUID(guid);
            if (retVal == null)
            {
                Log("GetClient {0} => NOT FOUND", guid);
                return 0;
            }
            return retVal.ID;
        }

        internal void Init()
        {
            try
            {
                if (!IsInitialized && Status == ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED)
                {
                    Log("INIT");
                    Update();
                    Check(Functions.getClientID(ID, ref LocalClientId));
                    var localClient = GetClient(LocalClientId);
                    localClient.IsLocalClient = true;
                    localClient.Update();
                    LocalClient = localClient;
                    LocalClientName = localClient.Name;
                    Log("Local client id {0}", LocalClientId);
                    
                    IntPtr retVal = IntPtr.Zero;
                    Check(Functions.getClientList(ID, ref retVal));
                    var newClients = TeamSpeakBase.ReadShortIDList<Client>(retVal, GetClient);
                    Log($"{newClients?.Count} Clients");
                    if (newClients != null)
                    {
                        newClients.ForEach(cl => cl.Update());
                        var cu = newClients.Select(cl => cl.ID).ToArray();
                        Log($"{cu.Length} Unmute Clients");
                        DoUnmuteList(cu);
                    }
                    LocalClient.UpdateChannel();
                    Check(Functions.systemset3DListenerAttributes(ID, new TSVector(), new TSVector(), new TSVector()));
                    IsInitialized = true;
                    Log("ENDINIT");
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public Client GetClientByNameOrGuid(string nameOrGuid)
        {
            return ClientMap.Values.FirstOrDefault(cl => cl.Name == nameOrGuid || cl.GUID == nameOrGuid);
        }

        public Client GetClientByNickname(string name)
        {
            return ClientMap.Values.FirstOrDefault(cl => cl.Name == name);
        }

        public Client GetClientByGUID(string guid)
        {
            return ClientMap.Values.FirstOrDefault(cl => cl.GUID == guid);
        }

        internal void RemoveClient(ushort id)
        {
            if (ClientMap.TryRemove(id, out var client))
            {
                if (client.ChannelID != 0)
                    GetChannel(client.ChannelID)?.RemoveClient(id);
            }
            RemoveMute(id);
        }

        public void Update()
        {
            try
            {
                Log("UPDATE");
                GUID = GetServerVarString(VirtualServerProperties.VIRTUALSERVER_UNIQUE_IDENTIFIER);
                Log("ENDUPDATE");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

        }

        public string GetServerVarString(VirtualServerProperties flag)
        {
            IntPtr retVal = IntPtr.Zero;
            

            if (Functions.getServerVariableAsString(ID, flag, ref retVal) == ERROR_OK)
            {
                var s = ReadString(retVal);
                return s;
            }
            return String.Empty;
        }

        internal void UpdateSetup(VoiceClient.Model.VoicePaketSetup newSetup)
        {

            _voiceSetup = newSetup;
            if (newSetup.AllowedOutGameChannels != null)
                AllowedOutGameChannels = new HashSet<ulong>(newSetup.AllowedOutGameChannels);
            else
                AllowedOutGameChannels = new HashSet<ulong>();
        }

        internal void ConnectVoiceServer(string myName)
        {
            IsVoiceEnabled = true;
            _MutedClients.Clear();
            _UnmutedClients.Clear();

            var cls = ClientMap.Values.Where(cl => cl.ID != LocalClientId && cl.ChannelID == IngameChannel).Select(cl => cl.ID).ToArray();
            _MutedClients = new ConcurrentHashSet<ushort>(cls);
            DoMuteList(cls);
            _MuteTimer.Start();
            var c = GetClient(LocalClientId);
            if (c != null)
            {
                c.JoinChannel(IngameChannel, IngameChannelPassword);
                c.SetNickName(myName);
            }
        }

        internal Client GetKnownClient(ushort clientID)
        {
            if (ClientMap.ContainsKey(clientID))
                return ClientMap[clientID];
            return null;
        }

        internal void DisconnectVoiceServer()
        {
            if (_MuteTimer.Enabled)
                _MuteTimer.Stop();

            if (IsVoiceEnabled)
            {
                IsVoiceEnabled = false;
                _MutedClients.Clear();
                _UnmutedClients.Clear();
                SwitchBack();
            }
            VoicePlugin.IsConnectedToVoiceServer = false;
        }

        internal void UnmuteAll()
        {
            DoUnmuteList(ClientMap.Values.Where(cl => cl.ID != LocalClientId).Select(cl => cl.ID).ToArray());
            _MutedClients.Clear();
            _UnmutedClients.Clear();
        }

        internal void Mute(string clientName)
        {
            if (clientName == "_ALL_")
            {
                Mute(ClientMap.Values.Where(cl => cl.ID != LocalClientId && cl.ChannelID == IngameChannel).Select(cl => cl.ID).ToArray());
                return;
            }
            var c = GetClient(clientName);
            DoMute(c.ID);
        }

        internal void AddVoiceFilter(byte distortion, VoiceFilterChain voiceFilterChain)
        {
            _VoiceFilters[distortion] = voiceFilterChain;
        }

        internal unsafe void ProcessVoice(byte currentVoiceDistortion, short* samples, int sampleCount, int channels,float volume = 1.0f)
        {
            if (_VoiceFilters.TryGetValue(currentVoiceDistortion,out var filter))
            {
                VoicePlugin.VerboseLog("ApplyFilter " + filter.ToString());
                const float mul = 1.0f / 32768.0f;
                filter.SignalQuality = volume;
                float[] data = new float[sampleCount * channels];
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    data[i] = (float)samples[i] * mul;
                }
                filter.Process(data,sampleCount*channels);
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    var f = data[i];
                    if (f > 1.0f)
                    {
                        f = 1.0f;
                    }
                    if (f < -1.0f)
                    {
                        f = -1.0f;
                    }
                   samples[i] = (short)(Math.Floor(f * 32767.0f));
                }
            }
        }


        internal void Unmute(string clientName)
        {
            if (clientName == "_ALL_")
            {
                Unmute(ClientMap.Values.Where(cl => cl.ID != LocalClientId && cl.IsMuted).Select(cl => cl.ID).ToArray());
                return;
            }
            var c = GetClient(clientName);

            DoUnmute(c.ID);
        }

        internal void Mute(params ushort[] clientIds)
        {
            if (clientIds.Length > 0)
            {
                for (int i = 0; i < clientIds.Length; i++)
                {
                    DoMute(clientIds[i]);
                }
            }
        }

        internal void Unmute(params ushort[] clientIds)
        {
            if (clientIds.Length > 0)
            {
                for (int i = 0; i < clientIds.Length; i++)
                {
                    DoUnmute(clientIds[i]);
                }
            }
        }

      
        internal void RemoveMute(ushort clientId)
        {
            _UnmutedClients.TryRemove(clientId);
            _MutedClients.TryRemove(clientId);
        }

        internal void DoMute(ushort clientId)
        {
            if (clientId != LocalClientId)
            {
                _UnmutedClients.TryRemove(clientId);
                if (_MutedClients.Add(clientId))
                    ExecuteMute(clientId, true);
            }
        }

        internal void DoUnmute(ushort clientId)
        {
            _MutedClients.TryRemove(clientId);
            if (_UnmutedClients.Add(clientId))
                ExecuteMute(clientId, false);
        }

        internal void ExecuteMute(ushort clientId, bool isMute)
        {
            if (isMute && clientId == LocalClientId)
                return;
            VoicePlugin.Log("{0} {1}", isMute ? "Muting" : "Unmuting", clientId);
            List<ushort> clients;
            clients = new List<ushort>();
            clients.Add(clientId);
            clients.Add(0);
            if (isMute)
                Check(Functions.requestMuteClients(ID, clients.ToArray(), ""));
            else
                Check(Functions.requestUnmuteClients(ID, clients.ToArray(), ""));
        }

        internal void DoUnmuteList(params ushort[] clientIds)
        {
            if (clientIds.Length > 0)
            {
                List<ushort> clients;
                clients = new List<ushort>(clientIds);
                clients.Add(0);
                Log("UnmutingList  {0}: {1}", clients.Count, string.Join(",", clients));
                Check(Functions.requestUnmuteClients(ID, clients.ToArray(), ""));
            }
        }

        internal void DoMuteList(params ushort[] clientIds)
        {
            if (clientIds.Length > 0)
            {
                List<ushort> clients;
                clients = new List<ushort>(clientIds);
                clients.Remove(LocalClientId);
                clients.Add(0);
                Log("MutingList  {0}: {1}", clients.Count, string.Join(",", clients));
                Check(Functions.requestMuteClients(ID, clients.ToArray(), ""));
            }
        }

        internal void RegisterMutedClients(params ushort[] clientIds)
        {
            if (clientIds.Length > 0)
            {
                for (int i = 0; i < clientIds.Length; i++)
                {
                    if (clientIds[i] != LocalClientId)
                        _MutedClients.Add(clientIds[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void DoMuteActions()
        {
            var clientCurrentChannel = LocalClient.CurrentChannel;
            if (clientCurrentChannel != IngameChannel)
            {
                if (!CanLeaveIngameChannel && !AllowedOutGameChannels.Contains(clientCurrentChannel))
                {
                    VoicePlugin.Log("Timer: Local Client left ingame channel while inGame");
                    LocalClient.JoinChannel(IngameChannel, IngameChannelPassword);
                }
                else
                { // client may be in this channel when ingame. Make sure people are unmuted here
                    GetChannel(clientCurrentChannel).UnmuteAll();
                    return;
                }
            }

            string retcode = "";
            List<ushort> clients;
            if (_MutedClients.Count > 0)
            {
                clients = new List<ushort>(_MutedClients.ToArray());
                clients.Add(0);
                Log("Timer: Muting  {0}: {1}", clients.Count, string.Join(",", clients));
                Check(Functions.requestMuteClients(ID, clients.ToArray(), retcode));
            }
            if (_UnmutedClients.Count > 0)
            {
                clients = new List<ushort>(_UnmutedClients.ToArray());
                clients.Add(0);
                Log("Timer: Unmuting  {0}: {1}", clients.Count, string.Join(",", clients));
                Check(Functions.requestUnmuteClients(ID, clients.ToArray(), retcode));
            }
        }

        internal void SetCanLeaveIngameChannel(bool canLeaveIngameChannel)
        {
            CanLeaveIngameChannel = canLeaveIngameChannel;
        }

        private void SwitchBack()
        {
            var c = GetClient(LocalClientId);
            if (c != null && c.ChannelID == IngameChannel)
            {
                c.UnmuteClientsAfterMove = true;
                c.JoinChannel(c.LastChannel);
                c.SetNickName(LocalClientName);
            }
        }

        internal void ClientTalkStatusChanged(ushort clientID, int status, int isReceivedWhisper)
        {
            if (clientID != LocalClientId)
                return;
            VoicePlugin.voiceClient.SendTalkStatusChanged(status == 1);
        }

        private bool DoesFileExist(string filename)
        {
            return File.Exists(filename);
        }
        internal string GetEffectiveFilename(string name)
        {
            if (DoesFileExist(VoicePlugin.GetFilename("custom", name)))
                return VoicePlugin.GetFilename("custom", name);
            if (DoesFileExist(VoicePlugin.GetFilename(ServerHashString, name)))
                return VoicePlugin.GetFilename(ServerHashString, name);
            if (DoesFileExist(VoicePlugin.GetFilename("default", name)))
                return VoicePlugin.GetFilename("default", name);
            return null;
        }

        internal void PlaySound(string voiceSound, bool isLocal)
        {
            string soundFilename;
            if (isLocal)
            {
                soundFilename = GetEffectiveFilename(voiceSound +"Local.wav");
                if (!String.IsNullOrEmpty(soundFilename))
                {
                    PlaySound(soundFilename);
                    return;
                }

            }
            soundFilename = GetEffectiveFilename(voiceSound+".wav");
            if (!String.IsNullOrEmpty(soundFilename) )
            {
                PlaySound(soundFilename);
            }
        }

        internal void PlaySound(string soundFilename)
        {
            try
            {
                Check(Functions.playWaveFile(ID, soundFilename));
            }
            catch (Exception ex)
            {
                Log("ERROR can't play sound {0}: {1}", soundFilename, ex.ToString());
            }
        }
    }
}
