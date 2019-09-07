using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary
{
    public struct LogMessage
    {
        /// <summary>
        ///     Gets the severity of the log entry.
        /// </summary>
        public LogSeverity Severity { get; }
        /// <summary>
        ///     Gets the source of the log entry.
        /// </summary>
        public string Source { get; }
        /// <summary>
        ///     Gets the message of this log entry.
        /// </summary>
        public string Message { get; }
        /// <summary>
        ///     Gets the exception of this log entry.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        ///     Initializes a new LogMessage with the severity, source, message of the event, and
        ///     optionally, an exception.
        /// </summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="source">The source of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="exception">The exception of the event.</param>
        public LogMessage(LogSeverity severity, string source, string message, Exception? exception = null)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Exception = exception;
        }

        public override string ToString() => ToString();
        public string ToString(StringBuilder? builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Utc, int? padSource = 25)
        {
            string sourceName = Source;
            string message = Message;
            string? exMessage = fullException ? Exception?.ToString() : Exception?.Message;

            int maxLength = 1 +
                (prependTimestamp ? 8 : 0) + 1 +
                (padSource ?? sourceName?.Length ?? 0) + 1 +
                (message?.Length ?? 0) +
                (exMessage?.Length ?? 0) + 3;

            if (builder == null)
                builder = new StringBuilder(maxLength);
            else
            {
                builder.Clear();
                builder.EnsureCapacity(maxLength);
            }

            if (prependTimestamp)
            {
                DateTime now;
                if (timestampKind == DateTimeKind.Utc)
                    now = DateTime.UtcNow;
                else
                    now = DateTime.Now;
                if (now.Hour < 10)
                    builder.Append('0');
                builder.Append(now.Hour);
                builder.Append(':');
                if (now.Minute < 10)
                    builder.Append('0');
                builder.Append(now.Minute);
                builder.Append(':');
                if (now.Second < 10)
                    builder.Append('0');
                builder.Append(now.Second);
                builder.Append(' ');
            }
            if (sourceName != null)
            {
                if (padSource.HasValue)
                {
                    if (sourceName.Length < padSource.Value)
                    {
                        builder.Append(sourceName);
                        builder.Append(' ', padSource.Value - sourceName.Length);
                    }
                    else if (sourceName.Length > padSource.Value)
                        builder.Append(sourceName.Substring(0, padSource.Value));
                    else
                        builder.Append(sourceName);
                }
                builder.Append(' ');
            }
            if (!string.IsNullOrEmpty(message))
            {
                for (int i = 0; i < message.Length; i++)
                {
                    //Strip control chars
                    char c = message[i];
                    if (!char.IsControl(c))
                        builder.Append(c);
                }
            }
            if (exMessage != null)
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    builder.Append(':');
                    builder.AppendLine();
                }
                builder.Append(exMessage);
            }

            return builder.ToString();
        }
    }
}
