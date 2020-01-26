using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public interface ILogger
    {
        Task Log<T>(LoggingEvent loggingEvent, string? message = null);

        Task Log<T>(LoggingEvent loggingEvent, Exception exception);

        Task Log(string source, LoggingEvent loggingEvent, string? message = null);

        Task Log(string source, LoggingEvent loggingEvent, Exception exception);
    }
}
