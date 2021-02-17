using System;

namespace CocApi.Cache
{
    public class LogEventArgs : EventArgs
    {
        //public string Method { get; private set; }

        public LogLevel LogLevel { get; }

        public string? Message { get; }

        public Exception? Exception { get; }

        public LogEventArgs(/*string method,*/ LogLevel logLevel = LogLevel.Information, string? message = null, Exception? exception = null)
        {
            //Method = method;
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
        }
    }
}
