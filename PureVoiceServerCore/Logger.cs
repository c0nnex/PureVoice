using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.Server
{
    internal class LogManager
    {
        internal static Logger GetCurrentClassLogger()
        {
            return new Logger("PureVoice");
        }
    }

    internal class Logger
    {
        private string v;
        private LogLevel _MinLogLevel = LogLevel.DEBUG;

        internal Logger(string v)
        {
            this.v = v;
        }

        internal void SetMinLogLevel(LogLevel logLevel)
        {
            _MinLogLevel = logLevel;
        }

        internal void Trace(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.TRACE)
                LogIt("TRACE", v, args);
        }
        internal void Debug(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.DEBUG)
                LogIt("DEBUG", v, args);
        }
        internal void Info(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.INFO)
                LogIt("INFO", v, args);
        }

        internal void Warn(Exception ex)
        {
            if (_MinLogLevel >= LogLevel.WARN)
                LogIt("WARN", ex.ToString());
        }
        internal void Warn(Exception ex, string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.TRACE)
            {
                LogIt("WARN", ex.ToString());
                LogIt("WARN", v, args);
            }
        }
        internal void Warn(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.TRACE)
                LogIt("WARN", v, args);
        }

        private void LogIt(string level, string v, params object[] args)
        {
            Console.WriteLine(String.Format("PureVoice|{0}|{1}", level, String.Format(v, args)));
        }
    }
}
