using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache
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
