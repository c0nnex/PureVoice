using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketUpdate : IVoicePaketModel
    {
        public string PlayerName { get; set; }
        public TSVector Position { get; set; }
        public float VolumeModifier { get; set; }
        public bool PositionIsRelative { get; set; }
    }

    class VoicePaketBatchUpdate : IVoicePaketModel
    {
        public BatchDataRecord[] Data { get; set; }
    }

    class BatchDataRecord : IVoicePaketModel
    {
        public ushort ClientID { get; set; }
        public TSVector Position { get; set; }
        public float VolumeModifier { get; set; }
    }
}
