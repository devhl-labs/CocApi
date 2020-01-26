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

        private static void PrintLogTitle(LoggingEvent loggingEvent)
        {
            switch (loggingEvent)
            {
                //green
                case LoggingEvent.UpdateServiceStarted:
                case LoggingEvent.HttpResponseStatusCodeSuccessful:
                case LoggingEvent.UpdatingClan:
                case LoggingEvent.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[dbug] ");
                    break;

                //yellow
                case LoggingEvent.HttpResponseError:
                case LoggingEvent.HttpResponseStatusCodeUnsuccessful:
                case LoggingEvent.InvalidTag:
                case LoggingEvent.IsPremptiveRateLimited:
                case LoggingEvent.UnhandledCase:
                case LoggingEvent.Unknown:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[warn] ");
                    break;

                //red
                case LoggingEvent.UpdateServiceEnding:
                case LoggingEvent.Exception:
                case LoggingEvent.IsRateLimited:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("[crit] ");
                    break;

                default:

                    break;
            }

            ResetConsoleColor();
        }

        public Task Log<T>(LoggingEvent loggingEvent, string? message = null) => Log(typeof(T).Name, loggingEvent, message);

        public Task Log<T>(LoggingEvent loggingEvent, Exception exception) => Log(typeof(T).Name, loggingEvent, exception.Message);

        public Task Log(string source, LoggingEvent loggingEvent, Exception exception) => Log(source, loggingEvent, exception.Message);

        public Task Log(string source, LoggingEvent loggingEvent, string? message = null)
        {
            if (loggingEvent == LoggingEvent.IsPremptiveRateLimited || loggingEvent == LoggingEvent.UpdatingClan) return Task.CompletedTask;

            if (source.Length > 15) source = source[0..15];

            source = source.PadRight(15);

            lock (_logLock)
            {
                PrintLogTitle(loggingEvent);

                Console.WriteLine($"{DateTime.UtcNow.ToShortTimeString()}  | {source} | {message ?? loggingEvent.ToString()}");
            }

            return Task.CompletedTask;
        }


    }
}
