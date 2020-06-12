using LiteNetLib;
using LiteNetLib.Utils;
using PureVoice.VoiceClient;
using PureVoice.VoiceClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTest
{
    class Program : INetLogger
    {
        public static void Log(string msg) => Console.WriteLine(msg);


        static void Main(string[] args)
        {
            var _netPacketProcessor = new VoicePaketProcessor();
            EventBasedNetListener clientListener = new EventBasedNetListener();
            

            clientListener.NetworkReceiveEvent +=
               (peer, reader, method) =>
               {
                   try
                   {
                       _netPacketProcessor.ReadAllPackets(reader, peer);
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine(ex.Message);
                   }
               };

            clientListener.PeerConnectedEvent += (p) => Log($"PeerConnectedEvent {p}");
            clientListener.PeerDisconnectedEvent += (p, r) =>
            {
                Log($"PeerDisconnectedEvent {p} => {r.Reason}");
            };
            clientListener.NetworkErrorEvent += (p, e) =>
            {
                Log($"NetworkErrorEvent {p} => {e}");
            };
            clientListener.PeerDisconnectedEvent += (p, r) =>
            {
                Log($"PeerDisconnectedEvent {p} => {r.Reason}");
            };
            //clientListener.NetworkLatencyUpdateEvent += ClientListener_NetworkLatencyUpdateEvent;

            _netPacketProcessor.Subscribe<VoicePaketSetup, NetPeer>((v, p) =>
            {
                Log("Setup received");
                var c = new VoicePaketConnectClient()
                {
                    ClientGUID = "64e274ec3a494fc6aabefa7f183c38fd",
                    TeamspeakClientID = 128,
                    TeamspeakClientName = "c0nnex",
                    TeamspeakID = "realgehim"
                };
                _netPacketProcessor.Send(p, c);
            });
            _netPacketProcessor.SubscribeNetSerializable<VoicePaketBatchUpdate, NetPeer>(onVoiceBatchUpdate);


            var Client = new NetManager(clientListener);
            Client.UnsyncedEvents = true;
            Client.UnconnectedMessagesEnabled = false;
            Client.Start();

            var cp = new VoicePaketConnectServer() { Secret = "9b2f87f5c79cce6609c2349793f4ffa0", Version = new Version(0,4,0,0), ClientGUID = "64e274ec3a494fc6aabefa7f183c38fd" };
            var nw = new NetDataWriter();
            cp.Serialize(nw);
            Client.Connect("127.0.0.1", 4498, nw);

            Console.ReadLine();

        }

        static void onVoiceBatchUpdate(VoicePaketBatchUpdate data, NetPeer peer)
        {
            Log("BatchUpdate " + data.BatchData.Count);
        }

            public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Console.WriteLine(String.Format(str, args));
        }
    }
}
