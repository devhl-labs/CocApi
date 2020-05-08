using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
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
