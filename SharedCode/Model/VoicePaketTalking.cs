using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketTalking 
    {
        public bool IsTalking { get; set; }

        public VoicePaketTalking() { }
        public VoicePaketTalking(bool isTalking) { IsTalking = isTalking; }
    }

    class VoicePaketMicrophoneState
    {
        public bool IsMuted { get; set; }

        public VoicePaketMicrophoneState() { }
        public VoicePaketMicrophoneState(bool isMuted) { IsMuted = isMuted; }
    }

    class VoicePaketSpeakersState
    {
        public bool IsMuted { get; set; }

        public VoicePaketSpeakersState() { }
        public VoicePaketSpeakersState(bool ismuted) { IsMuted = ismuted; }
    }
}
