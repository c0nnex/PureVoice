using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTMPVoice
{
    internal class SmartLock
    {
        private object LockObject = new object();
        private string HoldingTrace = "";
        private string HoldingTraceStack = "";
        private string Name = "Unkown";
        private static int WARN_TIMEOUT_MS = 5000; //5 secs


        private SmartLock()
        {

        }

        public SmartLock(string name)
        {
            Name = name;

        }

        public void SetName(string name)
        {
            Name = name;
        }

        public static TOut FuncInvoke<TOut>(Func<TOut> func)
        {
            return func();
        }

        public TOut Lock<TOut>(Func<TOut> action, [CallerMemberName] string tag = null, [CallerFilePath] string code = null, [CallerLineNumber] int codeLine = 0)
        {
            try
            {
                Enter(tag,code,codeLine);
                return action.Invoke();
            }
            catch (Exception ex)
            {
                NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"SmartLock {Name} Lock action {ex} : {GetStackTrace(true)}");

                return default(TOut);
            }
            finally
            {
                Exit();
            }

        }

        public void Lock(Action action, [CallerMemberName] string tag = null, [CallerFilePath] string code = null, [CallerLineNumber] int codeLine = 0)
        {
            try
            {
                Enter(tag,code,codeLine);
                action.Invoke();
            }
            catch (Exception ex)
            {
                NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"SmartLock {Name} Lock action {ex} : {GetStackTrace(true)}");
            }
            finally
            {
                Exit();
            }

        }

        private void Enter(string tag,string code,int codeLine)
        {
            try
            {
                bool locked = false;
                int timeoutMS = 0;
                while (!locked)
                {
                    //keep trying to get the lock, and warn if not accessible after timeout
                    locked = Monitor.TryEnter(LockObject, WARN_TIMEOUT_MS);
                    if (!locked)
                    {
                        timeoutMS += WARN_TIMEOUT_MS;
                        NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"{Name} Lock held: {(timeoutMS / 1000)} secs by {HoldingTrace} requested by [{Thread.CurrentThread.ManagedThreadId}] {FormatCode(tag, code, codeLine)}");
                        NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"HOLDING: {HoldingTraceStack}");
                        NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"REQUESTING: {GetStackTrace(true)}");
                    }
                }

                //save a stack trace for the code that is holding the lock
                HoldingTrace = "[" + Thread.CurrentThread.ManagedThreadId + "] "+ FormatCode(tag, code, codeLine);
                //HoldingTraceStack = GetStackTrace(true);
            }
            catch (Exception ex)
            {
                NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"SmartLock {Name} Enter {ex.Message}");
            }
        }
        private string FormatCode(string tag, string code, int codeLine)
        {
            return $"{tag} {code}:{codeLine}";
        }

        private string GetStackTrace(bool full = false)
        {
            try
            {
                object trace;

                if (full)
                {
                    trace = new StackTrace();
                    return "[" + Thread.CurrentThread.ManagedThreadId + "]" + trace.ToString();
                }
            }
            catch { }
            return String.Empty;

        }

        private void Exit()
        {
            try
            {
                Monitor.Exit(LockObject);
                HoldingTrace = "released";
            }
            catch (Exception ex)
            {
                NetDebug.Logger?.WriteNet(ConsoleColor.Black, $"SmartLock {Name} Exit {ex}");
            }
        }
    }
}
