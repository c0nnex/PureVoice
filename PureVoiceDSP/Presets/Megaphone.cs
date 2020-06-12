using PureVoice.DSP.Effects;

namespace PureVoice.DSP.Presets
{
    public class Megaphone : VoiceFilterChain, IIsFilterPreset
    {
        public Megaphone()
        {
            AddFilter(new HighpassFilter(TS_SAMPLE_RATE, 500).WithPrefix("high"));
            AddFilter(new LowpassFilter(TS_SAMPLE_RATE, 1600).WithPrefix("low"));
            AddFilter(new Echo().WithPrefix("echo"));
            AddFilter(new FoldBack());
            AddFilter(new RingModulate());
            AddFilter(new Amplify().WithPrefix("amp"));
            SignalQuality = 0.15f;
            CanChangeSignalQuality = false;
        }
    }
}
