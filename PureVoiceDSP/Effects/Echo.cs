using PureVoice.DSP;
using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.DSP.Effects
{
    public class Echo : IVoiceFilter
    {
        int delayMilliseconds = 200;
        float decay = 0.5f;
        float[] delayLine;
        int delaySamples;
        int delayPosition = 0;

        public override IVoiceFilter Configure()
        {
            delayMilliseconds = (int)GetConfigValue("delay", 200f);
            decay = GetConfigValue("decay", 0.5f);
            delaySamples = (int)((float)delayMilliseconds * (TS_SAMPLE_RATE / 1000.0f));
            delayLine = new float[delaySamples];
            return base.Configure();
        }
        public override void Process(float[] samples, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                delayLine[delayPosition] = samples[i];
                delayPosition = (delayPosition + 1) % delaySamples;
                samples[i] += ((float)delayLine[delayPosition] * decay);
            }
        }
    }
}
