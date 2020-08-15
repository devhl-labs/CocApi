using System;
using System.Threading.Tasks;

namespace CocApi.Test
{
    public class LogService
    {
        private static object LogLock { get; } = new object();

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

        public void Log(LogLevel logLevel, string source, string? method, string? message)
        {
            lock (LogLock)
            {
                PrintLogLevel(logLevel);

                Console.Write(DateTime.UtcNow.ToShortTimeString());
                Console.Write($"  | { source.PadRight(15)[..15]}");
                if (method != null)
                    Console.Write($" | {method}");
                if (message != null)
                    Console.Write($" | {message}");
                Console.WriteLine();
            }
        }
    }

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
}
