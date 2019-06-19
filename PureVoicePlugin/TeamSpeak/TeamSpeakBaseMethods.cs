using PureVoice;
using RGiesecke.DllExport;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
/// <summary>
/// Do not modify here.
/// </summary>
namespace TeamSpeakPlugin
{

    public class TeamSpeakBaseMethods
    {
        #region [ REQUIRED METHODS ]

        /// <summary>
        /// Send the plugin name to TeamSpeak.
        /// </summary>
        /// <returns>The plugin name.</returns>
        [DllExport]
        public static String ts3plugin_name()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return VoicePlugin.Name;
        }

        /// <summary>
        /// Send the plugin version to TeamSpeak.
        /// </summary>
        /// <returns>The plugin version.</returns>
        [DllExport]
        public static String ts3plugin_version()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return VoicePlugin.PluginVersion.ToString();
        }

        /// <summary>
        /// Send the api version to TeamSpeak.
        /// </summary>
        /// <returns>The API version.</returns>
        [DllExport]
        public static int ts3plugin_apiVersion()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return VoicePlugin.API_VERSION;
        }

        /// <summary>
        /// Send the plugin author to TeamSpeak.
        /// </summary>
        /// <returns>The author.</returns>
        [DllExport]
        public static String ts3plugin_author()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return VoicePlugin.Author;
        }

        /// <summary>
        /// Send the plugin description to TeamSpeak.
        /// </summary>
        /// <returns>The plugin description.</returns>
        [DllExport]
        public static String ts3plugin_description()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return VoicePlugin.Description;
        }

        /// <summary>
        /// Gets the functions from TeamSpeak.
        /// </summary>
        /// <param name="functionsFromTeamSpeak">The function pointers.</param>
        [DllExport]
        public static void ts3plugin_setFunctionPointers(TS3Functions functionsFromTeamSpeak)
        {
            try
            {
                Debug.WriteLine("SetFunctionPointers");
                VoicePlugin.SetFunctionPointer(functionsFromTeamSpeak);
                TeamSpeakBase.SetFunctionPointer(functionsFromTeamSpeak);
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <returns></returns>
        [DllExport]
        public static int ts3plugin_init()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
#if false
            return VoicePlugin.InitAsync().GetAwaiter().GetResult();
#else
            return VoicePlugin.InitAsync();
#endif
        }

        /// <summary>
        /// This triggered when the plugin shuts down.
        /// </summary>
        [DllExport]
        public static void ts3plugin_shutdown()
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            VoicePlugin.Shutdown();
        }

        [DllExport]
        public static void ts3plugin_registerPluginID(string pluginStr)
        {
            VoicePlugin.RegisterPluginID(pluginStr);
        }
#endregion [ REQUIRED METHODS ]

