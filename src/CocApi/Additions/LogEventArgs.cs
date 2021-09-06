using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocApi
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel LogLevel { get; }

        public string? MessageTemplate { get; }

        public string[]? Params { get; }

        public Exception? Exception { get; }

        internal LogEventArgs(LogLevel logLevel = LogLevel.Information, Exception? exception = null, string? message = null, string[]? parameters = null)
        {
            LogLevel = logLevel;
            MessageTemplate = message;
            Exception = exception;
            Params = parameters;
        }
    }
}
