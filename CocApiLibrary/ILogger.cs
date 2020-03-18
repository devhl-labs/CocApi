using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public interface ILogger
    {
        Task LogAsync<T>(string? message, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown);

        Task LogAsync<T>(Exception exception, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown);

        Task LogAsync(string source, Exception exception, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown);

        Task LogAsync<T>(LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown);


        Task LogAsync(string source, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null);


        //Task LogAsync<T>(LogLevel logLevel, string? message = null);

        //Task LogAsync<T>(LogLevel logLevel, Exception exception);

        //Task LogAsync(string source, LogLevel logLevel, string? message = null);
    }
}
