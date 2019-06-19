using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net.Sockets;
using System.Threading;

namespace PureVoiceTest
{
    class Program
    {
        static PureVoice.Server.VoiceServer _vs;

        static void Main(string[] args)
        {
            PureVoice.Server.VoiceServer.SetMinLogLevel(PureVoice.Server.LogLevel.TRACE);
            _vs = new PureVoice.Server.VoiceServer(1234, "geheim", "GusXJvzOPDkfrtsfCXTsxqAOLfI=", new Version(0, 2, 1265, 371), 2, 15, "123", true);
            
            _vs.VoiceClientConnected += _vs_VoiceClientConnected;
            
            UdpClient uc = new UdpClient();

            EventBasedNetListener clientListener = new EventBasedNetListener();
            clientListener.ConnectionRequestEvent += (p) => p.Reject();
            var _server = new NetManager(clientListener)
            {
                UnsyncedEvents = true,
                UnconnectedMessagesEnabled = false,
                DiscoveryEnabled = false
            };
            _server.Start();

            while (!Console.KeyAvailable)
            {
                NetDataWriter nw = new NetDataWriter();
                nw.Put("GTMPVOICE");
                nw.Put(1);
                nw.Put("127.0.0.1");
                nw.Put(1234);
                nw.Put("geheim");
                nw.Put("clientGUID");
                nw.Put("0.2.1265.371");
                while (true)
                {
                    try
                    {
                        //Logger.Trace("Sending Ping");
                        _server.SendUnconnectedMessage(nw.Data, new NetEndPoint("127.0.0.1", 4239));
                        Thread.Sleep(4000);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
        }

        private static void _vs_VoiceClientConnected(string clientGUID, string teamspeakID, ushort teamspeakClientID, long connectionID, string clientName, bool micMuted, bool speakersMuted)
        {
            _vs.ConfigureClient(connectionID, "Halifax", false);
            _vs.SendUpdate(connectionID, new PureVoice.VoiceLocationInformation(teamspeakClientID) { });
       
        }
    }
}
