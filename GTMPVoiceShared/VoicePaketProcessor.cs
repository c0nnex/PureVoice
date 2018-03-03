using LiteNetLib;
using LiteNetLib.Utils;

namespace GTMPVoice.VoiceClient
{
    class VoicePaketProcessor : NetPacketProcessor
    {
        private readonly NetDataWriter _netDataWriter = new NetDataWriter();

        public void SendKnown(NetManager netManager, INetSerializable o, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            _netDataWriter.Reset();
            WriteHash(o.GetType(), _netDataWriter);
            o.Serialize(_netDataWriter);
            netManager.SendToAll(_netDataWriter, deliveryMethod);
        }

        public void SendKnown(NetPeer peer, INetSerializable o, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            _netDataWriter.Reset();
            WriteHash(o.GetType(), _netDataWriter);
            o.Serialize(_netDataWriter);
            peer.Send(_netDataWriter, deliveryMethod);
        }
    }
}