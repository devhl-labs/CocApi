using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using CocApi.Cache.Models.Wars;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache
{
    public class CurrentWarLogEventArgs : LogEventArgs
    {
        public CurrentWar Queued { get; }

        public IWar? Fetched { get; }

        public CurrentWarLogEventArgs(string source, string method, CurrentWar queued, IWar? fetched) : base(source, method, LogLevel.Trace)
        {
            Queued = queued;

            Fetched = fetched;
        }
    }
}
