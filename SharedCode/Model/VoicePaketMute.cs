using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketMute 
    {
        public ushort ClientID { get; set; }
        public bool IsMute { get; set; }

        public VoicePaketMute() { }
        public VoicePaketMute(ushort clientID,bool isMute) { ClientID = clientID; IsMute = isMute; }
    }

    class VoicePaketBatchMute 
    {
        public List<ushort> Mutelist = new List<ushort>();
        public ushort[] Data
        {
            get { return Mutelist.ToArray(); }
            set { Mutelist = new List<ushort>(value); }
        }
    }
}
