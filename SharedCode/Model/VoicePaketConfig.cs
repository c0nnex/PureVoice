using LiteNetLib.Utils;
using System;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketConfig 
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string ServerSecret { get; set; }
        public string ServerGUID { get; set; }
        public string ClientGUID { get; set; }
        public string _ClientVersionRequired { get; set; } = "0.0.0.0";
        public Version ClientVersionRequired = new Version(0, 0, 0, 0);

        public override string ToString()
        {
            return $"{ServerIP}:{ServerPort} {ServerGUID} / {ClientGUID} v:{ClientVersionRequired}";
        }
    }
}
