namespace GTMPVoice.VoiceClient.Model
{
    class VoicePaketSetup : IVoicePaketModel
    {
        public string ServerGUID { get; set; }

        public ulong DefaultChannel { get; set; }

        public ulong IngameChannel { get; set; }

        public string IngameChannelPassword { get; set; }

        /// <summary>
        /// Send callback to Server when Talkstatus changes?
        /// </summary>
        public bool EnableLipSync { get; set; }

        /// <summary>
        /// Channels a normal client may be in without being moved back while being In-Game (e.g. Support channels)
        /// </summary>
        public ulong[] AllowedOutGameChannels { get; set; }
    }
}