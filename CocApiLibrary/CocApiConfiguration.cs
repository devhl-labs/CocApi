using System;
using System.Collections.Generic;
using System.Text;
using static CocApiLibrary.Enums;
using CocApiLibrary.Exceptions;

namespace CocApiLibrary
{
    public class CocApiConfiguration
    {
        /// <summary>
        /// List of tokens used to query the SC API
        /// </summary>
        public IList<string> Tokens { get; set; } = new List<string>();


        /// <summary>
        /// If you watch many clans increase this number for faster updates.
        /// </summary>
        public int NumberOfUpdaters { get; set; } = 1;

        /// <summary>
        /// Defines how quickly a SC API token may be reused.  Default is 3 seconds for testing purposes.  
        /// </summary>
        public TimeSpan TokenTimeOut { get; set; } = new TimeSpan(0, 0, 3);

        /// <summary>
        /// Defines how long to wait for HTTP requests before throwing <see cref="ServerTookTooLongToRespondException"/>
        /// </summary>
        public TimeSpan TimeToWaitForWebRequests { get; set; } = new TimeSpan(0, 0, 10);

        /// <summary>
        /// Set this to true if you want to store HTTP responses in memory.
        /// </summary>
        public bool CacheHttpResponses { get; set; } = true;



        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueWarAPIModelTimeToLive { get; set; } = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan CurrentWarAPIModelTimeToLive { get; set; } = new TimeSpan(0, 0, 15);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueGroupAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan ClanAPIModelTimeToLive { get; set; } = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan VillageAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan WarLogAPIModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

    }
}
