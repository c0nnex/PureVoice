using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.DSP.Effects
{
    public class Amplify : IVoiceFilter
    {
        float amplify = 4.0f;
        public override void Process(float[] samples, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++) samples[i] *= amplify;
        }
        public override IVoiceFilter Configure()
        {
            amplify = GetConfigValue("amplify", amplify);
            return base.Configure();
        }
    }
}
