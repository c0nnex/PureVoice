using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketTalking : IVoicePaketModel
    {
        public bool IsTalking { get; set; }

        public VoicePaketTalking() { }
        public VoicePaketTalking(bool isTalking) { IsTalking = isTalking; }
    }

    class VoicePaketMicrophoneState : IVoicePaketModel
    {
        public bool IsMuted { get; set; }

        public VoicePaketMicrophoneState() { }
        public VoicePaketMicrophoneState(bool isMuted) { IsMuted = isMuted; }
    }

    class VoicePaketSpeakersState : IVoicePaketModel
    {
        public bool IsMuted { get; set; }

        public VoicePaketSpeakersState() { }
        public VoicePaketSpeakersState(bool ismuted) { IsMuted = ismuted; }
    }
}
