using System;
using System.ComponentModel;

namespace CocApi.Cache
{
    public class CacheOptions
    {
        /// <summary>
        /// Downloads the current war for a clan which is warring one of your tracked clans, but otherwise would not be downloaded. 
        /// This ensures the most recent data is available. 
        /// It may help if a tracked clan's war log is private. 
        /// It also helps get the final war stats in the event the clan searches for a new war immediately.
        /// </summary>
        public ServiceOptions ActiveWars { get; } = new ServiceOptions { DelayBeforeExecution = TimeSpan.FromMinutes(5), DelayBetweenExecutions = TimeSpan.FromMinutes(10) };

        /// <summary>
        /// Iterates the Clan cached table searching for any clan with DownloadMembers enabled. 
        /// Every player present in the clan will be downloaded. 
        /// Players added to the Players table by this monitor will have Download set to false. 
        /// When the village leaves the tracked clan, it will no longer update and will eventually be removed from the cache. 
        /// If you wish to continue tracking these villages, on the OnClanUpdated event check for new members 
        /// using Clan.ClanMembersJoined(e.Stored, e.Fetched) and add them to the PlayersClient with Download set to true.
        /// </summary>
        public ServiceOptionsBase ClanMembers { get; } = new ServiceOptionsBase { DelayBetweenExecutions = TimeSpan.FromSeconds(5) };

        /// <summary>
        /// Download the clan, current war, war log, and CWL group.
        /// </summary>
        public ClanServiceOptions Clans { get; } = new ClanServiceOptions { ConcurrentUpdates = 200, DelayBetweenExecutions = TimeSpan.FromSeconds(5) };
        
        /// <summary>
        /// This will keep any already downloaded CWL war up to date.
        /// </summary>
        public ServiceOptions CwlWars { get; } = new ServiceOptions { ConcurrentUpdates = 500, DelayBetweenExecutions = TimeSpan.FromSeconds(5) };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ServiceOptionsBase DeleteStalePlayers { get; } = new ServiceOptionsBase { DelayBeforeExecution = TimeSpan.FromMinutes(20), DelayBetweenExecutions = TimeSpan.FromMinutes(20) };
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxConcurrentEvents { get; set; } = 25;

        /// <summary>
        /// Queries the clan's league group from the cache to obtain the war tags. 
        /// The API is then queried for each war tag. 
        /// If the resulting war does not contain the desired clan, the war will be stored in memory. 
        /// If the resulting war does contain the desired clan the war the NewWar event will be fired.
        /// </summary>
        public ServiceOptions NewCwlWars { get; } = new ServiceOptions { DelayBetweenExecutions = TimeSpan.FromMinutes(2), ConcurrentUpdates = 10 };

        /// <summary>
        /// Queries the current war cache for any war not yet announced. 
        /// Fires the NewWar event, and adds the war to the War table.
        /// NOTE: the Clans service must be enabled as well as DownloadCurrentWar
        /// </summary>
        public ServiceOptions NewWars { get; } = new ServiceOptions { DelayBetweenExecutions = TimeSpan.FromSeconds(15) };

        /// <summary>
        /// Iterates the Players cached table searching for players with Download set to true.
        /// </summary>
        public ServiceOptions Players { get; } = new ServiceOptions { DelayBetweenExecutions = TimeSpan.FromSeconds(5) };

        /// <summary>
        /// Iterates over the Wars cached table. 
        /// Queries the CurrentWar cached table for both clans in the war. 
        /// Takes the most recent of the two, checks if any changes have been downloaded, and fires the appropriate events.
        /// </summary>
        public ServiceOptions Wars { get; } = new ServiceOptions { ConcurrentUpdates = 500, DelayBetweenExecutions = TimeSpan.FromSeconds(5) };
    }
}
