using System;
using System.Collections.Generic;
using System.Globalization;

namespace PureVoice.DSP
{
    public abstract class IVoiceModifier
    {
        protected const int TS_SAMPLE_RATE = 48000;
        protected const float PI_2 = 1.57079632679489661923f;

        protected Random rnd = new Random(Environment.TickCount);

    }

    public interface IIsFilterPreset
    {
    }

    public class VoiceFilterChain : IVoiceModifier
    {
        protected List<IVoiceFilter> FilterChain = new List<IVoiceFilter>();

        private float signalQuality = 1.0f;

        public float SignalQuality
        {
            get => signalQuality; 
            set
            {
                if (CanChangeSignalQuality)
                    signalQuality = value;
            }
        }
        
        protected bool CanChangeSignalQuality = true;

        public void AddFilter(IVoiceFilter filter) { FilterChain.Add(filter); }
        public void RemoveFilter(IVoiceFilter filter) { FilterChain.Remove(filter); }
        public void RemoveFilters(Func<IVoiceFilter,bool> predicate) { FilterChain.RemoveAll(f => predicate(f)); }
        public void Clear() => FilterChain.Clear();
        public VoiceFilterChain WithFilter(IVoiceFilter filter) { AddFilter(filter); return this; }

        public void Congfigure(string settings)
        {
            foreach (var item in FilterChain)
            {
                item.WithConfiguration(settings);
            }

        }
        public void Process(float[] samples, int sampleCount)
        {
            foreach (var item in FilterChain)
            {
                item.SignalQuality = SignalQuality;
                item.Process(samples, sampleCount);
            }        
        
        }
    }

    public abstract class IVoiceFilter : IVoiceModifier
    {
        const int DELAY_SAMPLES = TS_SAMPLE_RATE / 20;
        float[] delayLine = new float[DELAY_SAMPLES];
        int delayPosition = 0;

        protected float signalQuality;
        protected float Inverse_SignalQuality => 1.25f - signalQuality;

        private string ConfigurationPrefix = null;

        public float SignalQuality
        {
            get => signalQuality;
            set
            {
                signalQuality = (float)Math.Max(0.001, Math.Min(1.0f, value));
            }
        }

        public IVoiceFilter() { }

        public virtual float Process(float sample) { return sample; }

        public virtual void Process(float[] samples, int sampleCount)
        {
            PreProcess(samples, sampleCount);
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = Process(samples[i]);
            }
            PostProcess(samples, sampleCount);
        }

        public virtual void PreProcess(float[] samples, int sampleCount) { }
        public virtual void PostProcess(float[] samples, int sampleCount) { }

        protected Dictionary<string, float> Settings = new Dictionary<string, float>();

        public IVoiceFilter WithConfiguration(string settings)
        {
            var parts = settings.Split(';');
            foreach (var item in parts)
            {
                var kvp = item.Split('=');
                if (kvp.Length != 2 || String.IsNullOrEmpty(kvp[0]))
                    continue;
                if (!String.IsNullOrEmpty(ConfigurationPrefix))
                {
                    if (!kvp[0].StartsWith(ConfigurationPrefix, StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    kvp[0] = kvp[0].Replace(ConfigurationPrefix, "");
                }
                var val = Convert.ToSingle(kvp[1],CultureInfo.InvariantCulture);
                Settings[kvp[0].ToLowerInvariant()] = val;
            }
            return Configure();
            
        }

        public IVoiceFilter WithPrefix(string prefix)
        {
            if (String.IsNullOrEmpty(prefix))
                throw new ArgumentNullException("prefix");
            ConfigurationPrefix = prefix+".";
            return this;
        }

        public virtual IVoiceFilter Configure() => this;

        protected float GetConfigValue(string key, float defaultValue)
        {
            if (Settings.TryGetValue(key.ToLowerInvariant(), out var val))
                return val;
            return defaultValue;
        }
        protected double GetConfigValue(string key, double defaultValue)
        {
            if (Settings.TryGetValue(key.ToLowerInvariant(), out var val))
                return val;
            return defaultValue;
        }
    }

    public abstract class IVoiceNoise : IVoiceModifier
    {
        public abstract float Tick();
        public virtual void Clear() { }
    }
}