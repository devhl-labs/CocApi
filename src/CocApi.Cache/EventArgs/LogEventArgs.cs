using System;

namespace CocApi.Cache
{
    public class LogEventArgs : EventArgs
    {
        public string Source { get; private set; }

        public string Method { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public string? Message { get; private set; }

        public LogEventArgs(string source, string method, LogLevel logLevel = LogLevel.Information, string? message = null)
        {
            Source = source;
            Method = method;
            LogLevel = logLevel;
            Message = message;
        }
    }
}