#region [ USED METHODS ]

        /// <summary>
        /// When connection status changed.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="newStatus">New connection status.</param>
        /// <param name="errorNumber">Error number.</param>
        [DllExport]
        public static void ts3plugin_onConnectStatusChangeEvent(ulong serverConnectionHandlerID, int newStatus, uint errorNumber)
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            VoicePlugin.OnConnectionStatusChanged(serverConnectionHandlerID, (ConnectionStatusEnum)newStatus, errorNumber);
        }

        /// <summary>
        /// When a client was updated.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">Updated client ID.</param>
        /// <param name="invokerID">Updater ID.</param>
        /// <param name="invokerName">Updater name.</param>
        /// <param name="invokerUniqueIdentifier">Updater UID.</param>
        [DllExport]
        public static void ts3plugin_onUpdateClientEvent(ulong serverConnectionHandlerID, ushort clientID, ushort invokerID, string invokerName, string invokerUniqueIdentifier)
        {
            VoicePlugin.VerboseLog("{0} {1}",MethodInfo.GetCurrentMethod().Name,clientID);
            VoicePlugin.GetConnection(serverConnectionHandlerID).GetKnownClient(clientID)?.Update();
        }

        /// <summary>
        /// When a client was moved.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">The moved client ID.</param>
        /// <param name="oldChannelID">The source channel ID.</param>
        /// <param name="newChannelID">The destination channel ID.</param>
        /// <param name="visibility">Is the client is visible now?</param>
        /// <param name="moveMessage">Move message.</param>
        [DllExport]
        public static void ts3plugin_onClientMoveEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, string moveMessage)
        {
            VoicePlugin.Log("{0} {1}: {2} => {3} v:{4} ", MethodInfo.GetCurrentMethod().Name, clientID, oldChannelID, newChannelID, visibility);
            if (newChannelID == 0) // Someone Disconnected
            {
                VoicePlugin.GetConnection(serverConnectionHandlerID).RemoveClient(clientID);
                return;
            }
            var cl = VoicePlugin.GetConnection(serverConnectionHandlerID).GetClient(clientID);
            cl.OnChannelChanged(newChannelID);
        }

        [DllExport]
        public static void ts3plugin_onClientMoveMovedEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, ushort moverID, string moverName, string moverUniqueIdentifier, string moveMessage)
        {
            VoicePlugin.Log("{0} {1}: {2} => {3} v:{4} ", MethodInfo.GetCurrentMethod().Name, clientID, oldChannelID, newChannelID, visibility);
            var cl = VoicePlugin.GetConnection(serverConnectionHandlerID).GetClient(clientID);
            if (oldChannelID == 0) // New User connected
                cl.Update();
            cl.OnChannelChanged(newChannelID);
        }

        [DllExport]
        public static void ts3plugin_onClientMoveTimeoutEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, string timeoutMessage)
        {
            VoicePlugin.Log("{0} {1}: {2} => {3} v:{4} ", MethodInfo.GetCurrentMethod().Name, clientID, oldChannelID, newChannelID, visibility);
            VoicePlugin.GetConnection(serverConnectionHandlerID).GetKnownClient(clientID)?.OnChannelChanged(newChannelID); 
        }

        [DllExport]
        public static int ts3plugin_onServerErrorEvent(ulong serverConnectionHandlerID, string errorMessage, uint error, string returnCode, string extraMessage)
        {
            if (!string.IsNullOrEmpty(returnCode))
            {
                if (error != 0)
                {
                    VoicePlugin.Log("ERROR: {0} ({1} {2} {3})", errorMessage, error, returnCode, extraMessage);
                    var rCode = VoicePlugin.ReturnCodeToPluginReturnCode(returnCode);
                    if (rCode == PluginReturnCode.JOINCHANNEL)
                    {
                        if (error != 770)
                            VoicePlugin.GetConnection(serverConnectionHandlerID).LocalClient.JoinDefaultChannel();
                    }
                }
                return 1;
            }
            return 0;
        }

        [DllExport]
        public static void ts3plugin_onTalkStatusChangeEvent(ulong serverConnectionHandlerID, int status, int isReceivedWhisper, ushort clientID)
        {
           // GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            VoicePlugin.GetConnection(serverConnectionHandlerID).ClientTalkStatusChanged(clientID, status, isReceivedWhisper);
        }

        /// <summary>
        /// When a client changed his/her display name.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">Client ID.</param>
        /// <param name="displayName">New display name.</param>
        /// <param name="uniqueClientIdentifier">Client UID.</param>
        [DllExport]
        public static void ts3plugin_onClientDisplayNameChanged(ulong serverConnectionHandlerID, ushort clientID, string displayName, string uniqueClientIdentifier)
        {
            VoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            VoicePlugin.GetConnection(serverConnectionHandlerID).GetClient(clientID).SetName(displayName);
        }

        /// <summary>
        /// When a variable of the local client changed
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="flag">Variable thet chaned</param>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        [DllExport]
        public static void ts3plugin_onClientSelfVariableUpdateEvent(ulong serverConnectionHandlerID, int flag, string oldValue, string newValue)
        {
            VoicePlugin.VerboseLog("{0} {1} o:{2} n:{3}",MethodInfo.GetCurrentMethod().Name,(ClientProperty)flag,oldValue,newValue);
            VoicePlugin.GetConnection(serverConnectionHandlerID).LocalClient?.OnSelfVariableChanged((ClientProperty)flag, oldValue, newValue);
        }

        #endregion
