using System;

namespace PureVoice.Server
{
    public enum LogLevel
    {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR
    }
    public interface ILogger
    {
        void Debug(string v, params object[] args);
        void Info(string v, params object[] args);
        void SetMinLogLevel(LogLevel logLevel);
        void Trace(string v, params object[] args);
        void Warn(Exception ex);
        void Warn(Exception ex, string v, params object[] args);
        void Warn(string v, params object[] args);
    }
}