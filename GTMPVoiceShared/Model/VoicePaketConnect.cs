using System;

namespace GTMPVoice.VoiceClient.Model
{
    class VoicePaketConnectServer : IVoicePaketModel
    {
        public string Secret { get; set; }
        public Version Version { get; set; }
        public string ClientGUID { get; set; }
    }

    class VoicePaketConnectClient : IVoicePaketModel
    {
        public string ClientGUID { get; set; }
        public string TeamspeakID{ get; set; }
        public ushort TeamspeakClientID { get; set; }
    }

    class VoicePaketConfigureClient : IVoicePaketModel
    {
        public string Nickname { get; set; }
        public bool CanLeaveIngameChannel { get; set; }
    }
}