using LiteNetLib;
using LiteNetLib.Utils;

namespace PureVoice.VoiceClient
{
    class VoicePaketProcessor : NetPacketProcessor
    {
        private readonly NetDataWriter _netDataWriter = new NetDataWriter();

        public void SendKnown(NetManager netManager, INetSerializable o, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            using (var nw = new NetDataWriter())
            {
                WriteHash(o.GetType(), nw);
                o.Serialize(nw);
                netManager.SendToAll(nw, deliveryMethod);
            }
        }

        public void SendKnown(NetPeer peer, INetSerializable o, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            using (var nw = new NetDataWriter())
            {
                WriteHash(o.GetType(), nw);
                o.Serialize(nw);
                peer.Send(nw, deliveryMethod);
            }
        }
    }
}