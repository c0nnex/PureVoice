using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.DSP.Effects
{
    public class FoldBack : IVoiceFilter
    {
        float max = 0.01f;
        public override void Process(float[] samples, int sampleCount)
        {
            Foldback(samples, sampleCount, SignalQuality);
            max = 1f / Math.Min(Math.Max(max, 0.01f), 1f);
            for (int i = 0; i < sampleCount; i++) samples[i] *= max;
        }

        private void Foldback(float[] samples, int count, float signalQuality)
        {
            float divFloat = (float)(256.0f * Math.Pow(signalQuality, 4.0f) - 693.33f * Math.Pow(signalQuality, 3.0f) + 648.0f * Math.Pow(signalQuality, 2.0f) - 250.67f * signalQuality + 40.0f);
            int divisor = (int)divFloat;
            if (divisor < 5)
                divisor = 5;
            for (int i = 0; i < count; i += divisor)
            {
                for (int x = 1; x < divisor && i + x < count; x++)
                {
                    samples[i + x] = samples[i];
                }
            }
        }
    }
}
