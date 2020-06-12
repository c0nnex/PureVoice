using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.DSP.Effects
{
    public class RingModulate : IVoiceFilter
    {
        float phase = 0;
        public override float Process(float sample)
        {
            var val = (1.0f - SignalQuality) * 0.20f;

            float multiple = (float)(sample * Math.Sin(phase * PI_2));
            phase += (90.0f * 1.0f / (float)TS_SAMPLE_RATE);
            if (phase > 1.0f) phase = 0.0f;
            return sample * (1.0f - val) + multiple * val;
        }

    }
}