#if DEBUG_TS3 || false
        #region [ ACTION METHODS ]

        /**
         * TODO
         * ====
         * Add subregions here
         * Method summaries and exports
         * */

        #region [ MESSAGES ]
        /// <summary>
        /// When a message appears from server, channel or private chat.
        /// </summary>
        /// <param name="serverConnectionHandlerID">The server connectionn.</param>
        /// <param name="targetMode">Target mode (see in enums)</param>
        /// <param name="toID">Target user/channel/server ID.</param>
        /// <param name="fromID">Sender ID.</param>
        /// <param name="fromName">Sender name.</param>
        /// <param name="fromUniqueIdentifier">Sender UID.</param>
        /// <param name="message">The message text.</param>
        /// <param name="ffIgnored">Is the sender is ignored?</param>
        /// <returns></returns>
        [DllExport]
        public static uint ts3plugin_onTextMessageEvent(ulong serverConnectionHandlerID, MessageTarget targetMode, ushort toID, ushort fromID, string fromName, string fromUniqueIdentifier, string message, bool ffIgnored)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return 0;
        }

        /// <summary>
        /// When a private message appears.
        /// </summary>
        /// <returns></returns>
        [DllExport]
        public static uint ts3plugin_onPrivateTextMessageEvent(ulong serverConnectionHandlerID, MessageTarget targetMode, ushort toID, ushort fromID, string fromName, string fromUniqueIdentifier, string message, bool ffIgnored)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            // In here we knows it is a private message.
            return 0;
        }

        #endregion [ MESSAGES ] 

        #region [ KICKS AND BANS ]

        /// <summary>
        /// When someone kicked from the channel.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">The kicked user ID.</param>
        /// <param name="oldChannelID">The source channel ID.</param>
        /// <param name="newChannelID">The destination channel ID.</param>
        /// <param name="visibility">(bool) Is the user in the new channel visible?</param>
        /// <param name="kickerID">The kicker ID.</param>
        /// <param name="kickerName">The kicker name.</param>
        /// <param name="kickerUniqueIdentifier">The kicked UID.</param>
        /// <param name="kickMessage">The kick message.</param>
        [DllExport]
        public static void ts3plugin_onClientKickFromChannelEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, ushort kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);

        }

        /// <summary>
        /// When someone kicked from the server.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">The kicked  client ID.</param>
        /// <param name="oldChannelID">The source channel ID.</param>
        /// <param name="newChannelID">New channel ID. (may drops error)</param>
        /// <param name="visibility">(bool) Is the user in the new channel visible?</param>
        /// <param name="kickerID">The kicker ID.</param>
        /// <param name="kickerName">The kicker name.</param>
        /// <param name="kickerUniqueIdentifier">The kicked UID.</param>
        /// <param name="kickMessage">The kick message.</param>
        [DllExport]
        public static void ts3plugin_onClientKickFromServerEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, ushort kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When a client banned from the server.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">The kicked client ID.</param>
        /// <param name="oldChannelID">The source channel ID.</param>
        /// <param name="newChannelID">The destination channel ID. (It may be null.)</param>
        /// <param name="visibility">Is the client visible in new channel?</param>
        /// <param name="kickerID">Banner ID.</param>
        /// <param name="kickerName">Banner name.</param>
        /// <param name="kickerUniqueIdentifier">Banner UID.</param>
        /// <param name="time">Ban time.</param>
        /// <param name="kickMessage">Ban message.</param>
        [DllExport]
        public static void ts3plugin_onClientBanFromServerEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, ushort kickerID, string kickerName, string kickerUniqueIdentifier, ulong time, string kickMessage)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        #endregion [ KICKS AND BANS ]

        #region [ CHANNEL CREATE, EDIT AND DELETE ]

        /// <summary>
        /// When someone creates a new channel.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">Created channel's ID.</param>
        /// <param name="channelParentID">Created channel's parent channel's ID.</param>
        [DllExport]
        public static void ts3plugin_onNewChannelEvent(ulong serverConnectionHandlerID, ulong channelID, ulong channelParentID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// After the new channel has been created.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">The new channel's ID.</param>
        /// <param name="channelParentID">The new channel's parent channel's ID.</param>
        /// <param name="invokerID">The creator ID.</param>
        /// <param name="invokerName">The creator name.</param>
        /// <param name="invokerUniqueIdentifier">The creator UID.</param>
        [DllExport]
        public static void ts3plugin_onNewChannelCreatedEvent(ulong serverConnectionHandlerID, ulong channelID, ulong channelParentID, ushort invokerID, string invokerName, string invokerUniqueIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When a channel was deleted.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">The deleted channel ID.</param>
        /// <param name="invokerID">The deletor ID.</param>
        /// <param name="invokerName">The deletor name.</param>
        /// <param name="invokerUniqueIdentifier">The deletor UID.</param>
        [DllExport]
        public static void ts3plugin_onDelChannelEvent(ulong serverConnectionHandlerID, ulong channelID, ushort invokerID, string invokerName, string invokerUniqueIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When a channel was moved.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">Moved channel ID.</param>
        /// <param name="newChannelParentID">Moved channel new parent's ID.</param>
        /// <param name="invokerID">Mover ID.</param>
        /// <param name="invokerName">Mover name.</param>
        /// <param name="invokerUniqueIdentifier">Mover UID.</param>
        [DllExport]
        public static void ts3plugin_onChannelMoveEvent(ulong serverConnectionHandlerID, ulong channelID, ulong newChannelParentID, ushort invokerID, string invokerName, string invokerUniqueIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone updates the channel.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">The channel ID.</param>
        [DllExport]
        public static void ts3plugin_onUpdateChannelEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverConnectionHandlerID"></param>
        /// <param name="channelID"></param>
        /// <param name="invokerID"></param>
        /// <param name="invokerName"></param>
        /// <param name="invokerUniqueIdentifier"></param>
        [DllExport]
        public static void ts3plugin_onUpdateChannelEditedEvent(ulong serverConnectionHandlerID, ulong channelID, ushort invokerID, string invokerName, string invokerUniqueIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        #endregion [ CHANNEL CREATE AND EDIT ]




        [DllExport]
        public static void ts3plugin_onClientMoveSubscriptionEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientMoveTimeoutEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, int visibility, string timeoutMessage)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }


        [DllExport]
        public static void ts3plugin_onClientIDsEvent(ulong serverConnectionHandlerID, string uniqueClientIdentifier, ushort clientID, string clientName)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientIDsFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone edits the server.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="editerID">The editer user ID.</param>
        /// <param name="editerName">The editer user name.</param>
        /// <param name="editerUniqueIdentifier">The editer user UID.</param>
        [DllExport]
        public static void ts3plugin_onServerEditedEvent(ulong serverConnectionHandlerID, ushort editerID, string editerName, string editerUniqueIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerUpdatedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }


        /// <summary>
        /// When the server stops.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="shutdownMessage">The suhtdown message.</param>
        [DllExport]
        public static void ts3plugin_onServerStopEvent(ulong serverConnectionHandlerID, string shutdownMessage)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }


        [DllExport]
        public static void ts3plugin_onConnectionInfoEvent(ulong serverConnectionHandlerID, ushort clientID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerConnectionInfoEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelSubscribeEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelSubscribeFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelUnsubscribeEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelUnsubscribeFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone changes the channel description.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">The channel ID.</param>
        [DllExport]
        public static void ts3plugin_onChannelDescriptionUpdateEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone changes the channel password.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">The channel ID.</param>
        [DllExport]
        public static void ts3plugin_onChannelPasswordChangedEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPlaybackShutdownCompleteEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onSoundDeviceListChangedEvent(string modeID, int playOrCap)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }
#if false
        [DllExport]
        public static void ts3plugin_onEditPlaybackVoiceDataEvent(ulong serverConnectionHandlerID, ushort clientID, short samples, int sampleCount, int channels)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onEditPostProcessVoiceDataEvent(ulong serverConnectionHandlerID, ushort clientID, short samples, int sampleCount, int channels, uint channelSpeakerArray, uint channelFillMask)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onEditMixedPlaybackVoiceDataEvent(ulong serverConnectionHandlerID, short samples, int sampleCount, int channels, uint channelSpeakerArray, uint channelFillMask)
        {
            
        }

        [DllExport]
        public static void ts3plugin_onEditCapturedVoiceDataEvent(ulong serverConnectionHandlerID, short samples, int sampleCount, int channels, int edited)
        {
           
        }

        [DllExport]
        public static void ts3plugin_onCustom3dRolloffCalculationClientEvent(ulong serverConnectionHandlerID, ushort clientID, float distance, ref float volume)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            //GTMPVoicePlugin.Log("ROllOff { GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);0} { GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);1} { GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);2}", clientID, distance, volume);
            //GTMPVoicePlugin.GetConnection(serverConnectionHandlerID).GetClient(clientID).ModifyVolume(distance, ref volume);
        }

        [DllExport]
        public static void ts3plugin_onCustom3dRolloffCalculationWaveEvent(ulong serverConnectionHandlerID, ulong waveHandle, float distance, float volume)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }
