using LiteNetLib.Utils;
using System;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketConnectServer : INetSerializable
    {
        public string Secret { get; set; }
        public Version Version { get; set; }
        public string ClientGUID { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Secret = reader.GetString();
            Version = Version.Parse(reader.GetString());
            ClientGUID = reader.GetString();

        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Secret);
            writer.Put(Version.ToString());
            writer.Put(ClientGUID);
        }
    }

    class VoicePaketConnectClient 
    {
        public string ClientGUID { get; set; }
        public string TeamspeakID{ get; set; }
        public ushort TeamspeakClientID { get; set; }
        public string TeamspeakClientName { get; set; }
        public bool MicrophoneMuted { get; set; }
        public bool SpeakersMuted { get; set; }
    }

    class VoicePaketConfigureClient 
    {
        public string Nickname { get; set; }
        public bool CanLeaveIngameChannel { get; set; }
    }
}