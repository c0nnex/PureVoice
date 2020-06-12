using System;
using System.Runtime.InteropServices;
using System.Text;
using TeamSpeakPlugin;


#region [ DELEGATED METHODS ]

/// Return Type: uint
///result: char**
public delegate uint TS3Functions_getClientLibVersion(ref IntPtr result);

/// Return Type: uint
///result: uint64*
public delegate uint TS3Functions_getClientLibVersionNumber(ref ulong result);

/// Return Type: uint
///port: int
///result: uint64*
public delegate uint TS3Functions_spawnNewServerConnectionHandler(int port, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_destroyServerConnectionHandler(ulong serverConnectionHandlerID);

/// Return Type: uint
///errorCode: uint
///error: char**
public delegate uint TS3Functions_getErrorMessage(uint errorCode, ref IntPtr error);

/// Return Type: uint
///pointer: void*
public delegate uint TS3Functions_freeMemory(IntPtr pointer);

/// Return Type: uint
///logMessage: char*
///severity: LogLevel
///channel: char*
///logID: uint64->unsigned __int64
public delegate uint TS3Functions_logMessage([In] [MarshalAs(UnmanagedType.LPStr)] string logMessage, LogLevel severity, [In] [MarshalAs(UnmanagedType.LPStr)] string channel, ulong logID);

/// Return Type: uint
///modeID: char*
///result: char****
public delegate uint TS3Functions_getPlaybackDeviceList([In] [MarshalAs(UnmanagedType.LPStr)] string modeID, ref IntPtr result);

/// Return Type: uint
///result: char***
public delegate uint TS3Functions_getPlaybackModeList(ref IntPtr result);

/// Return Type: uint
///modeID: char*
///result: char****
public delegate uint TS3Functions_getCaptureDeviceList([In] [MarshalAs(UnmanagedType.LPStr)] string modeID, ref IntPtr result);

/// Return Type: uint
///result: char***
public delegate uint TS3Functions_getCaptureModeList(ref IntPtr result);

/// Return Type: uint
///modeID: char*
///result: char***
public delegate uint TS3Functions_getDefaultPlaybackDevice([In] [MarshalAs(UnmanagedType.LPStr)] string modeID, ref IntPtr result);

/// Return Type: uint
///result: char**
public delegate uint TS3Functions_getDefaultPlayBackMode(ref IntPtr result);

/// Return Type: uint
///modeID: char*
///result: char***
public delegate uint TS3Functions_getDefaultCaptureDevice([In] [MarshalAs(UnmanagedType.LPStr)] string modeID, ref IntPtr result);

/// Return Type: uint
///result: char**
public delegate uint TS3Functions_getDefaultCaptureMode(ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///modeID: char*
///playbackDevice: char*
public delegate uint TS3Functions_openPlaybackDevice(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string modeID, [In] [MarshalAs(UnmanagedType.LPStr)] string playbackDevice);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///modeID: char*
///captureDevice: char*
public delegate uint TS3Functions_openCaptureDevice(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string modeID, [In] [MarshalAs(UnmanagedType.LPStr)] string captureDevice);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: char**
///isDefault: int
public delegate uint TS3Functions_getCurrentPlaybackDeviceName(ulong serverConnectionHandlerID, ref IntPtr result, ref int isDefault);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: char**
public delegate uint TS3Functions_getCurrentPlayBackMode(ulong serverConnectionHandlerID, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: char**
///isDefault: int
public delegate uint TS3Functions_getCurrentCaptureDeviceName(ulong serverConnectionHandlerID, ref IntPtr result, ref int isDefault);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: char**
public delegate uint TS3Functions_getCurrentCaptureMode(ulong serverConnectionHandlerID, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_initiateGracefulPlaybackShutdown(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_closePlaybackDevice(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_closeCaptureDevice(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_activateCaptureDevice(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///path: char*
///loop: int
///waveHandle: uint64*
public delegate uint TS3Functions_playWaveFileHandle(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string path, int loop, ref ulong waveHandle);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///waveHandle: uint64->unsigned __int64
///pause: int
public delegate uint TS3Functions_pauseWaveFileHandle(ulong serverConnectionHandlerID, ulong waveHandle, int pause);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///waveHandle: uint64->unsigned __int64
public delegate uint TS3Functions_closeWaveFileHandle(ulong serverConnectionHandlerID, ulong waveHandle);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///path: char*
public delegate uint TS3Functions_playWaveFile(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string path);

/// Return Type: uint
///deviceID: char*
///deviceDisplayName: char*
///capFrequency: int
///capChannels: int
///playFrequency: int
///playChannels: int
public delegate uint TS3Functions_registerCustomDevice([In] [MarshalAs(UnmanagedType.LPStr)] string deviceID, [In] [MarshalAs(UnmanagedType.LPStr)] string deviceDisplayName, int capFrequency, int capChannels, int playFrequency, int playChannels);

/// Return Type: uint
///deviceID: char*
public delegate uint TS3Functions_unregisterCustomDevice([In] [MarshalAs(UnmanagedType.LPStr)] string deviceID);

/// Return Type: uint
///deviceName: char*
///buffer: short
///samples: int
public delegate uint TS3Functions_processCustomCaptureData([In] [MarshalAs(UnmanagedType.LPStr)] string deviceName, ref short buffer, int samples);

/// Return Type: uint
///deviceName: char*
///buffer: short
///samples: int
public delegate uint TS3Functions_acquireCustomPlaybackData([In] [MarshalAs(UnmanagedType.LPStr)] string deviceName, ref short buffer, int samples);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///result: float
public delegate uint TS3Functions_getPreProcessorInfoValueFloat(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, ref float result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///result: char**
public delegate uint TS3Functions_getPreProcessorConfigValue(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///value: char*
public delegate uint TS3Functions_setPreProcessorConfigValue(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, [In] [MarshalAs(UnmanagedType.LPStr)] string value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///result: char**
public delegate uint TS3Functions_getEncodeConfigValue(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///result: float
public delegate uint TS3Functions_getPlaybackConfigValueAsFloat(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, ref float result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ident: char*
///value: char*
public delegate uint TS3Functions_setPlaybackConfigValue(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ident, [In] [MarshalAs(UnmanagedType.LPStr)] string value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///value: float
public delegate uint TS3Functions_setClientVolumeModifier(ulong serverConnectionHandlerID, ushort clientID, float value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_startVoiceRecording(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_stopVoiceRecording(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///position: Vector3*
///forward: Vector3*
///up: Vector3*
public delegate uint TS3Functions_systemset3DListenerAttributes(ulong serverConnectionHandlerID, [In] TSVector position, [In] TSVector forward, [In] TSVector up);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///waveHandle: uint64->unsigned __int64
///position: Vector3*
public delegate uint TS3Functions_set3DWaveAttributes(ulong serverConnectionHandlerID, ulong waveHandle, [In] TSVector position);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///distanceFactor: float
///rolloffScale: float
public delegate uint TS3Functions_systemset3DSettings(ulong serverConnectionHandlerID, float distanceFactor, float rolloffScale);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///position: Vector3*
public delegate uint TS3Functions_channelset3DAttributes(ulong serverConnectionHandlerID, ushort clientID, [In] TSVector position);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///identity: char*
///ip: char*
///port: uint
///nickname: char*
///defaultChannelArray: char**
///defaultChannelPassword: char*
///serverPassword: char*
public delegate uint TS3Functions_startConnection(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string identity, [In] [MarshalAs(UnmanagedType.LPStr)] string ip, uint port, [In] [MarshalAs(UnmanagedType.LPStr)] string nickname, ref IntPtr defaultChannelArray, [In] [MarshalAs(UnmanagedType.LPStr)] string defaultChannelPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string serverPassword);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///quitMessage: char*
public delegate uint TS3Functions_stopConnection(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string quitMessage);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///newChannelID: uint64->unsigned __int64
///password: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientMove(ulong serverConnectionHandlerID, ushort clientID, ulong newChannelID, [In] [MarshalAs(UnmanagedType.LPStr)] string password, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///returnCode: char*
public delegate uint TS3Functions_requestClientVariables(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///kickReason: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientKickFromChannel(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string kickReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///kickReason: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientKickFromServer(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string kickReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///force: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelDelete(ulong serverConnectionHandlerID, ulong channelID, int force, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///newChannelParentID: uint64->unsigned __int64
///newChannelOrder: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelMove(ulong serverConnectionHandlerID, ulong channelID, ulong newChannelParentID, ulong newChannelOrder, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///message: char*
///targetClientID: anyID->unsigned short
///returnCode: char*
public delegate uint TS3Functions_requestSendPrivateTextMsg(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string message, ushort targetClientID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///message: char*
///targetChannelID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestSendChannelTextMsg(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string message, ulong targetChannelID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///message: char*
///returnCode: char*
public delegate uint TS3Functions_requestSendServerTextMsg(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string message, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///returnCode: char*
public delegate uint TS3Functions_requestConnectionInfo(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///targetChannelIDArray: uint64*
///targetClientIDArray: anyID*
///returnCode: char*
public delegate uint TS3Functions_requestClientSetWhisperList(ulong serverConnectionHandlerID, ushort clientID, ref ulong targetChannelIDArray, ref ushort targetClientIDArray, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelIDArray: uint64*
///returnCode: char*
public delegate uint TS3Functions_requestChannelSubscribe(ulong serverConnectionHandlerID, ref ulong channelIDArray, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelSubscribeAll(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelIDArray: uint64*
///returnCode: char*
public delegate uint TS3Functions_requestChannelUnsubscribe(ulong serverConnectionHandlerID, ref ulong channelIDArray, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelUnsubscribeAll(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelDescription(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientIDArray: anyID*
///returnCode: char*
public delegate uint TS3Functions_requestMuteClients(ulong serverConnectionHandlerID,[Out] ushort[] clientIDArray, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientIDArray: anyID*
///returnCode: char*
public delegate uint TS3Functions_requestUnmuteClients(ulong serverConnectionHandlerID, [Out] ushort[] clientIDArray, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///message: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientPoke(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string message, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientUniqueIdentifier: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientIDs(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string clientUniqueIdentifier, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientUniqueIdentifier: char*
///clientID: anyID->unsigned short
///returnCode: char*
public delegate uint TS3Functions_clientChatClosed(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string clientUniqueIdentifier, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///returnCode: char*
public delegate uint TS3Functions_clientChatComposing(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///password: char*
///description: char*
///duration: uint64->unsigned __int64
///targetChannelID: uint64->unsigned __int64
///targetChannelPW: char*
///returnCode: char*
public delegate uint TS3Functions_requestServerTemporaryPasswordAdd(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string password, [In] [MarshalAs(UnmanagedType.LPStr)] string description, ulong duration, ulong targetChannelID, [In] [MarshalAs(UnmanagedType.LPStr)] string targetChannelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///password: char*
///returnCode: char*
public delegate uint TS3Functions_requestServerTemporaryPasswordDel(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string password, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerTemporaryPasswordList(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: anyID*
public delegate uint TS3Functions_getClientID(ulong serverConnectionHandlerID, ref ushort result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///result: int
public delegate uint TS3Functions_getClientSelfVariableAsInt(ulong serverConnectionHandlerID, ClientProperty flag, [Out] out int result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///result: char**
public delegate uint TS3Functions_getClientSelfVariableAsString(ulong serverConnectionHandlerID, ClientProperty flag, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///value: int
public delegate uint TS3Functions_setClientSelfVariableAsInt(ulong serverConnectionHandlerID, ClientProperty flag, int value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///value: char*
public delegate uint TS3Functions_setClientSelfVariableAsString(ulong serverConnectionHandlerID, ClientProperty flag, [In] [MarshalAs(UnmanagedType.LPStr)] string value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_flushClientSelfUpdates(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: int
public delegate uint TS3Functions_getClientVariableAsInt(ulong serverConnectionHandlerID, ushort clientID, ClientProperty flag, out int result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: uint64*
public delegate uint TS3Functions_getClientVariableAsUInt64(ulong serverConnectionHandlerID, ushort clientID, ClientProperty flag, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: char**
public delegate uint TS3Functions_getClientVariableAsString(ulong serverConnectionHandlerID, ushort clientID, ClientProperty flag, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: anyID**
public delegate uint TS3Functions_getClientList(ulong serverConnectionHandlerID, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///result: uint64*
public delegate uint TS3Functions_getChannelOfClient(ulong serverConnectionHandlerID, ushort clientID,out ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///result: int
public delegate uint TS3Functions_getChannelVariableAsInt(ulong serverConnectionHandlerID, ulong channelID, ChannelProperty flag, ref int result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///result: uint64*
public delegate uint TS3Functions_getChannelVariableAsUInt64(ulong serverConnectionHandlerID, ulong channelID, ChannelProperty flag, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///result: char**
public delegate uint TS3Functions_getChannelVariableAsString(ulong serverConnectionHandlerID, ulong channelID, ChannelProperty flag, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelNameArray: char**
///result: uint64*
public delegate uint TS3Functions_getChannelIDFromChannelNames(ulong serverConnectionHandlerID, ref IntPtr channelNameArray, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///value: int
public delegate uint TS3Functions_setChannelVariableAsInt(ulong serverConnectionHandlerID, ulong channelID, IntPtr flag, int value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///value: uint64->unsigned __int64
public delegate uint TS3Functions_setChannelVariableAsUInt64(ulong serverConnectionHandlerID, ulong channelID, IntPtr flag, ulong value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///flag: size_t->uint
///value: char*
public delegate uint TS3Functions_setChannelVariableAsString(ulong serverConnectionHandlerID, ulong channelID, IntPtr flag, [In] [MarshalAs(UnmanagedType.LPStr)] string value);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_flushChannelUpdates(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelParentID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_flushChannelCreation(ulong serverConnectionHandlerID, ulong channelParentID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: uint64**
public delegate uint TS3Functions_getChannelList(ulong serverConnectionHandlerID, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///result: anyID**
public delegate uint TS3Functions_getChannelClientList(ulong serverConnectionHandlerID, ulong channelID, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///result: uint64*
public delegate uint TS3Functions_getParentChannelOfChannel(ulong serverConnectionHandlerID, ulong channelID, ref ulong result);

/// Return Type: uint
///result: uint64**
public delegate uint TS3Functions_getServerConnectionHandlerList(ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///result: int
public delegate uint TS3Functions_getServerVariableAsInt(ulong serverConnectionHandlerID, VirtualServerProperties flag, ref int result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///result: uint64*
public delegate uint TS3Functions_getServerVariableAsUInt64(ulong serverConnectionHandlerID, VirtualServerProperties flag, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///flag: size_t->uint
///result: char**
public delegate uint TS3Functions_getServerVariableAsString(ulong serverConnectionHandlerID, VirtualServerProperties flag, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
public delegate uint TS3Functions_requestServerVariables(ulong serverConnectionHandlerID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///result: int
public delegate uint TS3Functions_getConnectionStatus(ulong serverConnectionHandlerID, ref int result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: uint64*
public delegate uint TS3Functions_getConnectionVariableAsUInt64(ulong serverConnectionHandlerID, ushort clientID, IntPtr flag, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: double*
public delegate uint TS3Functions_getConnectionVariableAsDouble(ulong serverConnectionHandlerID, ushort clientID, IntPtr flag, ref double result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///flag: size_t->uint
///result: char**
public delegate uint TS3Functions_getConnectionVariableAsString(ulong serverConnectionHandlerID, ushort clientID, IntPtr flag, ref IntPtr result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
public delegate uint TS3Functions_cleanUpConnectionInfo(ulong serverConnectionHandlerID, ushort clientID);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientUniqueIdentifier: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientDBIDfromUID(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string clientUniqueIdentifier, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientUniqueIdentifier: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientNamefromUID(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string clientUniqueIdentifier, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestClientNamefromDBID(ulong serverConnectionHandlerID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///clientDescription: char*
///returnCode: char*
public delegate uint TS3Functions_requestClientEditDescription(ulong serverConnectionHandlerID, ushort clientID, [In] [MarshalAs(UnmanagedType.LPStr)] string clientDescription, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///isTalker: int
///returnCode: char*
public delegate uint TS3Functions_requestClientSetIsTalker(ulong serverConnectionHandlerID, ushort clientID, int isTalker, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///isTalkerRequest: int
///isTalkerRequestMessage: char*
///returnCode: char*
public delegate uint TS3Functions_requestIsTalker(ulong serverConnectionHandlerID, int isTalkerRequest, [In] [MarshalAs(UnmanagedType.LPStr)] string isTalkerRequestMessage, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///command: char*
///returnCode: char*
public delegate uint TS3Functions_requestSendClientQueryCommand(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string command, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: char**
public delegate uint TS3Functions_getTransferFileName(ushort transferID, ref IntPtr result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: char**
public delegate uint TS3Functions_getTransferFilePath(ushort transferID, ref IntPtr result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: uint64*
public delegate uint TS3Functions_getTransferFileSize(ushort transferID, ref ulong result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: uint64*
public delegate uint TS3Functions_getTransferFileSizeDone(ushort transferID, ref ulong result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: int
public delegate uint TS3Functions_isTransferSender(ushort transferID, ref int result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: int
public delegate uint TS3Functions_getTransferStatus(ushort transferID, ref int result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: float
public delegate uint TS3Functions_getCurrentTransferSpeed(ushort transferID, ref float result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: float
public delegate uint TS3Functions_getAverageTransferSpeed(ushort transferID, ref float result);

/// Return Type: uint
///transferID: anyID->unsigned short
///result: uint64*
public delegate uint TS3Functions_getTransferRunTime(ushort transferID, ref ulong result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///file: char*
///overwrite: int
///resume: int
///sourceDirectory: char*
///result: anyID*
///returnCode: char*
public delegate uint TS3Functions_sendFile(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string file, int overwrite, int resume, [In] [MarshalAs(UnmanagedType.LPStr)] string sourceDirectory, ref ushort result, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///file: char*
///overwrite: int
///resume: int
///destinationDirectory: char*
///result: anyID*
///returnCode: char*
public delegate uint TS3Functions_requestFile(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string file, int overwrite, int resume, [In] [MarshalAs(UnmanagedType.LPStr)] string destinationDirectory, ref ushort result, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///transferID: anyID->unsigned short
///deleteUnfinishedFile: int
///returnCode: char*
public delegate uint TS3Functions_haltTransfer(ulong serverConnectionHandlerID, ushort transferID, int deleteUnfinishedFile, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///path: char*
///returnCode: char*
public delegate uint TS3Functions_requestFileList(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string path, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///file: char*
///returnCode: char*
public delegate uint TS3Functions_requestFileInfo(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string file, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///file: char**
///returnCode: char*
public delegate uint TS3Functions_requestDeleteFile(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, ref IntPtr file, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPW: char*
///directoryPath: char*
///returnCode: char*
public delegate uint TS3Functions_requestCreateDirectory(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string directoryPath, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///fromChannelID: uint64->unsigned __int64
///channelPW: char*
///toChannelID: uint64->unsigned __int64
///toChannelPW: char*
///oldFile: char*
///newFile: char*
///returnCode: char*
public delegate uint TS3Functions_requestRenameFile(ulong serverConnectionHandlerID, ulong fromChannelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPW, ulong toChannelID, [In] [MarshalAs(UnmanagedType.LPStr)] string toChannelPW, [In] [MarshalAs(UnmanagedType.LPStr)] string oldFile, [In] [MarshalAs(UnmanagedType.LPStr)] string newFile, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///toClientUID: char*
///subject: char*
///message: char*
///returnCode: char*
public delegate uint TS3Functions_requestMessageAdd(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string toClientUID, [In] [MarshalAs(UnmanagedType.LPStr)] string subject, [In] [MarshalAs(UnmanagedType.LPStr)] string message, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///messageID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestMessageDel(ulong serverConnectionHandlerID, ulong messageID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///messageID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestMessageGet(ulong serverConnectionHandlerID, ulong messageID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestMessageList(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///messageID: uint64->unsigned __int64
///flag: int
///returnCode: char*
public delegate uint TS3Functions_requestMessageUpdateFlag(ulong serverConnectionHandlerID, ulong messageID, int flag, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverPassword: char*
///returnCode: char*
public delegate uint TS3Functions_verifyServerPassword(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string serverPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///channelPassword: char*
///returnCode: char*
public delegate uint TS3Functions_verifyChannelPassword(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///timeInSeconds: uint64->unsigned __int64
///banReason: char*
///returnCode: char*
public delegate uint TS3Functions_banclient(ulong serverConnectionHandlerID, ushort clientID, ulong timeInSeconds, [In] [MarshalAs(UnmanagedType.LPStr)] string banReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///ipRegExp: char*
///nameRegexp: char*
///uniqueIdentity: char*
///timeInSeconds: uint64->unsigned __int64
///banReason: char*
///returnCode: char*
public delegate uint TS3Functions_banadd(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string ipRegExp, [In] [MarshalAs(UnmanagedType.LPStr)] string nameRegexp, [In] [MarshalAs(UnmanagedType.LPStr)] string uniqueIdentity, [In] [MarshalAs(UnmanagedType.LPStr)] string mytsID, ulong timeInSeconds, [In] [MarshalAs(UnmanagedType.LPStr)] string banReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);


/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDBID: uint64->unsigned __int64
///timeInSeconds: uint64->unsigned __int64
///banReason: char*
///returnCode: char*
public delegate uint TS3Functions_banclientdbid(ulong serverConnectionHandlerID, ulong clientDBID, ulong timeInSeconds, [In] [MarshalAs(UnmanagedType.LPStr)] string banReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///banID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_bandel(ulong serverConnectionHandlerID, ulong banID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_bandelall(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestBanList(ulong serverConnectionHandlerID, ulong start, uint duration, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///targetClientDatabaseID: uint64->unsigned __int64
///complainReason: char*
///returnCode: char*
public delegate uint TS3Functions_requestComplainAdd(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string complainReason, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///targetClientDatabaseID: uint64->unsigned __int64
///fromClientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestComplainDel(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, ulong fromClientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///targetClientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestComplainDelAll(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///targetClientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestComplainList(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupList(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///groupName: char*
///groupType: int
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupAdd(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string groupName, int groupType, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///force: int
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupDel(ulong serverConnectionHandlerID, ulong serverGroupID, int force, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupAddClient(ulong serverConnectionHandlerID, ulong serverGroupID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupDelClient(ulong serverConnectionHandlerID, ulong serverGroupID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupsByClientID(ulong serverConnectionHandlerID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///continueonerror: int
///permissionIDArray: int
///permissionValueArray: int
///permissionNegatedArray: int
///permissionSkipArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupAddPerm(ulong serverConnectionHandlerID, ulong serverGroupID, int continueonerror, ref uint permissionIDArray, ref int permissionValueArray, ref int permissionNegatedArray, ref int permissionSkipArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///continueOnError: int
///permissionIDArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupDelPerm(ulong serverConnectionHandlerID, ulong serverGroupID, int continueOnError, ref uint permissionIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupPermList(ulong serverConnectionHandlerID, ulong serverGroupID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///serverGroupID: uint64->unsigned __int64
///withNames: int
///returnCode: char*
public delegate uint TS3Functions_requestServerGroupClientList(ulong serverConnectionHandlerID, ulong serverGroupID, int withNames, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupList(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///groupName: char*
///groupType: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupAdd(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string groupName, int groupType, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelGroupID: uint64->unsigned __int64
///force: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupDel(ulong serverConnectionHandlerID, ulong channelGroupID, int force, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelGroupID: uint64->unsigned __int64
///continueonerror: int
///permissionIDArray: int
///permissionValueArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupAddPerm(ulong serverConnectionHandlerID, ulong channelGroupID, int continueonerror, ref uint permissionIDArray, ref int permissionValueArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelGroupID: uint64->unsigned __int64
///continueOnError: int
///permissionIDArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupDelPerm(ulong serverConnectionHandlerID, ulong channelGroupID, int continueOnError, ref uint permissionIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelGroupID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelGroupPermList(ulong serverConnectionHandlerID, ulong channelGroupID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelGroupIDArray: uint64*
///channelIDArray: uint64*
///clientDatabaseIDArray: uint64*
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestSetClientChannelGroup(ulong serverConnectionHandlerID, ref ulong channelGroupIDArray, ref ulong channelIDArray, ref ulong clientDatabaseIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///permissionIDArray: int
///permissionValueArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelAddPerm(ulong serverConnectionHandlerID, ulong channelID, ref uint permissionIDArray, ref int permissionValueArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///permissionIDArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelDelPerm(ulong serverConnectionHandlerID, ulong channelID, ref uint permissionIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelPermList(ulong serverConnectionHandlerID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///permissionIDArray: int
///permissionValueArray: int
///permissionSkipArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestClientAddPerm(ulong serverConnectionHandlerID, ulong clientDatabaseID, ref uint permissionIDArray, ref int permissionValueArray, ref int permissionSkipArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///permissionIDArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestClientDelPerm(ulong serverConnectionHandlerID, ulong clientDatabaseID, ref uint permissionIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestClientPermList(ulong serverConnectionHandlerID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///permissionIDArray: int
///permissionValueArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelClientAddPerm(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, ref uint permissionIDArray, ref int permissionValueArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///permissionIDArray: int
///arraySize: int
///returnCode: char*
public delegate uint TS3Functions_requestChannelClientDelPerm(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, ref uint permissionIDArray, int arraySize, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///clientDatabaseID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestChannelClientPermList(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandler: uint64->unsigned __int64
///tokenKey: char*
///returnCode: char*
public delegate uint TS3Functions_privilegeKeyUse(ulong serverConnectionHandler, [In] [MarshalAs(UnmanagedType.LPStr)] string tokenKey, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandler: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestPermissionList(ulong serverConnectionHandler, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///serverConnectionHandler: uint64->unsigned __int64
///clientDBID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///returnCode: char*
public delegate uint TS3Functions_requestPermissionOverview(ulong serverConnectionHandler, ulong clientDBID, ulong channelID, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: uint
///clientPropertyString: char*
///resultFlag: size_t*
public delegate uint TS3Functions_clientPropertyStringToFlag([In] [MarshalAs(UnmanagedType.LPStr)] string clientPropertyString, ref uint resultFlag);

/// Return Type: uint
///channelPropertyString: char*
///resultFlag: size_t*
public delegate uint TS3Functions_channelPropertyStringToFlag([In] [MarshalAs(UnmanagedType.LPStr)] string channelPropertyString, ref uint resultFlag);

/// Return Type: uint
///serverPropertyString: char*
///resultFlag: size_t*
public delegate uint TS3Functions_serverPropertyStringToFlag([In] [MarshalAs(UnmanagedType.LPStr)] string serverPropertyString, ref uint resultFlag);

/// Return Type: void
///path: char*
///maxLen: size_t->uint
public delegate void TS3Functions_getAppPath(IntPtr path, IntPtr maxLen);

/// Return Type: void
///path: char*
///maxLen: size_t->uint
public delegate void TS3Functions_getResourcesPath(IntPtr path, IntPtr maxLen);

/// Return Type: void
///path: char*
///maxLen: size_t->uint
public delegate void TS3Functions_getConfigPath(IntPtr path, IntPtr maxLen);

/// Return Type: void
///path: char*
///maxLen: size_t->uint
public delegate void TS3Functions_getPluginPath(IntPtr path, IntPtr maxLen);

/// Return Type: uint64->unsigned __int64
public delegate ulong TS3Functions_getCurrentServerConnectionHandlerID();

/// Return Type: void
///serverConnectionHandlerID: uint64->unsigned __int64
///message: char*
///messageTarget: PluginMessageTarget
public delegate void TS3Functions_printMessage(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string message, MessageTarget messageTarget);

/// Return Type: void
///message: char*
public delegate void TS3Functions_printMessageToCurrentTab([In] [MarshalAs(UnmanagedType.LPStr)] string message);

/// Return Type: void
///text: char*
///result: char*
///maxLen: size_t->uint
public delegate void TS3Functions_urlsToBB([In] [MarshalAs(UnmanagedType.LPStr)] string text, IntPtr result, IntPtr maxLen);

/// Return Type: void
///serverConnectionHandlerID: uint64->unsigned __int64
///pluginID: char*
///command: char*
///targetMode: int
///targetIDs: anyID*
///returnCode: char*
public delegate void TS3Functions_sendPluginCommand(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, [In] [MarshalAs(UnmanagedType.LPStr)] string command, int targetMode, ref ushort targetIDs, [In] [MarshalAs(UnmanagedType.LPStr)] string returnCode);

/// Return Type: void
///path: char*
///result: char*
///maxLen: size_t->uint
public delegate void TS3Functions_getDirectories([In] [MarshalAs(UnmanagedType.LPStr)] string path, IntPtr result, IntPtr maxLen);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///host: char*
///port: unsigned short
///password: char*
///maxLen: size_t->uint
public delegate uint TS3Functions_getServerConnectInfo(ulong scHandlerID, IntPtr host, ref ushort port, IntPtr password, IntPtr maxLen);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///channelID: uint64->unsigned __int64
///path: char*
///password: char*
///maxLen: size_t->uint
public delegate uint TS3Functions_getChannelConnectInfo(ulong scHandlerID, ulong channelID, IntPtr path, IntPtr password, IntPtr maxLen);

/// Return Type: void
///pluginID: char*
///returnCode: char*
///maxLen: size_t->uint
// public delegate void TS3Functions_createReturnCode([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, IntPtr returnCode, IntPtr maxLen);
public delegate void TS3Functions_createReturnCode([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, [Out] StringBuilder returnCode, ulong maxLen);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///itemType: PluginItemType
///itemID: uint64->unsigned __int64
public delegate uint TS3Functions_requestInfoUpdate(ulong scHandlerID, PluginType itemType, ulong itemID);

/// Return Type: uint64->unsigned __int64
///scHandlerID: uint64->unsigned __int64
public delegate ulong TS3Functions_getServerVersion(ulong scHandlerID);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///result: int
public delegate uint TS3Functions_isWhispering(ulong scHandlerID, ushort clientID, ref int result);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///result: int
public delegate uint TS3Functions_isReceivingWhisper(ulong scHandlerID, ushort clientID, ref int result);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///result: char*
///maxLen: size_t->uint
public delegate uint TS3Functions_getAvatar(ulong scHandlerID, ushort clientID, IntPtr result, IntPtr maxLen);

/// Return Type: void
///pluginID: char*
///menuID: int
///enabled: int
public delegate void TS3Functions_setPluginMenuEnabled([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, int menuID, int enabled);

/// Return Type: void
public delegate void TS3Functions_showHotkeySetup();

/// Return Type: void
///pluginID: char*
///keyword: char*
///qParentWindow: void*
public delegate void TS3Functions_requestHotkeyInputDialog([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, [In] [MarshalAs(UnmanagedType.LPStr)] string keyword, IntPtr qParentWindow);

/// Return Type: uint
///pluginID: char*
///keywords: char**
///hotkeys: char**
///arrayLen: size_t->uint
///hotkeyBufSize: size_t->uint
public delegate uint TS3Functions_getHotkeyFromKeyword([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, ref IntPtr keywords, ref IntPtr hotkeys, IntPtr arrayLen, IntPtr hotkeyBufSize);

/// Return Type: uint
///scHandlerID: uint64->unsigned __int64
///clientID: anyID->unsigned short
///result: char*
///maxLen: size_t->uint
public delegate uint TS3Functions_getClientDisplayName(ulong scHandlerID, ushort clientID, IntPtr result, IntPtr maxLen);

/// Return Type: uint
///list: PluginBookmarkList**
public delegate uint TS3Functions_getBookmarkList(ref IntPtr list);

/// Return Type: uint
///profile: PluginGuiProfile
///defaultProfileIdx: int
///result: char***
public delegate uint TS3Functions_getProfileList(PluginProfile profile, ref int defaultProfileIdx, ref IntPtr result);

/// Return Type: uint
///connectTab: PluginConnectTab
///serverLabel: char*
///serverAddress: char*
///serverPassword: char*
///nickname: char*
///channel: char*
///channelPassword: char*
///captureProfile: char*
///playbackProfile: char*
///hotkeyProfile: char*
///soundProfile: char*
///userIdentity: char*
///oneTimeKey: char*
///phoneticName: char*
///scHandlerID: uint64*
public delegate uint TS3Functions_guiConnect(ConnectionTarget connectTab, [In] [MarshalAs(UnmanagedType.LPStr)] string serverLabel, [In] [MarshalAs(UnmanagedType.LPStr)] string serverAddress, [In] [MarshalAs(UnmanagedType.LPStr)] string serverPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string nickname, [In] [MarshalAs(UnmanagedType.LPStr)] string channel, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string captureProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string playbackProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string hotkeyProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string soundProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string userIdentity, [In] [MarshalAs(UnmanagedType.LPStr)] string oneTimeKey, [In] [MarshalAs(UnmanagedType.LPStr)] string phoneticName, ref ulong scHandlerID);

/// Return Type: uint
///connectTab: PluginConnectTab
///bookmarkuuid: char*
///scHandlerID: uint64*
public delegate uint TS3Functions_guiConnectBookmark(ConnectionTarget connectTab, [In] [MarshalAs(UnmanagedType.LPStr)] string bookmarkuuid, ref ulong scHandlerID);

/// Return Type: uint
///bookmarkuuid: char*
///serverLabel: char*
///serverAddress: char*
///serverPassword: char*
///nickname: char*
///channel: char*
///channelPassword: char*
///captureProfile: char*
///playbackProfile: char*
///hotkeyProfile: char*
///soundProfile: char*
///uniqueUserId: char*
///oneTimeKey: char*
///phoneticName: char*
public delegate uint TS3Functions_createBookmark([In] [MarshalAs(UnmanagedType.LPStr)] string bookmarkuuid, [In] [MarshalAs(UnmanagedType.LPStr)] string serverLabel, [In] [MarshalAs(UnmanagedType.LPStr)] string serverAddress, [In] [MarshalAs(UnmanagedType.LPStr)] string serverPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string nickname, [In] [MarshalAs(UnmanagedType.LPStr)] string channel, [In] [MarshalAs(UnmanagedType.LPStr)] string channelPassword, [In] [MarshalAs(UnmanagedType.LPStr)] string captureProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string playbackProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string hotkeyProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string soundProfile, [In] [MarshalAs(UnmanagedType.LPStr)] string uniqueUserId, [In] [MarshalAs(UnmanagedType.LPStr)] string oneTimeKey, [In] [MarshalAs(UnmanagedType.LPStr)] string phoneticName);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///permissionName: char*
///result: uint
public delegate uint TS3Functions_getPermissionIDByName(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string permissionName, ref uint result);

/// Return Type: uint
///serverConnectionHandlerID: uint64->unsigned __int64
///permissionName: char*
///result: int
public delegate uint TS3Functions_getClientNeededPermission(ulong serverConnectionHandlerID, [In] [MarshalAs(UnmanagedType.LPStr)] string permissionName, ref int result);
public delegate void TS3Functions_notifyKeyEvent([In] [MarshalAs(UnmanagedType.LPStr)] string pluginID, [In] [MarshalAs(UnmanagedType.LPStr)] string keyId, int up_down);
#endregion [ DELEGATED METHODS ]

[StructLayoutAttribute(LayoutKind.Sequential)]
public struct TS3Functions
{

    /// <summary>
    /// Gets client library version.
    /// </summary>
    public TS3Functions_getClientLibVersion getClientLibVersion;

    /// <summary>
    /// Get client library version number.
    /// </summary>
    public TS3Functions_getClientLibVersionNumber getClientLibVersionNumber;

    /// <summary>
    /// Generates new server connection handler.
    /// </summary>
    public TS3Functions_spawnNewServerConnectionHandler spawnNewServerConnectionHandler;

    /// <summary>
    /// Destroys the server connection handler.
    /// </summary>
    public TS3Functions_destroyServerConnectionHandler destroyServerConnectionHandler;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public TS3Functions_getErrorMessage getErrorMessage;

    /// <summary>
    /// Free up memory.
    /// </summary>
    public TS3Functions_freeMemory freeMemory;

    /// <summary>
    /// Logs message.
    /// </summary>
    public TS3Functions_logMessage logMessage;

    /// <summary>
    /// Gets the playback devices list.
    /// </summary>
    public TS3Functions_getPlaybackDeviceList getPlaybackDeviceList;

    /// <summary>
    /// Gets the playback modes list.
    /// </summary>
    public TS3Functions_getPlaybackModeList getPlaybackModeList;

    /// <summary>
    /// Gets the capture devices list.
    /// </summary>
    public TS3Functions_getCaptureDeviceList getCaptureDeviceList;

    /// <summary>
    /// Gets the capture modes list.
    /// </summary>
    public TS3Functions_getCaptureModeList getCaptureModeList;

    /// <summary>
    /// Get default playback device.
    /// </summary>
    public TS3Functions_getDefaultPlaybackDevice getDefaultPlaybackDevice;

    /// <summary>
    /// Gets the default playback device.
    /// </summary>
    public TS3Functions_getDefaultPlayBackMode getDefaultPlayBackMode;

    /// <summary>
    /// Gets the default capture device.
    /// </summary>
    public TS3Functions_getDefaultCaptureDevice getDefaultCaptureDevice;

    /// <summary>
    /// Gets the current capture mode.
    /// </summary>
    public TS3Functions_getDefaultCaptureMode getDefaultCaptureMode;

    /// <summary>
    /// Opens the playback device.
    /// </summary>
    public TS3Functions_openPlaybackDevice openPlaybackDevice;

    /// <summary>
    /// Opens the capture device.
    /// </summary>
    public TS3Functions_openCaptureDevice openCaptureDevice;

    /// <summary>
    /// Gets the current playback device name.
    /// </summary>
    public TS3Functions_getCurrentPlaybackDeviceName getCurrentPlaybackDeviceName;

    /// <summary>
    /// Gets the current playback mode.
    /// </summary>
    public TS3Functions_getCurrentPlayBackMode getCurrentPlayBackMode;

    /// <summary>
    /// Gets the current capture device name.
    /// </summary>
    public TS3Functions_getCurrentCaptureDeviceName getCurrentCaptureDeviceName;

    /// <summary>
    /// Gets the current capture mode.
    /// </summary>
    public TS3Functions_getCurrentCaptureMode getCurrentCaptureMode;

    /// <summary>
    /// Initializes graceful playback shutdown.
    /// </summary>
    public TS3Functions_initiateGracefulPlaybackShutdown initiateGracefulPlaybackShutdown;

    /// <summary>
    /// Closes the playback device.
    /// </summary>
    public TS3Functions_closePlaybackDevice closePlaybackDevice;

    /// <summary>
    /// Closes the capture device.
    /// </summary>
    public TS3Functions_closeCaptureDevice closeCaptureDevice;

    /// <summary>
    /// Activates the capture device.
    /// </summary>
    public TS3Functions_activateCaptureDevice activateCaptureDevice;

    /// <summary>
    /// Plays a wave file.
    /// </summary>
    public TS3Functions_playWaveFileHandle playWaveFileHandle;

    /// <summary>
    /// Pauses wave file handling.
    /// </summary>
    public TS3Functions_pauseWaveFileHandle pauseWaveFileHandle;

    /// <summary>
    /// Closes wave file.
    /// </summary>
    public TS3Functions_closeWaveFileHandle closeWaveFileHandle;

    /// <summary>
    /// Plays wave file.
    /// </summary>
    public TS3Functions_playWaveFile playWaveFile;

    /// <summary>
    /// Registers custeom device.
    /// </summary>
    public TS3Functions_registerCustomDevice registerCustomDevice;

    /// <summary>
    /// Unregister a custom device.
    /// </summary>
    public TS3Functions_unregisterCustomDevice unregisterCustomDevice;

    /// <summary>
    /// Processes custom capture data.
    /// </summary>
    public TS3Functions_processCustomCaptureData processCustomCaptureData;

    /// <summary>
    /// Acquiring custom playback data.
    /// </summary>
    public TS3Functions_acquireCustomPlaybackData acquireCustomPlaybackData;

    /// <summary>
    /// Gets the preprocessor info value as float.
    /// </summary>
    public TS3Functions_getPreProcessorInfoValueFloat getPreProcessorInfoValueFloat;

    /// <summary>
    /// Get preprocessor configuration value.
    /// </summary>
    public TS3Functions_getPreProcessorConfigValue getPreProcessorConfigValue;

    /// <summary>
    /// Set preprocessor configuration value.
    /// </summary>
    public TS3Functions_setPreProcessorConfigValue setPreProcessorConfigValue;

    /// <summary>
    /// Get encoding configuration value as float.
    /// </summary>
    public TS3Functions_getEncodeConfigValue getEncodeConfigValue;

    /// <summary>
    /// Gets playback configuration value as float.
    /// </summary>
    public TS3Functions_getPlaybackConfigValueAsFloat getPlaybackConfigValueAsFloat;

    /// <summary>
    /// Sets playback configuration value.
    /// </summary>
    public TS3Functions_setPlaybackConfigValue setPlaybackConfigValue;

    /// <summary>
    /// Sets client volume modifier.
    /// </summary>
    public TS3Functions_setClientVolumeModifier setClientVolumeModifier;

    /// <summary>
    /// Starts voice recording.
    /// </summary>
    public TS3Functions_startVoiceRecording startVoiceRecording;

    /// <summary>
    /// Stops voice recording.
    /// </summary>
    public TS3Functions_stopVoiceRecording stopVoiceRecording;

    /// <summary>
    /// Set listeners 3D attributes.
    /// </summary>
    public TS3Functions_systemset3DListenerAttributes systemset3DListenerAttributes;

    /// <summary>
    /// Set wave 3D attributes.
    /// </summary>
    public TS3Functions_set3DWaveAttributes set3DWaveAttributes;

    /// <summary>
    /// Set system 3D settings.
    /// </summary>
    public TS3Functions_systemset3DSettings systemset3DSettings;

    /// <summary>
    /// Set +d attributes for channel.
    /// </summary>
    public TS3Functions_channelset3DAttributes channelset3DAttributes;

    /// <summary>
    /// Starts connection.
    /// </summary>
    public TS3Functions_startConnection startConnection;

    /// <summary>
    /// Stops the connection.
    /// </summary>
    public TS3Functions_stopConnection stopConnection;

    /// <summary>
    /// Request client moving.
    /// </summary>
    public TS3Functions_requestClientMove requestClientMove;

    /// <summary>
    /// Request for client variables.
    /// </summary>
    public TS3Functions_requestClientVariables requestClientVariables;

    /// <summary>
    /// Request client kicking from channel.
    /// </summary>
    public TS3Functions_requestClientKickFromChannel requestClientKickFromChannel;

    /// <summary>
    /// Request a client kicking from server.
    /// </summary>
    public TS3Functions_requestClientKickFromServer requestClientKickFromServer;

    /// <summary>
    /// Requests for channel deleting.
    /// </summary>
    public TS3Functions_requestChannelDelete requestChannelDelete;

    /// <summary>
    /// Requset a channel moving.
    /// </summary>
    public TS3Functions_requestChannelMove requestChannelMove;

    /// <summary>
    /// Request to send private message.
    /// </summary>
    public TS3Functions_requestSendPrivateTextMsg requestSendPrivateTextMsg;

    /// <summary>
    /// Request to send message in channel chat.
    /// </summary>
    public TS3Functions_requestSendChannelTextMsg requestSendChannelTextMsg;

    /// <summary>
    /// Request to send message in global server chat.
    /// </summary>
    public TS3Functions_requestSendServerTextMsg requestSendServerTextMsg;

    /// <summary>
    /// Request the connection info.
    /// </summary>
    public TS3Functions_requestConnectionInfo requestConnectionInfo;

    /// <summary>
    /// Request setting whispers list.
    /// </summary>
    public TS3Functions_requestClientSetWhisperList requestClientSetWhisperList;

    /// <summary>
    /// Request channel subscribing.
    /// </summary>
    public TS3Functions_requestChannelSubscribe requestChannelSubscribe;

    /// <summary>
    /// Request all channels subscribing.
    /// </summary>
    public TS3Functions_requestChannelSubscribeAll requestChannelSubscribeAll;

    /// <summary>
    /// Request channel unsubscribing.
    /// </summary>
    public TS3Functions_requestChannelUnsubscribe requestChannelUnsubscribe;

    /// <summary>
    /// Request all channel unsubscribes.
    /// </summary>
    public TS3Functions_requestChannelUnsubscribeAll requestChannelUnsubscribeAll;

    /// <summary>
    /// Request channel description.
    /// </summary>
    public TS3Functions_requestChannelDescription requestChannelDescription;

    /// <summary>
    /// Request mute clients.
    /// </summary>
    public TS3Functions_requestMuteClients requestMuteClients;

    /// <summary>
    /// Request unmute clients.
    /// </summary>
    public TS3Functions_requestUnmuteClients requestUnmuteClients;

    /// <summary>
    /// Request a poke.
    /// </summary>
    public TS3Functions_requestClientPoke requestClientPoke;

    /// <summary>
    /// Request client IDs.
    /// </summary>
    public TS3Functions_requestClientIDs requestClientIDs;

    /// <summary>
    /// When client closes the chat.
    /// </summary>
    public TS3Functions_clientChatClosed clientChatClosed;

    /// <summary>
    /// When client chat composed.
    /// </summary>
    public TS3Functions_clientChatComposing clientChatComposing;

    /// <summary>
    /// Request for temporary password adding.
    /// </summary>
    public TS3Functions_requestServerTemporaryPasswordAdd requestServerTemporaryPasswordAdd;

    /// <summary>
    /// Request for temporary password deleting.
    /// </summary>
    public TS3Functions_requestServerTemporaryPasswordDel requestServerTemporaryPasswordDel;

    /// <summary>
    /// Request for temporary passwords list.
    /// </summary>
    public TS3Functions_requestServerTemporaryPasswordList requestServerTemporaryPasswordList;

    /// <summary>
    /// Gets the client ID.
    /// </summary>
    public TS3Functions_getClientID getClientID;

    /// <summary>
    /// Gets the client self variable as unsigned? integer.
    /// </summary>
    public TS3Functions_getClientSelfVariableAsInt getClientSelfVariableAsInt;

    /// TS3Functions_getClientSelfVariableAsString
    public TS3Functions_getClientSelfVariableAsString getClientSelfVariableAsString;

    /// TS3Functions_setClientSelfVariableAsInt
    public TS3Functions_setClientSelfVariableAsInt setClientSelfVariableAsInt;

    /// TS3Functions_setClientSelfVariableAsString
    public TS3Functions_setClientSelfVariableAsString setClientSelfVariableAsString;

    /// TS3Functions_flushClientSelfUpdates
    public TS3Functions_flushClientSelfUpdates flushClientSelfUpdates;

    /// TS3Functions_getClientVariableAsInt
    public TS3Functions_getClientVariableAsInt getClientVariableAsInt;

    /// TS3Functions_getClientVariableAsUInt64
    public TS3Functions_getClientVariableAsUInt64 getClientVariableAsUInt64;

    /// TS3Functions_getClientVariableAsString
    public TS3Functions_getClientVariableAsString getClientVariableAsString;

    /// TS3Functions_getClientList
    public TS3Functions_getClientList getClientList;

    /// TS3Functions_getChannelOfClient
    public TS3Functions_getChannelOfClient getChannelOfClient;

    /// TS3Functions_getChannelVariableAsInt
    public TS3Functions_getChannelVariableAsInt getChannelVariableAsInt;

    /// TS3Functions_getChannelVariableAsUInt64
    public TS3Functions_getChannelVariableAsUInt64 getChannelVariableAsUInt64;

    /// TS3Functions_getChannelVariableAsString
    public TS3Functions_getChannelVariableAsString getChannelVariableAsString;

    /// TS3Functions_getChannelIDFromChannelNames
    public TS3Functions_getChannelIDFromChannelNames getChannelIDFromChannelNames;

    /// TS3Functions_setChannelVariableAsInt
    public TS3Functions_setChannelVariableAsInt setChannelVariableAsInt;

    /// TS3Functions_setChannelVariableAsUInt64
    public TS3Functions_setChannelVariableAsUInt64 setChannelVariableAsUInt64;

    /// TS3Functions_setChannelVariableAsString
    public TS3Functions_setChannelVariableAsString setChannelVariableAsString;

    /// TS3Functions_flushChannelUpdates
    public TS3Functions_flushChannelUpdates flushChannelUpdates;

    /// TS3Functions_flushChannelCreation
    public TS3Functions_flushChannelCreation flushChannelCreation;

    /// TS3Functions_getChannelList
    public TS3Functions_getChannelList getChannelList;

    /// TS3Functions_getChannelClientList
    public TS3Functions_getChannelClientList getChannelClientList;

    /// TS3Functions_getParentChannelOfChannel
    public TS3Functions_getParentChannelOfChannel getParentChannelOfChannel;

    /// TS3Functions_getServerConnectionHandlerList
    public TS3Functions_getServerConnectionHandlerList getServerConnectionHandlerList;

    /// TS3Functions_getServerVariableAsInt
    public TS3Functions_getServerVariableAsInt getServerVariableAsInt;

    /// TS3Functions_getServerVariableAsUInt64
    public TS3Functions_getServerVariableAsUInt64 getServerVariableAsUInt64;

    /// TS3Functions_getServerVariableAsString
    public TS3Functions_getServerVariableAsString getServerVariableAsString;

    /// TS3Functions_requestServerVariables
    public TS3Functions_requestServerVariables requestServerVariables;

    /// TS3Functions_getConnectionStatus
    public TS3Functions_getConnectionStatus getConnectionStatus;

    /// TS3Functions_getConnectionVariableAsUInt64
    public TS3Functions_getConnectionVariableAsUInt64 getConnectionVariableAsUInt64;

    /// TS3Functions_getConnectionVariableAsDouble
    public TS3Functions_getConnectionVariableAsDouble getConnectionVariableAsDouble;

    /// TS3Functions_getConnectionVariableAsString
    public TS3Functions_getConnectionVariableAsString getConnectionVariableAsString;

    /// <summary>
    /// Clean up the connection info.
    /// </summary>
    public TS3Functions_cleanUpConnectionInfo cleanUpConnectionInfo;

    /// <summary>
    /// Request client database name from unique ID.
    /// </summary>
    public TS3Functions_requestClientDBIDfromUID requestClientDBIDfromUID;

    /// <summary>
    /// Request client name from unique ID.
    /// </summary>
    public TS3Functions_requestClientNamefromUID requestClientNamefromUID;

    /// <summary>
    /// Request client name from database ID.
    /// </summary>
    public TS3Functions_requestClientNamefromDBID requestClientNamefromDBID;

    /// <summary>
    /// Request to set client description editing.
    /// </summary>
    public TS3Functions_requestClientEditDescription requestClientEditDescription;

    /// <summary>
    /// Request to set is talker.
    /// </summary>
    public TS3Functions_requestClientSetIsTalker requestClientSetIsTalker;

    /// <summary>
    /// Request to get is talker.
    /// </summary>
    public TS3Functions_requestIsTalker requestIsTalker;

    /// <summary>
    /// Request to send client query command.
    /// </summary>
    public TS3Functions_requestSendClientQueryCommand requestSendClientQueryCommand;

    /// <summary>
    /// Get transfer file name.
    /// </summary>
    public TS3Functions_getTransferFileName getTransferFileName;

    /// TS3Functions_getTransferFilePath
    public TS3Functions_getTransferFilePath getTransferFilePath;

    /// TS3Functions_getTransferFileSize
    public TS3Functions_getTransferFileSize getTransferFileSize;

    /// TS3Functions_getTransferFileSizeDone
    public TS3Functions_getTransferFileSizeDone getTransferFileSizeDone;

    /// TS3Functions_isTransferSender
    public TS3Functions_isTransferSender isTransferSender;

    /// TS3Functions_getTransferStatus
    public TS3Functions_getTransferStatus getTransferStatus;

    /// TS3Functions_getCurrentTransferSpeed
    public TS3Functions_getCurrentTransferSpeed getCurrentTransferSpeed;

    /// TS3Functions_getAverageTransferSpeed
    public TS3Functions_getAverageTransferSpeed getAverageTransferSpeed;

    /// TS3Functions_getTransferRunTime
    public TS3Functions_getTransferRunTime getTransferRunTime;

    /// TS3Functions_sendFile
    public TS3Functions_sendFile sendFile;

    /// TS3Functions_requestFile
    public TS3Functions_requestFile requestFile;

    /// TS3Functions_haltTransfer
    public TS3Functions_haltTransfer haltTransfer;

    /// TS3Functions_requestFileList
    public TS3Functions_requestFileList requestFileList;

    /// TS3Functions_requestFileInfo
    public TS3Functions_requestFileInfo requestFileInfo;

    /// TS3Functions_requestDeleteFile
    public TS3Functions_requestDeleteFile requestDeleteFile;

    /// TS3Functions_requestCreateDirectory
    public TS3Functions_requestCreateDirectory requestCreateDirectory;

    /// TS3Functions_requestRenameFile
    public TS3Functions_requestRenameFile requestRenameFile;

    /// TS3Functions_requestMessageAdd
    public TS3Functions_requestMessageAdd requestMessageAdd;

    /// TS3Functions_requestMessageDel
    public TS3Functions_requestMessageDel requestMessageDel;

    /// TS3Functions_requestMessageGet
    public TS3Functions_requestMessageGet requestMessageGet;

    /// TS3Functions_requestMessageList
    public TS3Functions_requestMessageList requestMessageList;

    /// TS3Functions_requestMessageUpdateFlag
    public TS3Functions_requestMessageUpdateFlag requestMessageUpdateFlag;

    /// TS3Functions_verifyServerPassword
    public TS3Functions_verifyServerPassword verifyServerPassword;

    /// TS3Functions_verifyChannelPassword
    public TS3Functions_verifyChannelPassword verifyChannelPassword;

    /// TS3Functions_banclient
    public TS3Functions_banclient banclient;

    /// TS3Functions_banadd
    public TS3Functions_banadd banadd;

    /// TS3Functions_banclientdbid
    public TS3Functions_banclientdbid banclientdbid;

    /// TS3Functions_bandel
    public TS3Functions_bandel bandel;

    /// TS3Functions_bandelall
    public TS3Functions_bandelall bandelall;

    /// TS3Functions_requestBanList
    public TS3Functions_requestBanList requestBanList;

    /// TS3Functions_requestComplainAdd
    public TS3Functions_requestComplainAdd requestComplainAdd;

    /// TS3Functions_requestComplainDel
    public TS3Functions_requestComplainDel requestComplainDel;

    /// TS3Functions_requestComplainDelAll
    public TS3Functions_requestComplainDelAll requestComplainDelAll;

    /// TS3Functions_requestComplainList
    public TS3Functions_requestComplainList requestComplainList;

    /// TS3Functions_requestServerGroupList
    public TS3Functions_requestServerGroupList requestServerGroupList;

    /// TS3Functions_requestServerGroupAdd
    public TS3Functions_requestServerGroupAdd requestServerGroupAdd;

    /// TS3Functions_requestServerGroupDel
    public TS3Functions_requestServerGroupDel requestServerGroupDel;

    /// TS3Functions_requestServerGroupAddClient
    public TS3Functions_requestServerGroupAddClient requestServerGroupAddClient;

    /// TS3Functions_requestServerGroupDelClient
    public TS3Functions_requestServerGroupDelClient requestServerGroupDelClient;

    /// TS3Functions_requestServerGroupsByClientID
    public TS3Functions_requestServerGroupsByClientID requestServerGroupsByClientID;

    /// TS3Functions_requestServerGroupAddPerm
    public TS3Functions_requestServerGroupAddPerm requestServerGroupAddPerm;

    /// TS3Functions_requestServerGroupDelPerm
    public TS3Functions_requestServerGroupDelPerm requestServerGroupDelPerm;

    /// TS3Functions_requestServerGroupPermList
    public TS3Functions_requestServerGroupPermList requestServerGroupPermList;

    /// TS3Functions_requestServerGroupClientList
    public TS3Functions_requestServerGroupClientList requestServerGroupClientList;

    /// TS3Functions_requestChannelGroupList
    public TS3Functions_requestChannelGroupList requestChannelGroupList;

    /// TS3Functions_requestChannelGroupAdd
    public TS3Functions_requestChannelGroupAdd requestChannelGroupAdd;

    /// TS3Functions_requestChannelGroupDel
    public TS3Functions_requestChannelGroupDel requestChannelGroupDel;

    /// TS3Functions_requestChannelGroupAddPerm
    public TS3Functions_requestChannelGroupAddPerm requestChannelGroupAddPerm;

    /// TS3Functions_requestChannelGroupDelPerm
    public TS3Functions_requestChannelGroupDelPerm requestChannelGroupDelPerm;

    /// TS3Functions_requestChannelGroupPermList
    public TS3Functions_requestChannelGroupPermList requestChannelGroupPermList;

    /// TS3Functions_requestSetClientChannelGroup
    public TS3Functions_requestSetClientChannelGroup requestSetClientChannelGroup;

    /// TS3Functions_requestChannelAddPerm
    public TS3Functions_requestChannelAddPerm requestChannelAddPerm;

    /// TS3Functions_requestChannelDelPerm
    public TS3Functions_requestChannelDelPerm requestChannelDelPerm;

    /// TS3Functions_requestChannelPermList
    public TS3Functions_requestChannelPermList requestChannelPermList;

    /// TS3Functions_requestClientAddPerm
    public TS3Functions_requestClientAddPerm requestClientAddPerm;

    /// TS3Functions_requestClientDelPerm
    public TS3Functions_requestClientDelPerm requestClientDelPerm;

    /// TS3Functions_requestClientPermList
    public TS3Functions_requestClientPermList requestClientPermList;

    /// TS3Functions_requestChannelClientAddPerm
    public TS3Functions_requestChannelClientAddPerm requestChannelClientAddPerm;

    /// TS3Functions_requestChannelClientDelPerm
    public TS3Functions_requestChannelClientDelPerm requestChannelClientDelPerm;

    /// TS3Functions_requestChannelClientPermList
    public TS3Functions_requestChannelClientPermList requestChannelClientPermList;

    /// TS3Functions_privilegeKeyUse
    public TS3Functions_privilegeKeyUse privilegeKeyUse;

    /// TS3Functions_requestPermissionList
    public TS3Functions_requestPermissionList requestPermissionList;

    /// TS3Functions_requestPermissionOverview
    public TS3Functions_requestPermissionOverview requestPermissionOverview;

    /// TS3Functions_clientPropertyStringToFlag
    public TS3Functions_clientPropertyStringToFlag clientPropertyStringToFlag;

    /// TS3Functions_channelPropertyStringToFlag
    public TS3Functions_channelPropertyStringToFlag channelPropertyStringToFlag;

    /// TS3Functions_serverPropertyStringToFlag
    public TS3Functions_serverPropertyStringToFlag serverPropertyStringToFlag;

    /// TS3Functions_getAppPath
    public TS3Functions_getAppPath getAppPath;

    /// TS3Functions_getResourcesPath
    public TS3Functions_getResourcesPath getResourcesPath;

    /// TS3Functions_getConfigPath
    public TS3Functions_getConfigPath getConfigPath;

    /// <summary>
    /// Gets the plugin path.
    /// </summary>
    public TS3Functions_getPluginPath getPluginPath;

    /// <summary>
    /// Gets the current server connection handler ID.
    /// </summary>
    public TS3Functions_getCurrentServerConnectionHandlerID getCurrentServerConnectionHandlerID;

    /// <summary>
    /// Prints a message.
    /// </summary>
    public TS3Functions_printMessage printMessage;

    /// <summary>
    /// Prints a message to current tab.
    /// </summary>
    public TS3Functions_printMessageToCurrentTab printMessageToCurrentTab;

    /// <summary>
    /// Convert URL-s to BB.
    /// </summary>
    public TS3Functions_urlsToBB urlsToBB;

    /// <summary>
    /// Sends a plugin command.
    /// </summary>
    public TS3Functions_sendPluginCommand sendPluginCommand;

    /// <summary>
    /// Gets the directories.
    /// </summary>
    public TS3Functions_getDirectories getDirectories;

    /// <summary>
    /// Gets the connection info at current server.
    /// </summary>
    public TS3Functions_getServerConnectInfo getServerConnectInfo;

    /// <summary>
    /// Gets the connection info at current channel.
    /// </summary>
    public TS3Functions_getChannelConnectInfo getChannelConnectInfo;

    /// <summary>
    /// Creates a return code.
    /// </summary>
    public TS3Functions_createReturnCode createReturnCode;

    /// <summary>
    /// Request for info update.
    /// </summary>
    public TS3Functions_requestInfoUpdate requestInfoUpdate;

    /// <summary>
    /// Gets the server version.
    /// </summary>
    public TS3Functions_getServerVersion getServerVersion;

    /// <summary>
    /// Shows if whsipering.
    /// </summary>
    public TS3Functions_isWhispering isWhispering;

    /// <summary>
    /// Shows if recieving whisper.
    /// </summary>
    public TS3Functions_isReceivingWhisper isReceivingWhisper;

    /// <summary>
    /// Gets the avatar.
    /// </summary>
    public TS3Functions_getAvatar getAvatar;

    /// <summary>
    /// Sets the plugin menu to ENABLED.
    /// </summary>
    public TS3Functions_setPluginMenuEnabled setPluginMenuEnabled;

    /// <summary>
    /// Shows hotkey setup.
    /// </summary>
    public TS3Functions_showHotkeySetup showHotkeySetup;

    /// <summary>
    /// Request a hotkey input dialog.
    /// </summary>
    public TS3Functions_requestHotkeyInputDialog requestHotkeyInputDialog;

    /// <summary>
    /// Gets hotkey from its keyword.
    /// </summary>
    public TS3Functions_getHotkeyFromKeyword getHotkeyFromKeyword;

    /// <summary>
    /// Gets client display name.
    /// </summary>
    public TS3Functions_getClientDisplayName getClientDisplayName;

    /// <summary>
    /// Gets bookmark list.
    /// </summary>
    public TS3Functions_getBookmarkList getBookmarkList;

    /// <summary>
    /// Gets profiles list.
    /// </summary>
    public TS3Functions_getProfileList getProfileList;

    /// <summary>
    /// Connect.
    /// </summary>
    public TS3Functions_guiConnect guiConnect;

    /// <summary>
    /// Connects to bookmark.
    /// </summary>
    public TS3Functions_guiConnectBookmark guiConnectBookmark;

    /// <summary>
    /// Creates new bookmark.
    /// </summary>
    public TS3Functions_createBookmark createBookmark;

    /// <summary>
    /// Gets the permission by its name.
    /// </summary>
    public TS3Functions_getPermissionIDByName getPermissionIDByName;

    /// <summary>
    /// Gets needed permission for the client.
    /// </summary>
    public TS3Functions_getClientNeededPermission getClientNeededPermission;

    public TS3Functions_notifyKeyEvent notifyKeyEvent;

}

