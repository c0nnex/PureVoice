using PureVoice.VoiceClient.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.VoiceClient.Model
{
    class VoiceDistortionSetting
    {
        public VoiceDistortionSetting() { }
        public VoiceDistortionSetting(byte voiceDistortion) : base()
        {
            Enable = true;
            VoiceDistortion = voiceDistortion;
        }

        public void AddFilter(string filterClass, string filterSettings)
        {
            Filters.Add(filterClass + "##" + filterSettings);
        }

        public byte VoiceDistortion { get; set; } = 0;
        public bool Enable { get; set; } = false;
        public string[] _Filters { get => Filters.ToArray(); set => Filters = new List<string>(value); }

        public List<string> Filters = new List<string>();

        public override string ToString()
        {
            return $"{VoiceDistortion}:{String.Join(" ",Filters)} Enabled {Enable}'";
        }


    }
}
