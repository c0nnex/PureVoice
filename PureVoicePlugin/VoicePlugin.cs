using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TeamSpeakPlugin;

namespace PureVoice
{
    public class VoicePlugin
    {
        /// <summary>
        /// Create a plugin instance.
        /// </summary>
        internal static VoiceClient.VoiceClient voiceClient;

        /// <summary>
        /// The functions from the TeamSpeak.
        /// </summary>
        internal static TS3Functions Functions { get; private set; }

        private static ConcurrentDictionary<ulong, Connection> Connections;

        internal static bool InitializationDone { get; private set; }
#if false
        // TODO: Make configurable by ini file
        internal static string VersionCheckUrl = "http://cherrypad.cherryrp.de:28002/pluginVersion.txt";
#endif
        /// <summary>
        /// The plugin name.
        /// </summary>
        public static String Name = "PureVoice TeamSpeak plugin";

        /// <summary>
        /// The plugin version.
        /// </summary>
        public static Version PluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
        /// <summary>
        /// The API version.
        /// <para>Always check this, if error while loading plugin.</para>
        /// </summary>
        public static int API_VERSION = 23;

        /// <summary>
        /// Author name.
        /// </summary>
        public static String Author = "c0nnex";

        /// <summary>
        /// Plugin description.
        /// </summary>
        public static String Description = "PureVoice Teamspeak 3 Plugin\r\n* LiteNetLib (https://github.com/RevenantX/LiteNetLib)\r\n* Teamspeak C# Plugin API (https://github.com/kfarkasHU/TeamSpeak-plugin-API)";

        public static String PluginID = "";
        private static Dictionary<PluginReturnCode, string> _pluginReturnCodes;

        private static bool _FunctionsSet = false;
#if false
        internal static async System.Threading.Tasks.Task<int> InitAsync()
#else
        internal static int InitAsync()
#endif
        {
            Log("Initializing {0}", PluginVersion);

            Connections = new ConcurrentDictionary<ulong, Connection>();
            _pluginReturnCodes = new Dictionary<PluginReturnCode, string>();

            foreach (PluginReturnCode code in Enum.GetValues(typeof(PluginReturnCode)))
            {
                var sb = new StringBuilder(255);
                Functions.createReturnCode(PluginID, sb, 255);
                _pluginReturnCodes[code] = sb.ToString();
                Log("{0} => {1}", code, sb.ToString());
            }
#if false
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            try
            {
                string v = await client.GetStringAsync(VersionCheckUrl);
                if (!String.IsNullOrEmpty(v))
                {
                    if (Version.TryParse(v,out var reqVersion))
                    {
                        if (reqVersion > PluginVersion)
                        {
                            MessageBox.Show($"{Name} veraltet. Version {reqVersion} wird benötigt. Bitte aktualisieren.", "GT-MP Voice Plugin Fehler");
                            return -1;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log("Could not check version: {0}", ex.Message);
            }
#endif
            IntPtr serverList = IntPtr.Zero;
            if (Functions.getServerConnectionHandlerList(ref serverList) == 0)
            {
                var servers = TeamSpeakBase.ReadLongIDList(serverList, GetConnection);
                Log("{0} already connected Servers", servers.Count);
                servers.Where(c => c.ID != 0).ForEach(c => OnConnectionStatusChanged(c.ID, ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED, 0));
            }
            voiceClient = new VoiceClient.VoiceClient();
            voiceClient.OnDisconnected += VoiceClient_OnDisconnected;
            voiceClient.AcceptConnections();
            return 0;
        }

        internal static void RegisterPluginID(string pluginStr)
        {
            PluginID = String.Copy(pluginStr);
            Log("PluginID = {0}", PluginID);
        }

        internal static string GetReturnCode(PluginReturnCode code)
        {
            if (_pluginReturnCodes.TryGetValue(code, out var codeStr))
                return codeStr;
            return String.Empty;
        }

        internal static PluginReturnCode ReturnCodeToPluginReturnCode(string inStr)
        {
            foreach (var item in _pluginReturnCodes)
            {
                if (item.Value == inStr)
                    return item.Key;
            }
            return PluginReturnCode.NOCODE;
        }

        internal static void Shutdown()
        {
            Log("Shutdown {0}", PluginVersion);

            try
            {
                voiceClient?.Shutdown();
                if (Connections.Count > 0)
                {
                    Connections.Values.ToList().Where(c => c.IsVoiceEnabled).ForEach(c => c.DisconnectVoiceServer());
                }
                Connections.Clear();
                Connections = null;
                voiceClient = null;
                _FunctionsSet = false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private static void VoiceClient_OnDisconnected(object sender, VoiceClient.VoiceClient e)
        {
            return;
        }


        [Conditional("DEBUG")]
        internal static void VerboseLog(string str, params object[] args)
        {
            Log(str, args);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Log(string str, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                Debug.WriteLine(str);
                if (_FunctionsSet)
                    Functions.logMessage(str, LogLevel.LogLevel_DEVEL, "PureVoice", (ulong)Thread.CurrentThread.ManagedThreadId);

            }
            else
            {
                str = String.Format(str, args);
                Debug.WriteLine(str);
                if (_FunctionsSet)
                    Functions.logMessage(str, LogLevel.LogLevel_DEVEL, "PureVoice", (ulong)Thread.CurrentThread.ManagedThreadId);

            }
        }

        internal static void SetFunctionPointer(TS3Functions functionsFromTeamSpeak)
        {
            Functions = functionsFromTeamSpeak;
            _FunctionsSet = true;
        }

        internal static Connection GetConnection(ulong id)
        {
            return Connections.GetOrAdd(id, _ => new Connection(id));
        }

        internal static Connection GetConnection(string guid)
        {
            return Connections.Values.FirstOrDefault(c => c.GUID == guid);
        }

        internal static void OnConnectionStatusChanged(ulong serverConnectionHandlerID, ConnectionStatusEnum newStatus, uint errorNumber)
        {
            if (newStatus == ConnectionStatusEnum.STATUS_DISCONNECTED)
            {
                Connections.TryRemove(serverConnectionHandlerID, out var unused);
                return;
            }
            var con = GetConnection(serverConnectionHandlerID);
            con.Status = newStatus;
            switch (newStatus)
            {
                case ConnectionStatusEnum.STATUS_DISCONNECTED:
                    break;
                case ConnectionStatusEnum.STATUS_CONNECTING:
                    break;
                case ConnectionStatusEnum.STATUS_CONNECTED:
                    break;
                case ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHING:
                    break;
                case ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED:
                    {
                        con.Update();
                        con.DelayedCall(1000, (c) => c.Init());
                    }
                    break;
                default:
                    break;
            }
            Log($"Server {serverConnectionHandlerID} {newStatus} {con.GUID}");
        }

    }
}
