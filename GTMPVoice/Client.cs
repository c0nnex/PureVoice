using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamSpeakPlugin;

namespace GTMPVoice
{
    public class Client : TeamSpeakBase
    {
        private static TSVector NullVector = new TSVector(0, 0, 0);
        public Connection Connection;
        public ushort ID { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
        public string LastName;
        public TSVector Position;
        public float VolumeModifier;
        public bool IsTalking;
        public ulong ChannelID = 0;
        public ulong LastChannel = 1;

        public bool IsLocalClient;
        public bool ShouldBeMuted = false;
        public bool IsMuted => GetBool(ClientProperty.CLIENT_IS_MUTED);
        public ulong CurrentChannel => GetClientChannel();
        public Channel Channel => Connection?.GetChannel(ChannelID);

        private ulong GetClientChannel()
        {
            ulong cId = ChannelID;
            Check(Functions.getChannelOfClient(Connection.ID, ID, out cId));
            return cId;
        }

        public Client(Connection connection, ushort id, bool waitForProperties = true)
        {
            Connection = connection;
            ID = id;
            if (waitForProperties)
                Update();
        }

        public Client(Connection connection, ushort id, string name) : this(connection, id)
        {
            LastName = Name;
            Name = name;
        }

        public void Update(bool updateChannel = false)
        {
            Name = GetClientVarString(ClientProperty.CLIENT_NICKNAME);
            if (String.IsNullOrEmpty(GUID))
            {
                GUID = GetClientVarString(ClientProperty.CLIENT_UNIQUE_IDENTIFIER);
                Log("Client {0} {1} ({2})", ID, GUID, Name);
            }
        }

        public void UpdateChannel()
        {
            OnChannelChanged(CurrentChannel);
        }

        public void SetNickName(string newName)
        {
            if (ID != Connection.LocalClientId)
                return;
            if (Name != newName)
            {
                var saveName = Name;
                if (Functions.setClientSelfVariableAsString(Connection.ID, ClientProperty.CLIENT_NICKNAME, newName) == ERROR_OK)
                {
                    LastName = saveName;
                    Name = newName;
                }
            }
        }

        public void OnChannelChanged(ulong newChannel)
        {
            if (newChannel == 0)
            {
                Connection.RemoveClient(ID);
                Connection = null;
                return;
            }
            if (newChannel == ChannelID)
                return;
            var oldChannel = ChannelID;
            ChannelID = newChannel;

            if (ID == Connection.LocalClientId)
            {
                if (newChannel == Connection.IngameChannel)
                {
                    if (LastChannel == 1)
                        LastChannel = oldChannel;
                    Connection.GetChannel(newChannel).MuteAll();
                    Connection.RegisterMutedClients(Channel?.AllClients);
                }
                else
                {
                    LastChannel = newChannel;
                    Connection.GetChannel(newChannel).UnmuteAll(true);
                }
                return;
            }
            if (Connection.IsInitialized)
            {
                if (newChannel == Connection.IngameChannel)
                {
                    Connection.DoMute(ID);
                }
                else // someone leaves the ingame channel. Reset 3D position and volume if i am not ingame
                {
                    if (Connection.LocalClient.ChannelID != Connection.IngameChannel)
                    {
                        Connection.ExecuteMute(ID, false);
                        Check(Functions.channelset3DAttributes(Connection.ID, ID, new TSVector()));
                        Check(Functions.setClientVolumeModifier(Connection.ID, ID, 0));
                    }
                }
            }
        }

        internal void JoinDefaultChannel()
        {
            if (ID != Connection.LocalClientId)
                return;
            Log("Joining channel {0}", Connection.DefaultChannel);
            Check(Functions.requestClientMove(Connection.ID, ID, Connection.DefaultChannel, "", ""));
        }

        internal void JoinChannel(ulong newChannel, string password = "")
        {
            if (ID != Connection.LocalClientId)
                return;
            Log("{0} Joining channel {1}", ID, newChannel);
            if (newChannel == Connection.IngameChannel)
                Check(Functions.requestClientMove(Connection.ID, ID, newChannel, password, GTMPVoicePlugin.GetReturnCode(PluginReturnCode.JOINCHANNEL_INGAME)));
            else
                Check(Functions.requestClientMove(Connection.ID, ID, newChannel, password, GTMPVoicePlugin.GetReturnCode(PluginReturnCode.JOINCHANNEL)));
        }

        public string GetClientVarString(ClientProperty flag)
        {
            if (!Connection.IsConnected)
            {
                Log("Not connected");
                return String.Empty;
            }
            IntPtr retVal = IntPtr.Zero;
            IntPtr flg = new IntPtr((int)flag);
            if (ID == Connection.LocalClientId)
            {
                if (Functions.getClientSelfVariableAsString(Connection.ID, flag, ref retVal) == ERROR_OK)
                {
                    var s = ReadString(retVal);
                    return s;
                }
            }
            else
            {
                if (Functions.getClientVariableAsString(Connection.ID, ID, flag, ref retVal) == ERROR_OK)
                {
                    var s = ReadString(retVal);
                    return s;
                }
            }
            return String.Empty;
        }

        

        private int GetInt(ClientProperty flag)
        {
            int res = 0;
            Check(Functions.getClientVariableAsInt(Connection.ID, ID, (IntPtr)flag, out res));
            return res;
        }

        private bool GetBool(ClientProperty flag)
        {
            return GetInt(flag) != 0;
        }


        internal void UpdatePosition(TSVector position, float volumeModifier, bool positionIsRelative,bool execMute = true)
        {
            if (positionIsRelative)
            {
                if (Connection.LocalClient != null)
                    Position = position + Connection.LocalClient.Position;
            }
            else
                Position = position;
            VolumeModifier = volumeModifier;
            if (IsLocalClient)
            {
                Check(Functions.systemset3DListenerAttributes(Connection.ID, Position, NullVector, NullVector));
            }
            else
            {
                Check(Functions.channelset3DAttributes(Connection.ID, ID, Position));
                Check(Functions.setClientVolumeModifier(Connection.ID, ID, VolumeModifier));
                if (execMute)
                    Unmute();
            }

        }

        internal void SetName(string displayName)
        {
            if (Name != displayName)
            {
                Log("new name c:{0} {1} => {2}", ID, Name, displayName);
                Name = displayName;
            }
        }

        internal void ModifyVolume(float distance, ref float volume)
        {
            Log("ModifyVolume c:{0} d:{1} v:{2} m:{3} => {4}", Name, distance, volume, VolumeModifier, volume + VolumeModifier);
            volume += VolumeModifier;
        }

        internal void Mute()
        {
            Connection.DoMute(ID);
        }

        internal void Unmute()
        {
            Connection.DoUnmute(ID);
        }

        
    }
}
