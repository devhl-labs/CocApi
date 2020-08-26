using System;
using System.Collections.Generic;

namespace CocApi.Cache
{
    public sealed class CocApiConfiguration
    {
        public string ClashApiBaseAddress { get; set; } = "https://api.clashofclans.com/v1/";

        /// <summary>
        /// List of tokens used to query the SC Api
        /// </summary>
        public IList<string> Tokens { get; set; } = new List<string>();

        /// <summary>
        /// If you watch many clans increase this number for faster updates.
        /// </summary>
        public int ConcurrentUpdates { get; set; } = 1;

        public TimeSpan DelayBetweenUpdates { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Defines how quickly a SC Api token may be reused. Default is 1 seconds for testing purposes. You may make it faster for production.
        /// </summary>
        public TimeSpan TokenTimeOut { get; set; } = new TimeSpan(0, 0, 1);

        /// <summary>
        /// Defines how long to wait for HTTP requests before throwing <see cref="ServerTookTooLongToRespondException"/>
        /// </summary>
        public TimeSpan TimeToWaitForWebRequests { get; set; } = new TimeSpan(0, 0, 2);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueWarTimeToLive { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan CurrentWarTimeToLive { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan NotInWarTimeToLive { get; set; } = new TimeSpan(0, 20, 00);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan PrivateWarLogTimeToLive { get; set; } = new TimeSpan(0, 20, 00);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueGroupTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan ClansTimeToLive { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan VillageTimeToLive { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This property defines how often to check if a clan has joined CWL.
        /// </summary>
        public TimeSpan LeagueGroupNotFoundTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This property defines how often to download the war log.
        /// </summary>
        public TimeSpan WarLogTimeToLive { get; set; } = new TimeSpan(0, 0, 0);



        //Paginated Options
        //public TimeSpan Clans { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan TopBuilderClans { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan TopMainClans { get; set; } = new TimeSpan(1, 0, 0);


        public TimeSpan Labels { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan Leagues { get; set; } = new TimeSpan(1, 0, 0);


        public TimeSpan Locations { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan TopMainVillages { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan TopBuilderVillages { get; set; } = new TimeSpan(1, 0, 0);

        public TimeSpan WarLeagues { get; set; } = new TimeSpan(1, 0, 0);











        public string? DatabasePath { get; set; }

        public string DatabaseName { get; set; } = "CocApiDatabase.sqlite";

        public string ConnectionString { get; set; } = "Data Source=cocapi.db";
    }
}
