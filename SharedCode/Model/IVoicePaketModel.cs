using LiteNetLib.Utils;
using System;

namespace PureVoice.VoiceClient.Model
{
    class IVoicePaketModel : INetSerializable
    {
        private static readonly VoiceSerializer Serializer = new VoiceSerializer();

        static IVoicePaketModel()
        {
            Serializer.RegisterNestedType<Version>((w, v) => w.Put(v.ToString()), (r) => Version.Parse(r.GetString()));
            Serializer.RegisterNestedType<TSVector>((w, v) =>
            {
                w.Put(v.X);
                w.Put(v.Y);
                w.Put(v.Z);
            },
            (r) =>
            {
                return new TSVector(r.GetFloat(),r.GetFloat(),r.GetFloat());
            });
            Serializer.RegisterNestedType<BatchDataRecord>((w, b) => b.Serialize(w), (r) =>
            {
                var b = new BatchDataRecord();
                b.Deserialize(r);
                return b;
            });
            Register<BatchDataRecord>();
            Register<VoicePaketSetup>();
            Register<VoicePaketCommand>();
            Register<VoicePaketConnectServer>();
            Register<VoicePaketConnectClient>();
            Register<VoicePaketUpdate>();
            Register<VoicePaketTalking>();
            Register<VoicePaketMute>();
            Register<VoicePaketBatchMute>();
            Register<VoicePaketBatchUpdate>();
            Register<VoicePaketConfigureClient>();
            Register<VoicePaketMicrophoneState>();
            Register<VoicePaketSpeakersState>();
        }

        public static void Init() { }

        public IVoicePaketModel()
        {

        }

        public void Deserialize(NetDataReader reader)
        {
            Serializer.DeserializeKnown(reader, this);
        }

        public void Serialize(NetDataWriter writer)
        {
            Serializer.SerializeKnown(writer, this);
        }

        public static void Register<T>()
        {
            Serializer.Register<T>();
        }

    }
}