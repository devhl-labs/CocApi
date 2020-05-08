using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class VillageLogEventArgs : LogEventArgs
    {
        public Village Village { get; private set; }

        public VillageLogEventArgs(string source, string method, Village village) : base(source, method, LogLevel.Trace)
        {
            Village = village;
        }
    }
}
