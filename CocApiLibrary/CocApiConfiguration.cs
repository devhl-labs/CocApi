using System;
using System.Collections.Generic;

using devhl.CocApi.Exceptions;

namespace devhl.CocApi
{
    public sealed class CocApiConfiguration
    {
        /// <summary>
        /// List of tokens used to query the SC Api
        /// </summary>
        public IList<string> Tokens { get; set; } = new List<string>();


        /// <summary>
        /// If you watch many clans increase this number for faster updates.
        /// </summary>
        public int NumberOfUpdaters { get; set; } = 1;

        /// <summary>
        /// Defines how quickly a SC Api token may be reused.  Default is 3 seconds for testing purposes.  
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
        public TimeSpan LeagueWarApiModelTimeToLive { get; set; } = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan CurrentWarApiModelTimeToLive { get; set; } = new TimeSpan(0, 0, 15);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueGroupApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan ClanApiModelTimeToLive { get; set; } = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan VillageApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This end point is not cached, so do not spam it.
        /// </summary>
        public TimeSpan WarLogApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This end point is not cached, so do not spam it.  
        /// Each request will query the server.
        /// </summary>
        public TimeSpan ClanSearchApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);


        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This end point is not cached, so do not spam it.  
        /// Each request will query the server.
        /// </summary>
        public TimeSpan VillageLeagueSearchApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This end point is not cached, so do not spam it.  
        /// Each request will query the server.
        /// </summary>
        public TimeSpan LocationSearchApiModelTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This property defines how often to check if a clan has joined CWL.
        /// </summary>
        public TimeSpan LeagueGroupNotFoundTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

    }
}
