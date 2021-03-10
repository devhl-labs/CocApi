using System;

namespace CocApi.Cache
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel LogLevel { get; }

        public string? Message { get; }

        public Exception? Exception { get; }

        public LogEventArgs(LogLevel logLevel = LogLevel.Information, string? message = null, Exception? exception = null)
        {
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
        }
    }
}
