using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketMute : IVoicePaketModel
    {
        public string Name { get; set; }
        public bool IsMute { get; set; }

        public VoicePaketMute() { }
        public VoicePaketMute(bool isMute) { IsMute = isMute; }
    }

    class VoicePaketBatchMute : IVoicePaketModel
    {
        public List<ushort> Mutelist = new List<ushort>();
        public ushort[] Data
        {
            get { return Mutelist.ToArray(); }
            set { Mutelist = new List<ushort>(value); }
        }
    }
}
