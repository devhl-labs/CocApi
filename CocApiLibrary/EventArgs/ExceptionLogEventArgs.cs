using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class ExceptionLogEventArgs : LogEventArgs
    {
        public Exception Exception { get; internal set; }

        public ExceptionLogEventArgs(string source, string method, Exception exception) : base(source, method, LogLevel.Error, exception.Message)
        {
            Exception = exception;
        }
    }
}
