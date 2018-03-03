using System;
using System.Collections.Generic;
using System.Text;

namespace GTMPVoice.VoiceClient.Model
{
    class VoicePaketTalking : IVoicePaketModel
    {
        public bool IsTalking { get; set; }

        public VoicePaketTalking() { }
        public VoicePaketTalking(bool isTalking) { IsTalking = isTalking; }
    }
}
