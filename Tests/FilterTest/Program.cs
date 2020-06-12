using System;
using System.Diagnostics;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using PureVoice.DSP;
using PureVoice.DSP.Effects;

namespace BiQuadFilterSample
{
    class Program
    {
        [STAThread]
        static void Main()
        {

            using (var source = CodecFactory.Instance.GetCodec(@"H:\tmp\sound\polspeaker.wav")
                .ToSampleSource()      
                .AppendSource(x => new BiQuadFilterSource(x)))
            {
                using (var soundOut = new WasapiOut())
                {
                    source.ChangeSampleRate(48000);

                    var chain = new PureVoice.DSP.Presets.LongRangeRadio();
                    chain.Congfigure("");// "amplitude=0.1");
                    source.Filter = chain;// new PhoneFilter().Configure(null);// new RadioFilter(750, 4000, 3.0f, true);// new BandPass(300, 3300, 1.5f, true);
                    source.SignalQuality = 0.1f;// 0.075f;// 0.07f;// 0.27f;// 0.005f;
                    soundOut.Initialize(source.ToWaveSource());
                    soundOut.Play();
                    Console.ReadKey();
                }
            }
        }
    }

    public class BiQuadFilterSource : SampleAggregatorBase
    {
        private readonly object _lockObject = new object();
        private VoiceFilterChain _filters;

        public VoiceFilterChain Filter
        {
            get { return _filters; }
            set
            {
                lock (_lockObject)
                {
                    _filters = value;
                }
            }
        }

        public float SignalQuality { get => Filter.SignalQuality; set => Filter.SignalQuality= value; }

        public BiQuadFilterSource(ISampleSource source) : base(source)
        {
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            if (read > 0)
            {
                Filter.Process(buffer, read);
            }
            return read;
        }
    }
}
