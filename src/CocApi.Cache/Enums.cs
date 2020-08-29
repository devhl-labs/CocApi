using System;
using System.Runtime.Serialization;

namespace CocApi.Cache
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

    [Flags]
    public enum Announcements
    {
        None = 0,
        WarEndingSoon = 1,
        WarStartingSoon = 2,
        WarIsAccessible = 4,
        WarEndNotSeen = 8,
        WarStarted = 16,
        WarEnded = 32
    }
}
