using PureVoice.DSP.Effects;

namespace PureVoice.DSP.Presets
{
    public class ShortRangeRadio: VoiceFilterChain, IIsFilterPreset
    {
        public ShortRangeRadio()
        {
            AddFilter(new HighpassFilter(TS_SAMPLE_RATE, 750).WithPrefix("high"));
            AddFilter(new LowpassFilter(TS_SAMPLE_RATE, 4000).WithPrefix("low"));
            AddFilter(new Amplify().WithPrefix("amp"));
            AddFilter(new PinkNoise());
            AddFilter(new WhiteNoise());
            AddFilter(new RingModulate());
            AddFilter(new FoldBack());
            

        }
    }
}
