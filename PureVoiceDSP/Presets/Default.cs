using PureVoice.DSP.Effects;

namespace PureVoice.DSP.Presets
{
    public class Default : VoiceFilterChain, IIsFilterPreset
    {
        public Default()
        {
            AddFilter(new Amplify().WithPrefix("amp"));
            AddFilter(new HighpassFilter(48000, 400).WithPrefix("high"));
            AddFilter(new LowpassFilter(48000, 2800).WithPrefix("low"));
        }
    }
}