#endif
        /*
        [DllExport]
        public static void ts3plugin_onUserLoggingMessageEvent(string logMessage, int logLevel, string logChannel, ulong logID, string logTime, string completeLogString)
        { GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }
        */
        [DllExport]
        public static int ts3plugin_onClientPokeEvent(ulong serverConnectionHandlerID, ushort fromClientID, string pokerName, string pokerUniqueIdentity, string message, int ffIgnored)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return 0;
        }

        

        [DllExport]
        public static void ts3plugin_onFileListEvent(ulong serverConnectionHandlerID, ulong channelID, string path, string name, ulong size, ulong datetime, int type, ulong incompletesize, string returnCode)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onFileListFinishedEvent(ulong serverConnectionHandlerID, ulong channelID, string path)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onFileInfoEvent(ulong serverConnectionHandlerID, ulong channelID, string name, ulong size, ulong datetime)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupListEvent(ulong serverConnectionHandlerID, ulong serverGroupID, string name, int type, int iconID, int saveDB)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupListFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupByClientIDEvent(ulong serverConnectionHandlerID, string name, ulong serverGroupList, ulong clientDatabaseID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupPermListEvent(ulong serverConnectionHandlerID, ulong serverGroupID, uint permissionID, int permissionValue, int permissionNegated, int permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupPermListFinishedEvent(ulong serverConnectionHandlerID, ulong serverGroupID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupClientListEvent(ulong serverConnectionHandlerID, ulong serverGroupID, ulong clientDatabaseID, string clientNameIdentifier, string clientUniqueID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelGroupListEvent(ulong serverConnectionHandlerID, ulong channelGroupID, string name, int type, int iconID, int saveDB)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelGroupListFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelGroupPermListEvent(ulong serverConnectionHandlerID, ulong channelGroupID, uint permissionID, int permissionValue, int permissionNegated, int permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelGroupPermListFinishedEvent(ulong serverConnectionHandlerID, ulong channelGroupID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onChannelPermListEvent(ulong serverConnectionHandlerID, ulong channelID, uint permissionID, int permissionValue, int permissionNegated, int permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }


        [DllExport]
        public static void ts3plugin_onChannelPermListFinishedEvent(ulong serverConnectionHandlerID, ulong channelID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When client server permission loading.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientDatabaseID">Client ID in database</param>
        /// <param name="permissionID">ID of the permission.</param>
        /// <param name="permissionValue">Value of the permission.</param>
        /// <param name="permissionNegated">Is the permission negated?</param>
        /// <param name="permissionSkip">Is the permission skipped?</param>
        [DllExport]
        public static void ts3plugin_onClientPermListEvent(ulong serverConnectionHandlerID, ulong clientDatabaseID, uint permissionID, int permissionValue, int permissionNegated, int permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When client server permision finished loading.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientDatabaseID">Client ID in database.</param>
        [DllExport]
        public static void ts3plugin_onClientPermListFinishedEvent(ulong serverConnectionHandlerID, ulong clientDatabaseID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When client channel permissions loading.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">Channel ID.</param>
        /// <param name="clientDatabaseID">Client ID in database.</param>
        /// <param name="permissionID">ID of the permission.</param>
        /// <param name="permissionValue">Value of the permission.</param>
        /// <param name="permissionNegated">Is the permission negated?</param>
        /// <param name="permissionSkip">Is the permission skipped?</param>
        [DllExport]
        public static void ts3plugin_onChannelClientPermListEvent(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, uint permissionID, int permissionValue, bool permissionNegated, bool permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }


        /// <summary>
        /// When client channel permissions finished loading.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="channelID">Client ID.</param>
        /// <param name="clientDatabaseID">Client ID in the database.</param>
        [DllExport]
        public static void ts3plugin_onChannelClientPermListFinishedEvent(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When client channel group changed.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler.</param>
        /// <param name="channelGroupID">The new channel group ID.</param>
        /// <param name="channelID">Channel ID.</param>
        /// <param name="clientID">Client ID.</param>
        /// <param name="invokerClientID">The granter ID.</param>
        /// <param name="invokerName">Tha granter name.</param>
        /// <param name="invokerUniqueIdentity">The granter UID.</param>
        [DllExport]
        public static void ts3plugin_onClientChannelGroupChangedEvent(ulong serverConnectionHandlerID, ulong channelGroupID, ulong channelID, ushort clientID, ushort invokerClientID, string invokerName, string invokerUniqueIdentity)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static int ts3plugin_onServerPermissionErrorEvent(ulong serverConnectionHandlerID, string errorMessage, uint error, string returnCode, uint failedPermissionID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return 0;
        }

        [DllExport]
        public static void ts3plugin_onPermissionListGroupEndIDEvent(ulong serverConnectionHandlerID, uint groupEndID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPermissionListEvent(ulong serverConnectionHandlerID, uint permissionID, string permissionName, string permissionDescription)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPermissionListFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPermissionOverviewEvent(ulong serverConnectionHandlerID, ulong clientDatabaseID, ulong channelID, int overviewType, ulong overviewID1, ulong overviewID2, uint permissionID, int permissionValue, int permissionNegated, int permissionSkip)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPermissionOverviewFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupClientAddedEvent(ulong serverConnectionHandlerID, ushort clientID, string clientName, string clientUniqueIdentity, ulong serverGroupID, ushort invokerClientID, string invokerName, string invokerUniqueIdentity)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerGroupClientDeletedEvent(ulong serverConnectionHandlerID, ushort clientID, string clientName, string clientUniqueIdentity, ulong serverGroupID, ushort invokerClientID, string invokerName, string invokerUniqueIdentity)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientNeededPermissionsEvent(ulong serverConnectionHandlerID, uint permissionID, int permissionValue)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientNeededPermissionsFinishedEvent(ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onFileTransferStatusEvent(ushort transferID, uint status, string statusMessage, ulong remotefileSize, ulong serverConnectionHandlerID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientChatClosedEvent(ulong serverConnectionHandlerID, ushort clientID, string clientUniqueIdentity)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientChatComposingEvent(ulong serverConnectionHandlerID, ushort clientID, string clientUniqueIdentity)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Log to the server.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="logMsg">Log message.</param>
        [DllExport]
        public static void ts3plugin_onServerLogEvent(ulong serverConnectionHandlerID, string logMsg)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone visits the server log, and it finished loading.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="lastPos">Last position.</param>
        /// <param name="fileSize">Log file size.</param>
        [DllExport]
        public static void ts3plugin_onServerLogFinishedEvent(ulong serverConnectionHandlerID, ulong lastPos, ulong fileSize)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onMessageListEvent(ulong serverConnectionHandlerID, ulong messageID, string fromClientUniqueIdentity, string subject, ulong timestamp, int flagRead)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onMessageGetEvent(ulong serverConnectionHandlerID, ulong messageID, string fromClientUniqueIdentity, string subject, string message, ulong timestamp)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When query to server (get client ID by UID).
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="uniqueClientIdentifier">Client UID.</param>
        /// <param name="clientDatabaseID">Client ID.</param>
        [DllExport]
        public static void ts3plugin_onClientDBIDfromUIDEvent(ulong serverConnectionHandlerID, string uniqueClientIdentifier, ulong clientDatabaseID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When query to server (get client name by UID)
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="uniqueClientIdentifier">Client UID.</param>
        /// <param name="clientDatabaseID">Client ID.</param>
        /// <param name="clientNickName">Client name.</param>
        [DllExport]
        public static void ts3plugin_onClientNamefromUIDEvent(ulong serverConnectionHandlerID, string uniqueClientIdentifier, ulong clientDatabaseID, string clientNickName)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientNamefromDBIDEvent(ulong serverConnectionHandlerID, string uniqueClientIdentifier, ulong clientDatabaseID, string clientNickName)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone visit complains list.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="targetClientDatabaseID">The targeted user ID.</param>
        /// <param name="targetClientNickName">The targeted user name.</param>
        /// <param name="fromClientDatabaseID">The sender ID.</param>
        /// <param name="fromClientNickName">The sender name.</param>
        /// <param name="complainReason">Reason.</param>
        /// <param name="timestamp">When?</param>
        [DllExport]
        public static void ts3plugin_onComplainListEvent(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, string targetClientNickName, ulong fromClientDatabaseID, string fromClientNickName, string complainReason, ulong timestamp)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When someone visit ban list.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="banid">Ban ID.</param>
        /// <param name="ip">Banned IP.</param>
        /// <param name="name">Banned name.</param>
        /// <param name="uid">Banned UID.</param>
        /// <param name="creationTime">Creation date.</param>
        /// <param name="durationTime">Ban duration.</param>
        /// <param name="invokerName">Banner name.</param>
        /// <param name="invokercldbid">???</param>
        /// <param name="invokeruid">Banner UID.</param>
        /// <param name="reason">Ban reason.</param>
        /// <param name="numberOfEnforcements">???</param>
        /// <param name="lastNickName">Banned last nickname.</param>
        [DllExport]
        public static void ts3plugin_onBanListEvent(ulong serverConnectionHandlerID, ulong banid, string ip, string name, string uid, string myTSId, ulong creationTime, ulong durationTime, string invokerName,
                                      ulong invokercldbid, string invokeruid, string reason, int numberOfEnforcements, string lastNickName)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onClientServerQueryLoginPasswordEvent(ulong serverConnectionHandlerID, string loginPassword)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onPluginCommandEvent(ulong serverConnectionHandlerID, string pluginName, string pluginCommand, ushort invokerClientID, string invokerName, string invokerUniqueIdentity )
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);

        }

        [DllExport]
        public static void ts3plugin_onIncomingClientQueryEvent(ulong serverConnectionHandlerID, string commandText)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onServerTemporaryPasswordListEvent(ulong serverConnectionHandlerID, string clientNickname, string uniqueClientIdentifier, string description, string password, ulong timestampStart, ulong timestampEnd, ulong targetChannelID, string targetChannelPW)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// When a client uplaod a new avatar.
        /// </summary>
        /// <param name="serverConnectionHandlerID">Server connection handler ID.</param>
        /// <param name="clientID">Client ID.</param>
        /// <param name="avatarPath">Path to the avatar.</param>
        [DllExport]
        public static void ts3plugin_onAvatarUpdated(ulong serverConnectionHandlerID, ushort clientID, string avatarPath)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onMenuItemEvent(ulong serverConnectionHandlerID, PluginMenuTypeEnum type, int menuItemID, ulong selectedItemID)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onHotkeyEvent(string keyword)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static void ts3plugin_onHotkeyRecordedEvent(string keyword, string key)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
        }

        [DllExport]
        public static string ts3plugin_keyDeviceName(string keyIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return null;
        }

        [DllExport]
        public static string ts3plugin_displayKeyText(string keyIdentifier)
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return null;
        }

        [DllExport]
        public static string ts3plugin_keyPrefix()
        {
            GTMPVoicePlugin.VerboseLog(MethodInfo.GetCurrentMethod().Name);
            return null;
        }



        #endregion [ ACTION METHODS ]
#endif

    }
}
