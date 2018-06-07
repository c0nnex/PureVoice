namespace TeamSpeakPlugin
{
    /// <summary>
    /// Message target of the plugin.
    /// </summary>
    public enum MessageTarget : int
    {

        /// <summary>
        /// Server.
        /// </summary>
        SERVER = 0,

        /// <summary>
        /// Channel.
        /// </summary>
        CHANNEL = 1,

        /// <summary>
        /// User.
        /// </summary>
        USER = 2
    }
}
