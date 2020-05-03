//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;

//using System.Collections.Immutable;
//using devhl.CocApi.Models;

//namespace devhl.CocApi
//{
    //public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);

    //public delegate Task LogEventHandler(string source, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null);

    //public sealed partial class CocApi : IDisposable
    //{
        ///// <summary>
        ///// Fires if you query the Api during an outage.
        ///// If the service is not available, you may still try to query the Api if you wish.
        ///// </summary>
        //public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;

        //public event LogEventHandler? Log;

        //internal void LogEvent(string source, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null) => Log?.Invoke(source, logLevel, loggingEvent, message);
        

        //internal void LogEvent<T>(string? message, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, message);

        //internal void LogEvent<T>(Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, exception.Message);

        //internal void LogEvent(string source, Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(source, logLevel, loggingEvent, exception.Message);

        //internal void LogEvent<T>(LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, null);


        //internal void ClanUpdaterCrashDetectedEvent()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            //wait to allow the updater to finish crashing
        //            await Task.Delay(5000).ConfigureAwait(false);

        //            StartUpdatingClans();
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        LogEvent<CocApi>($"A clan update service crashed and could not be restarted.", LogLevel.Critical, LoggingEvent.CrashDetected);
        //    }
        //}

        //internal void WarUpdaterCrashDetectedEvent()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            //wait to allow the updater to finish crashing
        //            await Task.Delay(5000).ConfigureAwait(false);

        //            StartUpdatingWars();
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        LogEvent<CocApi>($"The war update service crashed and could not be restarted.", LogLevel.Critical, LoggingEvent.CrashDetected);
        //    }
        //}
//    }
//}
