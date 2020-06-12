using LiteNetLib;
using LiteNetLib.Utils;
using PureVoice.VoiceClient.Model;
using System;
using System.Collections.Generic;

namespace PureVoice.VoiceClient
{
    class VoicePaketProcessor : NetPacketProcessor
    {
        private readonly NetDataWriter _netDataWriter = new NetDataWriter();
        private readonly NetSerializer _InternalSerializer = new NetSerializer();

        public void Subscribe<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
        {
            Subscribe<T, TUserData>(onReceive, () => new T());
        }

        public void Register<T>() where T : class, new()
        {
            Subscribe<T, NetPeer>((d, p) => { throw new NotImplementedException("Unregisted DummyPacket "+typeof(T)+" received!"); }, () => new T());
        }

        public void Send<T>(NetPeer peer, T packet) where T : class, new()
        {
            Send<T>(peer, packet, DeliveryMethod.ReliableOrdered);
        }
        public void Send<T>(NetManager peer, T packet) where T : class, new()
        {
            Send<T>(peer, packet, DeliveryMethod.ReliableOrdered);
        }
        public void SendNetSerializable<T>(NetPeer peer, T packet) where T : INetSerializable
        {
            SendNetSerializable<T>(peer, packet, DeliveryMethod.ReliableOrdered);
        }
        public void SendNetSerializable<T>(NetManager peer, T packet) where T : INetSerializable
        {
            SendNetSerializable<T>(peer, packet, DeliveryMethod.ReliableOrdered);
        }

        private void RegisterClass<T>(bool registerList = true) where T : class, new()
        {
            RegisterNestedType<T>((w, v) => _InternalSerializer.Serialize(w, v), (r) => _InternalSerializer.Deserialize<T>(r));
            if (registerList)
                RegisterNestedType<List<T>>((w, v) => SerializeList(w, v), (r) => DeserializeList<T>(r));
        }

        private void RegisterListClass<T>() where T : class, new()
        {
           RegisterNestedType<List<T>>((w, v) => SerializeList(w, v), (r) => DeserializeList<T>(r));
        }

        private List<T> DeserializeList<T>(NetDataReader r) where T : class,new()
        {
            ushort len = r.GetUShort();
            var v = new List<T>();
            for (int i = 0; i < len; i++)
            {
                v.Add(_InternalSerializer.Deserialize<T>(r));
            }
            return v;
        }

        private void SerializeList<T>(NetDataWriter w,List<T> list) where T:class,new()
        {
            if (list.Count > ushort.MaxValue)
                throw new NotSupportedException("Too many items!");
            w.Put((ushort)list.Count);
            foreach (var item in list)
            {
                _InternalSerializer.Serialize<T>(w,item);
            }

        }

        public VoicePaketProcessor() : base()
        {
            RegisterNestedType<Version>((w, v) => w.Put(v.ToString()), (r) => Version.Parse(r.GetString()));
            RegisterNestedType<TSVector>((w, v) =>
            {
                w.Put(v.X);
                w.Put(v.Y);
                w.Put(v.Z);
            },
            (r) =>
            {
                return new TSVector(r.GetFloat(), r.GetFloat(), r.GetFloat());
            });
            RegisterNestedType<Dictionary<string, long>>(
                (w, v) =>
                {
                    w.Put(v.Count);
                    foreach (var item in v)
                    {
                        w.Put(item.Key);
                        w.Put(item.Value);
                    }
                },
                (r) =>
                {
                    var val = new Dictionary<string, long>();
                    var ct = r.GetInt();
                    for (int i = 0; i < ct; i++)
                    {
                        val.Add(r.GetString(), r.GetLong());
                    }
                    return val;
                });
            RegisterClass<VoiceDistortionSetting>();
            // Dummy Make Type registered
            Register<VoicePaketSetup>();
            Register<VoicePaketCommand>();
            Register<VoicePaketConnectServer>();
            Register<VoicePaketConnectClient>();
            Register<VoicePaketUpdate>();
            Register<VoicePaketTalking>();
            Register<VoicePaketMute>();
            Register<VoicePaketBatchMute>();
            Register<BatchDataRecord>();
            Register<VoicePaketBatchUpdate>();
            Register<VoicePaketConfigureClient>();
            Register<VoicePaketMicrophoneState>();
            Register<VoicePaketSpeakersState>();
            Register<VoicePaketUpdateDistortion>();
            Register<VoiceSoundData>();
        }

    }
}