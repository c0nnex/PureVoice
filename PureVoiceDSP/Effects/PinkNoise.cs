using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureVoice.DSP.Effects
{
    public class PinkNoise : IVoiceFilter
    {
        float[] state = new float[3];
        static readonly float[] A = { 0.02109238f, 0.07113478f, 0.68873558f };
        static readonly float[] P = { 0.3190f, 0.7756f, 0.9613f };
        static readonly float offset = 0.02109238f + 0.07113478f + 0.68873558f;

        public PinkNoise()
        {
            Clear();
        }

        public float Tick()
        {
            const float RMI2 = 2.0f / int.MaxValue; // + 1.0; // change for range [0,1)
            // unrolled loop
            float temp = (float)rnd.Next();
            state[0] = P[0] * (state[0] - temp) + temp;
            temp = (float)rnd.Next();
            state[1] = P[1] * (state[1] - temp) + temp;
            temp = (float)rnd.Next();
            state[2] = P[2] * (state[2] - temp) + temp;
            return (A[0] * state[0] + A[1] * state[1] + A[2] * state[2]) * RMI2 - offset;
        }

        public void Clear()
        {
            state[0] = state[1] = state[2] = 0.0f;
        }

        public override float Process(float sample)
        {
            float noise = Tick() * (0.35f * Inverse_SignalQuality);
            return (sample + noise) - (noise * sample);
        }
    }

    public class WhiteNoise : IVoiceFilter
    {
        public float Tick()
        {
            /* Setup constants */
            const int q = 15;
            const float c1 = (1 << q) - 1;
            const float c2 = (float)((int)(c1 / 3)) + 1;
            const float c3 = 1.0f / c1;

            float random = (float)rnd.NextDouble();
            return (2.0f * ((random * c2) + (random * c2) + (random * c2)) - 3.0f * (c2 - 1.0f)) * c3;
        }

        public override float Process(float sample)
        {
            float noise = Tick() * (0.001f * Inverse_SignalQuality);
            return sample + noise;
        }
    }
}
