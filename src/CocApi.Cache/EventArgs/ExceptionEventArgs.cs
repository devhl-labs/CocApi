using System;

namespace CocApi.Cache
{
    public class ExceptionEventArgs : LogEventArgs
    {
        public Exception Exception { get; internal set; }

        public ExceptionEventArgs(string source, string method, Exception exception) : base(source, method, LogLevel.Error, exception.Message)
        {
            Exception = exception;
        }
    }
}
