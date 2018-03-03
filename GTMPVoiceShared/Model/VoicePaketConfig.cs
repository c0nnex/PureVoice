using LiteNetLib.Utils;
using System;

namespace GTMPVoice.VoiceClient.Model
{
    class VoicePaketConfig : INetSerializable
    {
        public string Hello { get; set; } = "INVALID";
        public int VoiceVersion { get; set; } = -1;
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string ServerSecret { get; set; }
        public string ClientGUID { get; set; }
        public string _ClientVersionRequired { get; set; } = "0.0.0.0";
        public Version ClientVersionRequired = new Version(0, 0, 0, 0);

        public void Deserialize(NetDataReader reader)
        {
            try
            {
                Hello = reader.GetString();
                VoiceVersion = reader.GetInt();
                ServerIP = reader.GetString();
                ServerPort = reader.GetInt();
                ServerSecret = reader.GetString();
                ClientGUID = reader.GetString();
                _ClientVersionRequired = reader.GetString();
                if (!String.IsNullOrEmpty(_ClientVersionRequired))
                    Version.TryParse(_ClientVersionRequired,out ClientVersionRequired);
            }
            catch { }
        }

        public void Serialize(NetDataWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"{ServerIP}:{ServerPort} {ClientGUID} v:{ClientVersionRequired}";
        }
    }
}
