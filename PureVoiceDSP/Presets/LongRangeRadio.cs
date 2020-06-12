using PureVoice.DSP.Effects;

namespace PureVoice.DSP.Presets
{
    public class LongRangeRadio: VoiceFilterChain, IIsFilterPreset
    {
        public LongRangeRadio()
        {
            AddFilter(new Amplify().WithPrefix("amp"));
            AddFilter(new HighpassFilter(TS_SAMPLE_RATE, 500).WithPrefix("high"));
            AddFilter(new LowpassFilter(TS_SAMPLE_RATE, 2000).WithPrefix("low"));
            AddFilter(new WhiteNoise());
            AddFilter(new RingModulate());
        }
    }
}
