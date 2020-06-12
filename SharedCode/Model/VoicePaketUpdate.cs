
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureVoice.VoiceClient.Model
{
    class VoicePaketUpdate 
    {
        public ushort ClientID { get; set; } = 0;
        public TSVector Position { get; set; }
        public float VolumeModifier { get; set; }
        public float SignalQuality { get; set; } = 1.0f;
        public bool PositionIsRelative { get; set; }

        public byte VoiceDistortion { get; set; }
    }

    class VoicePaketUpdateDistortion 
    {
        public ushort ClientID { get; set; } = 0;
        public byte VoiceDistortion { get; set; }
        
        public string VoiceSound { get; set; }
    }

    

    class VoicePaketBatchUpdate : INetSerializable
    {
        public List<BatchDataRecord> BatchData = new List<BatchDataRecord>(); // No getter/Setter = do not serialize over network

        public void Deserialize(NetDataReader reader)
        {
            var LZ4Utility = new LZ4Sharp.LZ4Decompressor64();
            var data = LZ4Utility.Decompress(reader.GetBytesWithLength());
            var r = new NetDataReader(data);
            var len = r.GetUShort();
            var v = new List<BatchDataRecord>();
            for (int i = 0; i < len; i++)
            {
                var d = new BatchDataRecord();
                d.Deserialize(r);
                v.Add(d);
            }
            BatchData = v;
            r.Clear();
        }

        public void Serialize(NetDataWriter writer)
        {
            var w = new NetDataWriter();
            if (BatchData.Count > ushort.MaxValue)
                throw new NotSupportedException("Too many items!");
            w.Put((ushort)BatchData.Count);
            foreach (var item in BatchData)
            {
                item.Serialize(w);
            }
            var LZ4Utility = new LZ4Sharp.LZ4Compressor64();
            writer.PutBytesWithLength(LZ4Utility.Compress(w.Data));
        }
    }

    class BatchDataRecord : INetSerializable
    {
        public ushort ClientID { get; set; }
        public TSVector Position { get; set; }
        public float VolumeModifier { get; set; }
        public float SignalQuality { get; set; }
        public byte VoiceDistortion { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ClientID = reader.GetUShort();
            Position = new TSVector(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            VolumeModifier = reader.GetFloat();
            SignalQuality = reader.GetFloat();
            VoiceDistortion = (byte)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ClientID);
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put(Position.Z);
            writer.Put(VolumeModifier);
            writer.Put(SignalQuality);
            writer.Put((byte)VoiceDistortion);
        }
    }
}
