using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache
{
    public class ClanLogEventArgs : LogEventArgs
    {
        public Clan Clan { get; private set; }

        public ClanLogEventArgs(string source, string method, Clan clan) : base(source, method, LogLevel.Trace)
        {
            Clan = clan;
        }
    }
}
