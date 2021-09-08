using System;
using System.Threading.Tasks;

namespace CocApi.Test
{
    public static class LogService
    {
        private static object LogLock { get; } = new();

        static LogService()
        {
            Log(LogLevel.Information, nameof(LogService), null, "Press CTRL-C to exit");
        }

        private static void ResetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void PrintLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[trac] ");
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[dbug] ");
                    break;

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

        public static void Log(LogLevel logLevel, string source, params string?[] messages)
        {
            lock (LogLock)
            {
                PrintLogLevel(logLevel);

                Console.Write(DateTime.UtcNow.ToShortTimeString().PadRight(8));
                Console.Write($"  | {source.PadRight(15)[..15]}");

                foreach (string? message in messages)
                {
                    if (string.IsNullOrWhiteSpace(message))
                        continue;

                    Console.Write($" | {message}");
                }

                Console.WriteLine();
            }
        }

        public static Task OnLog(object sender, LogEventArgs log)
        {
            Log(
                log.LogLevel,
                sender.GetType().Name,
                new string?[] { string.Format(log.MessageTemplate ?? "", log.Params ?? Array.Empty<string>()),
                    log.Exception?.Message,
                    log.Exception?.InnerException?.Message });

            return Task.CompletedTask;
        }

        public static Task OnHttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            string seconds = ((int)log.HttpRequestResult.Elapsed.TotalSeconds).ToString();

            if (log.HttpRequestResult is HttpRequestException exception)
                Log(LogLevel.Warning, sender.GetType().Name, seconds, exception.Path, exception.Message, exception.InnerException?.Message);
            else if (log.HttpRequestResult is HttpRequestNonSuccess nonSuccess)
                Log(LogLevel.Debug, sender.GetType().Name, seconds, nonSuccess.Path, nonSuccess.Reason);
            else
                Log(LogLevel.Information, sender.GetType().Name, seconds, log.HttpRequestResult.Path);

            return Task.CompletedTask;
        }
    }
}
