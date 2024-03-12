using System;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class CacheOptions
{
    /// <summary>
    /// Downloads the current war for a clan which is warring one of your tracked clans, but otherwise would not be downloaded.
    /// This ensures the most recent data is available.
    /// It may help if a tracked clan's war log is private.
    /// It also helps get the final war stats in the event the clan searches for a new war immediately.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ActiveWarServiceOptions ActiveWars { get; } = new ActiveWarServiceOptions { ConcurrentUpdates = 50, DelayBeforeExecution = TimeSpan.FromMinutes(5), DelayBetweenExecutions = TimeSpan.FromMinutes(10), Enabled = true };

    /// <summary>
    /// Iterates the Clan cached table searching for any clan with DownloadMembers enabled.
    /// Every player present in the clan will be downloaded.
    /// Players added to the Players table by this monitor will have Download set to false.
    /// When the village leaves the tracked clan, it will no longer update and will eventually be removed from the cache.
    /// If you wish to continue tracking these villages, on the OnClanUpdated event check for new members
    /// using Clan.ClanMembersJoined(e.Stored, e.Fetched) and add them to the PlayersClient with Download set to true.
    /// </summary>
    public MemberServiceOptions ClanMembers { get; } = new MemberServiceOptions { DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(5), Enabled = true };

    /// <summary>
    /// Download the clan, current war, war log, and CWL group.
    /// </summary>
    public ClanServiceOptions Clans { get; } = new ClanServiceOptions { ConcurrentUpdates = 200, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(5), Enabled = true };

    /// <summary>
    /// This will keep any already downloaded CWL war up to date.
    /// </summary>
    public CwlWarServiceOptions CwlWars { get; } = new CwlWarServiceOptions { ConcurrentUpdates = 500, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(5), Enabled = true };

    [EditorBrowsable(EditorBrowsableState.Never)]
    public StalePlayerServiceOptions DeleteStalePlayers { get; } = new StalePlayerServiceOptions { DelayBeforeExecution = TimeSpan.FromMinutes(20), DelayBetweenExecutions = TimeSpan.FromMinutes(20), Enabled = true };

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int MaxConcurrentEvents { get; set; } = 25;

    /// <summary>
    /// Queries the clan's league group from the cache to obtain the war tags.
    /// The API is then queried for each war tag. 
    /// If the resulting war does not contain the desired clan, the war will be stored in memory.
    /// If the resulting war does contain the desired clan the war the NewWar event will be fired.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public NewCwlWarServiceOptions NewCwlWars { get; } = new NewCwlWarServiceOptions { ConcurrentUpdates = 10, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromMinutes(2), Enabled = true };

    /// <summary>
    /// Queries the current war cache for any war not yet announced.
    /// Fires the NewWar event, and adds the war to the War table.
    /// NOTE: the Clans service must be enabled as well as DownloadCurrentWar
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public NewWarServiceOptions NewWars { get; } = new NewWarServiceOptions { ConcurrentUpdates = 50, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(15), Enabled = true };

    /// <summary>
    /// Iterates the Players cached table searching for players with Download set to true.
    /// </summary>
    public PlayerServiceOptions Players { get; } = new PlayerServiceOptions { ConcurrentUpdates = 50, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(5), Enabled = true };

    /// <summary>
    /// Iterates over the Wars cached table.
    /// Queries the CurrentWar cached table for both clans in the war.
    /// Takes the most recent of the two, checks if any changes have been downloaded, and fires the appropriate events.
    /// </summary>
    public WarServiceOptions Wars { get; } = new WarServiceOptions { ConcurrentUpdates = 500, DelayBeforeExecution = TimeSpan.FromSeconds(5), DelayBetweenExecutions = TimeSpan.FromSeconds(5), Enabled = true };

    /// <summary>
    /// Used to bypass cache. Only SuperCell approved users may use this operation.
    /// </summary>
    public bool? RealTime { get; set; }
}
