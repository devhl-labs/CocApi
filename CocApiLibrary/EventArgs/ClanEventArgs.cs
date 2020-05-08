using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
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
