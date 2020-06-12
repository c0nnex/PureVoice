using PureVoice.DSP.Effects;

namespace PureVoice.DSP.Presets
{
    public class Phone : VoiceFilterChain, IIsFilterPreset
    {
        public Phone()
        {
            AddFilter(new Amplify().WithPrefix("amp"));
            AddFilter(new HighpassFilter(TS_SAMPLE_RATE, 400).WithPrefix("high"));
            AddFilter(new LowpassFilter(TS_SAMPLE_RATE, 2800).WithPrefix("low"));
            AddFilter(new ButterworthLowPassFilter());
            AddFilter(new Tremolo().WithPrefix("temolo"));
        }
    }
}
