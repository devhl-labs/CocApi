using System;
using System.Collections.Generic;

using devhl.CocApi.Exceptions;
using Newtonsoft.Json;

namespace devhl.CocApi
{
    public sealed class CocApiConfiguration
    {
        /// <summary>
        /// Set this to true if you want to store HTTP responses in memory.
        /// </summary>
        public bool CacheHttpResponses { get; set; } = true;

        /// <summary>
        /// Controls whether any clan will download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.Auto;

        /// <summary>
        /// Controls whether any clan will be able to download villages.
        /// </summary>
        public bool DownloadVillages { get; set; } = false;

        /// <summary>
        /// Controls whether any clan will download the current war.
        /// </summary>
        public bool DownloadCurrentWar { get; set; } = true;

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
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueWarTimeToLive { get; set; } = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan CurrentWarTimeToLive { get; set; } = new TimeSpan(0, 0, 15);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan LeagueGroupTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan ClanTimeToLive { get; set; } = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// </summary>
        public TimeSpan VillageTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Defines how long to wait before an HTTP request is considered expired.
        /// This property defines how often to check if a clan has joined CWL.
        /// </summary>
        public TimeSpan LeagueGroupNotFoundTimeToLive { get; set; } = new TimeSpan(1, 0, 0);

    }
}
