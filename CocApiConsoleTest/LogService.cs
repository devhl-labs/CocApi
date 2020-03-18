using System;
using System.Threading.Tasks;

using devhl.CocApi;

namespace CocApiConsoleTest
{
    public class LogService : ILogger
    {
        private static readonly object _logLock = new object();

        private static void ResetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void PrintLogTitle(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[trace]");
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[dbug] ");
                    break;

                case LogLevel.None:
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[info] ");
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[warn] ");
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("[err]  ");
                    break;

                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[crit] ");
                    break;
            }

            ResetConsoleColor();
        }

        //private static void PrintLogTitle(LoggingEvent loggingEvent)
        //{
        //    switch (loggingEvent)
        //    {
        //        //green
        //        case LoggingEvent.UpdateServiceStarted:
        //        case LoggingEvent.HttpResponseStatusCodeSuccessful:
        //        case LoggingEvent.UpdatingClan:
        //        case LoggingEvent.Debug:
        //            Console.ForegroundColor = ConsoleColor.Green;
        //            Console.BackgroundColor = ConsoleColor.Black;
        //            Console.Write("[dbug] ");
        //            break;

        //        //yellow
        //        case LoggingEvent.HttpResponseError:
        //        case LoggingEvent.HttpResponseStatusCodeUnsuccessful:
        //        case LoggingEvent.InvalidTag:
        //        case LoggingEvent.IsPremptiveRateLimited:
        //        case LoggingEvent.UnhandledCase:
        //        case LoggingEvent.Unknown:
        //            Console.ForegroundColor = ConsoleColor.Yellow;
        //            Console.BackgroundColor = ConsoleColor.Black;
        //            Console.Write("[warn] ");
        //            break;

        //        //red
        //        case LoggingEvent.UpdateServiceEnding:
        //        case LoggingEvent.Exception:
        //        case LoggingEvent.IsRateLimited:
        //            Console.ForegroundColor = ConsoleColor.White;
        //            Console.BackgroundColor = ConsoleColor.Red;
        //            Console.Write("[crit] ");
        //            break;

        //        default:

        //            break;
        //    }

        //    ResetConsoleColor();
        //}

        //public Task Log<T>(LoggingEvent loggingEvent, string? message = null) => Log(typeof(T).Name, loggingEvent, message);

        //public Task Log<T>(LoggingEvent loggingEvent, Exception exception) => Log(typeof(T).Name, loggingEvent, exception.Message);

        //public Task Log(string source, LoggingEvent loggingEvent, Exception exception) => Log(source, loggingEvent, exception.Message);

        //public Task Log(string source, LoggingEvent loggingEvent, string? message = null)
        //{
        //    if (loggingEvent == LoggingEvent.IsPremptiveRateLimited || loggingEvent == LoggingEvent.UpdatingClan) return Task.CompletedTask;

        //    if (source.Length > 15) source = source[0..15];

        //    source = source.PadRight(15);

        //    lock (_logLock)
        //    {
        //        PrintLogTitle(loggingEvent);

        //        Console.WriteLine($"{DateTime.UtcNow.ToShortTimeString()}  | {source} | {message ?? loggingEvent.ToString()}");
        //    }

        //    return Task.CompletedTask;
        //}

        //public Task LogAsync<T>(string? message, LogLevel logLevel, LoggingEvent loggingEvent = LoggingEvent.Unknown)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task LogAsync<T>(Exception exception, LogLevel logLevel, LoggingEvent loggingEvent = LoggingEvent.Unknown)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task LogAsync(string source, Exception exception, LogLevel logLevel, LoggingEvent loggingEvent = LoggingEvent.Unknown)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task LogAsync<T>(LogLevel logLevel, LoggingEvent loggingEvent)
        //{
        //    throw new NotImplementedException();
        //}

        public Task LogAsync<T>(string? message, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => 
            LogAsync(typeof(T).Name, logLevel, loggingEvent, message);

        public Task LogAsync<T>(Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => 
            LogAsync(typeof(T).Name, logLevel, loggingEvent, exception.Message);

        public Task LogAsync(string source, Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => 
            LogAsync(source, logLevel, loggingEvent, exception.Message);

        public Task LogAsync<T>(LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => 
            LogAsync(typeof(T).Name, logLevel, loggingEvent, null);


        public Task LogAsync(string source, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null)
        {
            if (loggingEvent == LoggingEvent.IsPremptiveRateLimited || loggingEvent == LoggingEvent.UpdatingClan)
                return Task.CompletedTask;

            if (source.Length > 15)
                source = source[0..15];

            source = source.PadRight(15);

            lock (_logLock)
            {
                PrintLogTitle(logLevel);

                Console.WriteLine($"{DateTime.UtcNow.ToShortTimeString()}  | {source} | {message ?? loggingEvent.ToString()}");
            }

            return Task.CompletedTask;
        }
    }
}
