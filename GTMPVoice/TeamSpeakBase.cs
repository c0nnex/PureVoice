using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TeamSpeakPlugin;

namespace GTMPVoice
{
    internal abstract class TeamSpeakBase
    {
        public static TS3Functions Functions;
        public static uint ERROR_OK = 0;
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly int SizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

        internal static void SetFunctionPointer(TS3Functions functions)
        {
            Functions = functions;
        }

        internal static void Log(string str,params object[] args)
        {
            GTMPVoicePlugin.Log(str, args);
        }

        internal bool Check(uint error, [CallerMemberName] string tag = null, [CallerFilePath] string code = null, [CallerLineNumber] int codeLine = 0) { return Check((Error)error,tag,code,codeLine); }
        internal bool Check(Error error, string tag = null, string code = null, int codeLine = 0)
        {
            if (error != Error.Ok && error != Error.OkNoUpdate)
            {
                Log("ERROR: {0} {1} ({2} {3}:{4})", error,(uint)error,tag,code,codeLine);
                return false;
            }
            return true;
        }

        internal static List<T> ReadShortIDList<T>(IntPtr pointer, Func<ushort, T> createFunc, bool free = true)
        {
            if (pointer == IntPtr.Zero) return null;
            const int sizeOfItem = 2;
            List<T> result = new List<T>();
            for (int offset = 0; ; offset += sizeOfItem)
            {
                ushort id = (ushort)Marshal.ReadInt16(pointer, offset);
                if (id == 0) break;
                result.Add(createFunc(id));
            }
            if (free)
                Functions.freeMemory(pointer);
            return result;
        }

        internal static string ReadString(IntPtr pointer, bool free = true)
        {
            if (pointer == IntPtr.Zero) return null;
            int length = 0;
            while (Marshal.ReadByte(pointer, length) != 0) length += 1; ;
            byte[] bytes = new byte[length];
            Marshal.Copy(pointer, bytes, 0, length);
            if (free)
                Functions.freeMemory(pointer);
            return Encoding.UTF8.GetString(bytes);
        }

        internal static List<T> ReadLongIDList<T>(IntPtr pointer, Func<ulong, T> createFunc,bool free = true)
        {
            if (pointer == IntPtr.Zero) return null;
            const int sizeOfItem = 8;
            List<T> result = new List<T>();
            for (int offset = 0; ; offset += sizeOfItem)
            {
                ulong id = (ulong)Marshal.ReadInt64(pointer, offset);
                if (id == 0) break;
                result.Add(createFunc(id));
            }
            if (free)
                Functions.freeMemory(pointer);
            return result;
        }
    }
}
