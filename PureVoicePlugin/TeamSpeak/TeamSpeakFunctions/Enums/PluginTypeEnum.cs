namespace TeamSpeakPlugin
{
    /// <summary>
    /// Plugin types.
    /// </summary>
    public enum PluginType : int
    {

        /// <summary>
        /// Server plugin.
        /// </summary>
        PLUGIN_SERVER = 0,

        /// <summary>
        /// Channel plugin.
        /// </summary>
        PLUGIN_CHANNEL = 1,

        /// <summary>
        /// Client side plugin.
        /// </summary>
        PLUGIN_CLIENT = 2
    }
}
