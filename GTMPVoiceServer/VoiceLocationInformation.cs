using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureVoice
{
    public class VoiceLocationInformation
    {
        public ushort ClientID;
        public string Name;
        public bool IsTalking = false;

        private TSVector position = new TSVector();
        private float volumeModifier = 0;
        private bool isRelative = false;
        private bool needUpdate = false;

        public VoiceLocationInformation(string name)
        {
            Name = name;
            needUpdate = true;
        }

        public VoiceLocationInformation(ushort id)
        {
            ClientID = id;
            needUpdate = true;
        }

        public VoiceLocationInformation(string name, ushort id)
        {
            Name = name;
            ClientID = id;
            needUpdate = true;
        }

        public VoiceLocationInformation(VoiceLocationInformation data) : this(data.Name)
        {
            ClientID = data.ClientID;
            position = data.Position;
            volumeModifier = data.VolumeModifier;
            isRelative = data.IsRelative;
            IsTalking = data.IsTalking;
            needUpdate = true;
        }

        public bool NeedUpdate
        {
            get
            {
                var r = needUpdate;
                needUpdate = false;
                return r;
            }

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

        public bool Update(TSVector position, float volumeModifier, bool isRelative)
        {
            VolumeModifier = volumeModifier;
            IsRelative = isRelative;
            Position = position;
            return needUpdate;
        }
    }
}
