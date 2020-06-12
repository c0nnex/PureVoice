using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.DSP.Effects
{
    public class Tremolo : IVoiceFilter
    {
        private double _frequency = 800;
        private double _amplitude = 1.0;
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the amplitude of the sine wave.
        /// </summary>
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _amplitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the phase of the sine wave.
        /// </summary>
        public double Phase { get; set; }

        private float w;

        public override void Process(float[] samples, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                 //float sine = (float)(Amplitude * Math.Sin(Frequency * Phase * Math.PI * 2));
                float sine = (float)((1 - Inverse_SignalQuality) + Inverse_SignalQuality * Math.Pow(Math.Sin(Frequency * Phase * Math.PI * 2),2));
//                float mod = (1-Inverse_SignalQuality) + Inverse_SignalQuality * (Math.Sin(w*Frequency))
                samples[i] *= sine;

                Phase += (1.0 / TS_SAMPLE_RATE);
            }
        }
        public override IVoiceFilter Configure()
        {
            Amplitude = GetConfigValue("amplitude", 1.0f);
            Frequency = GetConfigValue("frequency", 800f);

            w = (float)(2*Math.PI) / TS_SAMPLE_RATE;

            return base.Configure();
        }
    }
}
