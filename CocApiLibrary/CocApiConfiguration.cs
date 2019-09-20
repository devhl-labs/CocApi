using System;
using System.Collections.Generic;
using System.Text;
using static CocApiLibrary.Enums;

namespace CocApiLibrary
{
    public class CocApiConfiguration
    {
        public IList<string> Tokens = new List<string>();

        public int NumberOfUpdaters { get; set; } = 1;

        public VerbosityType Verbosity { get; set; } = VerbosityType.None;


        public TimeSpan TokenTimeOut { get; set; } = new TimeSpan(0, 0, 3);

        public TimeSpan TimeToWaitForWebRequests { get; set; } = new TimeSpan(0, 0, 10);



        public TimeSpan LeagueWarAPIModelTimeToLive { get; set; } = new TimeSpan(0, 5, 0);

        public TimeSpan CurrentWarAPIModelTimeToLive { get; set; } = new TimeSpan(0, 0, 15);

        public TimeSpan LeagueGroupAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan ClanAPIModelTimeToLive { get; set; } = new TimeSpan(0, 10, 0);

        public TimeSpan VillageAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan WarLogAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

    }
}
