using System.Collections.Generic;
using System.Linq;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketSetup
    {
        public string ServerGUID { get; set; }

        public ulong DefaultChannel { get; set; }

        public ulong IngameChannel { get; set; }

        public string IngameChannelPassword { get; set; }

        /// <summary>
        /// Send callback to Server when Talkstatus changes?
        /// </summary>
        public bool EnableLipSync { get; set; }

        /// <summary>
        /// Channels a normal client may be in without being moved back while being In-Game (e.g. Support channels)
        /// </summary>
        public ulong[] AllowedOutGameChannels { get; set; }


        public Dictionary<string, long> VoiceSoundHashes { get; set; } = new Dictionary<string, long>();

        public List<VoiceDistortionSetting> DistortionSettings { get; set;} = new List<VoiceDistortionSetting>();
        public List<VoiceDistortionSetting> GetDistortionSettings() => DistortionSettings;

    }

    class VoiceSoundData 
    {
        public string VoiceSound { get; set; }
       
        public byte[] Data { get; set; }

        public long Hash { get; set; }
    }

    
}