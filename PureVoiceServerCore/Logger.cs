using System;
using System.Collections.Generic;
using System.Text;

namespace PureVoice.Server
{
    class ConsoleLogger : ILogger
    {
        private string v;
        private LogLevel _MinLogLevel = LogLevel.DEBUG;

        public ConsoleLogger(string v)
        {
            this.v = v;
        }

        public void SetMinLogLevel(LogLevel logLevel)
        {
            _MinLogLevel = logLevel;
        }

        public void Trace(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.TRACE)
                LogIt("TRACE", v, args);
        }
        public void Debug(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.DEBUG)
                LogIt("DEBUG", v, args);
        }
        public void Info(string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.INFO)
                LogIt("INFO", v, args);
        }

        public void Warn(Exception ex)
        {
            if (_MinLogLevel >= LogLevel.WARN)
                LogIt("WARN", ex.ToString());
        }
        public void Warn(Exception ex, string v, params object[] args)
        {
            if (_MinLogLevel >= LogLevel.TRACE)
            {
                LogIt("WARN", ex.ToString());
                LogIt("WARN", v, args);
            }
        }
        public void Warn(string v, params object[] args)
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
