using System;

namespace CocApi.Cache
{
    public class ExceptionEventArgs : LogEventArgs
    {
        public Exception Exception { get; internal set; }

        public ExceptionEventArgs(string method, Exception exception) : base(method, LogLevel.Error, exception.Message)
        {
            Exception = exception;
        }
    }
}
