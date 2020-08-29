using System;

namespace CocApi.Test
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    public class LogService
    {
        private object LogLock { get; } = new object();

        public LogService()
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

        public void Log(LogLevel logLevel, string source, params string?[] messages)
        {
            lock (LogLock)
            {
                PrintLogLevel(logLevel);

                Console.Write(DateTime.UtcNow.ToShortTimeString().PadRight(8));
                Console.Write($"  | {source.PadRight(15)[..15]}");

                foreach (string? message in messages)
                {
                    if (string.IsNullOrEmpty(message))
                        continue;

                    Console.Write($" | {message}");
                }

                Console.WriteLine();
            }
        }
    }
}
