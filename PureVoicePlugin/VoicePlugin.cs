using PureVoice.DSP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    class VoicePlugin : TeamSpeakBase
    {
        /// <summary>
        /// Create a plugin instance.
        /// </summary>
        internal static VoiceClient.VoiceClient voiceClient;

        /// <summary>
        /// The functions from the TeamSpeak.
        /// </summary>

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
        public static int API_VERSION = 24;

        /// <summary>
        /// Author name.
        /// </summary>
        public static String Author = "c0nnex";

        /// <summary>
        /// Plugin description.
        /// </summary>
        public static String Description = "PureVoice Teamspeak 3 Plugin";

        public static String PluginID = "";
        public static string StartTag = "<PureVoice>";
        public static string EndTag = "</PureVoice>";

        private static string _BaseDirectory;
        public static string BaseDirectory => _BaseDirectory ?? (_BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PureVoice"));

        public static bool IsConnectedToVoiceServer { get; set; } = false;

        private static Dictionary<PluginReturnCode, string> _pluginReturnCodes;

        private static bool _FunctionsSet = false;
        private static object LockObject = new object();
        public static bool HasBeenInitialized = false;
        public static bool HasBeenActivatedBefore = false;
        private static bool VerboseLogging = false;

        internal static void Init([CallerMemberName] string tag = null, [CallerFilePath] string code = null, [CallerLineNumber] int codeLine = 0)
        {
            if (HasBeenInitialized)
                return;
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                try
                {
                    VerboseLogging |= File.Exists(Path.Combine(BaseDirectory, "VERBOSE"));
                }
                catch { }
                Log("Initializing {0}", PluginVersion);
                VerboseLog("{0} {1}:{2})", tag, code, codeLine);
                Connections = new ConcurrentDictionary<ulong, Connection>();
                if (_pluginReturnCodes == null)
                {
                    _pluginReturnCodes = new Dictionary<PluginReturnCode, string>();

                    foreach (PluginReturnCode pcode in Enum.GetValues(typeof(PluginReturnCode)))
                    {
                        var sb = new StringBuilder(255);
                        Functions.createReturnCode(PluginID, sb, 255);
                        _pluginReturnCodes[pcode] = sb.ToString();
                        Log("{0} => {1}", pcode, sb.ToString());
                    }
                }
                voiceClient = new VoiceClient.VoiceClient();
                voiceClient.OnDisconnected += VoiceClient_OnDisconnected;
                HasBeenInitialized = true;
                HasBeenActivatedBefore = true;
                PluginInitialize();
                return;
                IntPtr serverList = IntPtr.Zero;
                if (Functions.getServerConnectionHandlerList(ref serverList) == 0)
                {
                    var servers = TeamSpeakBase.ReadLongIDList(serverList, GetOrAddConnection);
                    Log("{0} already connected Servers", servers.Count);
                    servers.Where(c => c.ID != 0).ForEach(c => OnConnectionStatusChanged(c.ID, ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED, 0));
                }
                return;
            }
        }

        public static void PluginInitialize()
        {
            if (HasBeenActivatedBefore)
            {
                ulong serverID = Functions.getCurrentServerConnectionHandlerID();
                if (serverID > 0)
                {
                    VerboseLog("already connected to " + serverID);
                    int result = 0;
                    if (Check(Functions.getConnectionStatus(serverID, ref result)))
                    {
                        if ((ConnectionStatusEnum)result == ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED)
                            OnConnectionStatusChanged(serverID, ConnectionStatusEnum.STATUS_CONNECTION_ESTABLISHED, 0);
                    }
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log("FirstChance " + e.ExceptionObject?.ToString());
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is ObjectDisposedException)
            {
                VerboseLog("Unhandled " + e.Exception?.ToString());
                return;
            }
            Log("Unhandled " + e.Exception?.ToString());
            if (e.Exception.InnerException != null)
                Log("Inner" + e.Exception.InnerException?.ToString());
        }

        internal static string GetFilename(string name)
        {
            return Path.Combine(BaseDirectory, name);
        }

        internal static string GetFilename(string subDir, string name)
        {
            return Path.Combine(BaseDirectory, subDir, name);
        }

        internal static uint Hash(string stringToHash)
        {
            if (string.IsNullOrEmpty(stringToHash)) return 0;

            var characters = Encoding.UTF8.GetBytes(stringToHash.ToLower());

            uint hash = 0;

            foreach (var c in characters)
            {
                hash += c;
                hash += hash << 10;
                hash ^= hash >> 6;
            }

            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;

            return hash;
        }

        internal static void RegisterPluginID(string pluginStr)
        {
            PluginID = String.Copy(pluginStr);
            Log("PluginID = {0}", PluginID);
        }

        internal static string GetReturnCode(PluginReturnCode code)
        {
            VoicePlugin.Init();
            if (_pluginReturnCodes.TryGetValue(code, out var codeStr))
                return codeStr;
            return String.Empty;
        }

        internal static PluginReturnCode ReturnCodeToPluginReturnCode(string inStr)
        {
            VoicePlugin.Init();
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
                if (Connections != null && Connections.Count > 0)
                {
                    Connections.Values.ToList().Where(c => c.IsVoiceEnabled).ForEach(c => c.DisconnectVoiceServer());
                }
                Connections?.Clear();
                Connections = null;
                voiceClient = null;
                _FunctionsSet = false;
                HasBeenInitialized = false;
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


        //[Conditional("DEBUG")]
        internal static void VerboseLog(string str, params object[] args)
        {
            if (VerboseLogging)
                Log(str, args);
        }

        internal static void VerboseLog(Func<string> f)
        {
            if (VerboseLogging)
                Log(f());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Log(Func<string> f)
        {
            Log(f());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Log(string str, params object[] args)
        {
            try
            {
                lock (LockObject)
                {
                    if (args == null || args.Length == 0)
                    {
                        Trace.WriteLine(str);
                        if (_FunctionsSet)
                            Functions.logMessage(str, LogLevel.LogLevel_DEVEL, "PureVoice", (ulong)Thread.CurrentThread.ManagedThreadId);
                    }
                    else
                    {
                        str = String.Format(str, args);
                        Trace.WriteLine(str);
                        if (_FunctionsSet)
                            Functions.logMessage(str, LogLevel.LogLevel_DEVEL, "PureVoice", (ulong)Thread.CurrentThread.ManagedThreadId);

                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        internal static void SetFunctionPointer(TS3Functions functionsFromTeamSpeak)
        {
            Functions = functionsFromTeamSpeak;
            _FunctionsSet = true;
        }

        internal static bool HasConnection(ulong id)
        {
            VoicePlugin.Init();
            return Connections.ContainsKey(id);
        }

        internal static Connection GetConnection(ulong id)
        {
            VoicePlugin.Init();
            if (!Connections.TryGetValue(id, out var con) || !con.IsInitialized)
                return null;
            return con;
        }
        internal static Connection GetOrAddConnection(ulong id)
        {
            VoicePlugin.Init();
            var con = Connections.GetOrAdd(id, _ => new Connection(id));
            return con;
        }

        internal static Connection GetConnection(string guid)
        {
            VoicePlugin.Init();
            var con = Connections.Values.FirstOrDefault(c => c.GUID == guid);
            if (con == null || !con.IsInitialized)
                return null;
            return con;
        }


        internal static void OnConnectionStatusChanged(ulong serverConnectionHandlerID, ConnectionStatusEnum newStatus, uint errorNumber)
        {
            VoicePlugin.Init();
            if (newStatus == ConnectionStatusEnum.STATUS_DISCONNECTED)
            {
                Connections.TryRemove(serverConnectionHandlerID, out var unused);
                return;
            }
            var con = GetOrAddConnection(serverConnectionHandlerID);
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
                        con.Init();
                        con.LocalClient?.SetMetaData("PureVoice", PluginVersion.ToString());
                        voiceClient.AcceptConnections();
                    }
                    break;
                default:
                    break;
            }
            Log($"Server {serverConnectionHandlerID} {newStatus} {con.GUID}");
        }


    }
}
