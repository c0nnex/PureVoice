using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureVoice
{
    public class VoiceLocationInformation 
    {
        public bool IsTalking = false;

        private TSVector position = new TSVector();
        private float volumeModifier = 0;
        private bool isRelative = false;
        private bool needUpdate = false;
        private float signalQuality = 1.0f;
        private byte voiceDistortion = 0;

        public float DistanceToMe = 0f; // Not Synced to Voice-Server!

        public VoiceLocationInformation(ushort id)
        {
            ClientID = id;
            needUpdate = true;
        }

        public VoiceLocationInformation(VoiceLocationInformation data) : this(data.ClientID)
        {
            ClientID = data.ClientID;
            position = data.Position;
            volumeModifier = data.VolumeModifier;
            signalQuality = data.SignalQuality;
            isRelative = data.IsRelative;
            IsTalking = data.IsTalking;
            needUpdate = true;
        }

        public void UpdateProcessed()
        {
            needUpdate = false;
        }
        public ushort ClientID { get; set; }

        public bool NeedUpdate
        {
            get => needUpdate;
            set
            {
                needUpdate = value;
            }
        }

        public float VolumeModifier
        {
            get
            {
                return volumeModifier;
            }

            set
            {
                if (volumeModifier != value)
                    needUpdate = true;
                volumeModifier = value;
            }
        }

        public TSVector Position
        {
            get
            {
                return position;
            }

            set
            {
                if (position != value)
                    needUpdate = true;
                position = value;
            }
        }

        public bool IsRelative
        {
            get
            {
                return isRelative;
            }

            set
            {
                if (isRelative != value)
                    needUpdate = true;
                isRelative = value;
            }
        }

        public float SignalQuality
        {
            get => signalQuality; 
            set
            {
                if (signalQuality != value)
                    needUpdate = true;
                signalQuality = value;
            }
        }

        public byte VoiceDistortion
        {
            get => voiceDistortion;
            set
            {
                if (voiceDistortion != value)
                    needUpdate = true;
                voiceDistortion = value;
            }
        }

        public bool Update(TSVector position, float volumeModifier, bool isRelative)
        {
            VolumeModifier = volumeModifier;
            IsRelative = isRelative;
            Position = position;
            return needUpdate;
        }
    }
}
