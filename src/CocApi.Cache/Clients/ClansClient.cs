﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using CocApi.Rest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CocApi.Cache.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CocApi.Cache.Services.Options;
using CocApi.Rest.Apis;
using System.Collections.Concurrent;

namespace CocApi.Cache;

public class ClansClient : ClientBase<ClansClient>
{
    public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
    public event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
    public event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
    public event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
    public event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
    public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
    public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;
    public event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
    public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;


    public IClansApi ClansApi { get; }


    public ClansClient(
        ILogger<ClansClient> logger,
        IClansApi clansApi, 
        IServiceScopeFactory scopeFactory,
        Synchronizer synchronizer,
        ClanService clanService,
        NewWarService newWarService,
        NewCwlWarService newCwlWarService,
        WarService warService,
        CwlWarService cwlWarService,
        IOptions<CacheOptions> options
        )
    : base(logger, scopeFactory, synchronizer, options)
    {
        if (!(options.Value.CwlWars.Enabled == 
                options.Value.Clans.DownloadGroup == 
                options.Value.NewCwlWars.Enabled == 
                options.Value.Clans.Enabled))
        {
            logger.LogWarning("Your cache control options has CWL partially enabled.");
            // TODO: then why do all these booleans exist?
        }

        if (!(options.Value.ClanMembers.Enabled == options.Value.DeleteStalePlayers.Enabled))
            logger.LogWarning("You are downloading clan members but not deleting stale players. This will cause departed clan members to stay in cache forever.");

        if (!(options.Value.ActiveWars.Enabled ==
                options.Value.Wars.Enabled ==
                options.Value.Clans.Enabled ==
                options.Value.ClanWars.DownloadCurrentWar ==
                options.Value.NewWars.Enabled))
            logger.LogWarning("Your cache control options has current wars partially enabled.");

        ClansApi = clansApi;

        clanService.ClanUpdated += OnClanUpdatedAsync;
        clanService.ClanWarLeagueGroupUpdated += OnClanWarLeagueGroupUpdatedAsync;
        clanService.ClanWarLogUpdated += OnClanWarLogUpdatedAsync;

        newWarService.ClanWarAdded += OnClanWarAddedAsync;

        newCwlWarService.ClanWarAdded += OnClanWarAddedAsync;

        warService.ClanWarEnded += OnClanWarEndedAsync;
        warService.ClanWarEndingSoon += OnClanWarEndingSoonAsync;
        warService.ClanWarEndNotSeen += OnClanWarEndNotSeenAsync;
        warService.ClanWarStartingSoon += OnClanWarStartingSoonAsync;
        warService.ClanWarUpdated += OnClanWarUpdatedAsync;

        cwlWarService.ClanWarEnded += OnClanWarEndedAsync;
        cwlWarService.ClanWarEndingSoon += OnClanWarEndingSoonAsync;
        cwlWarService.ClanWarStartingSoon += OnClanWarStartingSoonAsync;
        cwlWarService.ClanWarUpdated += OnClanWarUpdatedAsync;
    }


    public async Task DeleteAsync(string tag)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        while (!Synchronizer.UpdatingClan.TryAdd(formattedTag, null))
            await Task.Delay(250).ConfigureAwait(false);

        try
        {
            CachedClan? cachedClan = await dbContext.Clans.FirstOrDefaultAsync(c => c.Tag == formattedTag).ConfigureAwait(false);

            if (cachedClan != null)
                dbContext.Clans.Remove(cachedClan);

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        finally
        {
            Synchronizer.UpdatingClan.TryRemove(formattedTag, out _);
        }
    }

    public async Task AddOrUpdateAsync(string tag, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
        => await AddOrUpdateAsync(new string[] { tag }, downloadClan, downloadWar, downloadLog, downloadGroup, downloadMembers).ConfigureAwait(false);

    public async Task AddOrUpdateAsync(
        IEnumerable<string> tags, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
    {
        HashSet<string> formattedTags = new();

        foreach (string tag in tags)
            formattedTags.Add(Clash.FormatTag(tag));

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c => formattedTags.Contains(c.Tag))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (string formattedTag in formattedTags)
        {
            CachedClan? cachedClan = cachedClans.FirstOrDefault(c => c.Tag == formattedTag);

            cachedClan ??= new CachedClan();

            cachedClan.Tag = formattedTag;
            cachedClan.Download = downloadClan;
            cachedClan.WarLog.Download = downloadLog;
            cachedClan.CurrentWar.Download = downloadWar;
            cachedClan.Group.Download = downloadGroup;
            cachedClan.DownloadMembers = downloadMembers;

            dbContext.Clans.Update(cachedClan);
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<CachedWar?> GetActiveClanWarOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedWar> cache = await dbContext.Wars
            .AsNoTracking()
            .Where(i => (i.ClanTag == formattedTag || i.OpponentTag == formattedTag))
            .OrderByDescending(w => w.PreparationStartTime)
            .ToListAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);

        if (cache.Count == 0)
            return null;

        return cache.FirstOrDefault(c => c.State == Rest.Models.WarState.InWar && c.EndTime > DateTime.UtcNow)
            ?? cache.FirstOrDefault(c => c.State == Rest.Models.WarState.Preparation && c.EndTime > DateTime.UtcNow)
            ?? cache.First();
    }

    public async Task<ClanWarLeagueGroup> GetOrFetchLeagueGroupAsync(string tag, Rest.Client.Option<bool> realtime = default, CancellationToken cancellationToken = default)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return (await dbContext.Clans.FirstOrDefaultAsync(g => g.Tag == formattedTag).ConfigureAwait(false))?.Group.Content
            ?? (await ClansApi.FetchClanWarLeagueGroupAsync(formattedTag, realtime, cancellationToken).ConfigureAwait(false)).Ok()!;
    }

    public async Task<ClanWarLeagueGroup?> GetOrFetchLeagueGroupOrDefaultAsync(string tag, Rest.Client.Option<bool> realtime = default, CancellationToken cancellationToken = default)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return (await dbContext.Clans.FirstOrDefaultAsync(g => g.Tag == formattedTag).ConfigureAwait(false))?.Group.Content
            ?? (await ClansApi.FetchClanWarLeagueGroupOrDefaultAsync(formattedTag, realtime, cancellationToken).ConfigureAwait(false))?.Ok();
    }

    public async Task<CachedWar> GetLeagueWarAsync(string warTag, DateTime season, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(warTag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        CachedWar war = await dbContext.Wars
            .AsNoTracking()
            .FirstAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);

        return war;
    }

    public async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(warTag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        CachedWar? war = await dbContext.Wars
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);

        return war;
    }

    public async Task<List<ClanWar>> GetOrFetchLeagueWarsAsync(ClanWarLeagueGroup group, Rest.Client.Option<bool> realtime = default, CancellationToken cancellationToken = default)
    {
        List<ClanWar> result = new();

        foreach (var round in group.Rounds)
            foreach (string warTag in round.WarTags.Where(t => t != "#0"))
            {
                ClanWar? clanWar = (await GetLeagueWarOrDefaultAsync(warTag, group.Season, cancellationToken).ConfigureAwait(false))?.Content;

                if (clanWar == null)
                    clanWar = (await ClansApi.FetchClanWarLeagueWarAsync(warTag, realtime, cancellationToken).ConfigureAwait(false)).Ok();

                if (clanWar.PreparationStartTime.Month == group.Season.Month && clanWar.PreparationStartTime.Year == group.Season.Year)
                    result.Add(clanWar);
            }

        return result;
    }

    public async Task<List<CachedWar>> GetClanWarsAsync(string tag, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Wars
            .AsNoTracking()
            .Where(i => i.ClanTag == formattedTag || i.OpponentTag == formattedTag)
            .ToListAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<CachedClan> GetCachedClanAsync(string tag, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Clans
            .AsNoTracking()
            .Where(i => i.Tag == formattedTag)
            .FirstAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<CachedClan?> GetCachedClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Clans
            .AsNoTracking()
            .Where(i => i.Tag == formattedTag)
            .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<List<CachedClan>> GetCachedClansAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = null)
    {
        List<string> formattedTags = new();

        foreach (string tag in tags)
            formattedTags.Add(Clash.FormatTag(tag));

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Clans
            .AsNoTracking()
            .Where(i => formattedTags.Contains(i.Tag))
            .ToListAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<Clan> GetOrFetchClanAsync(string tag, CancellationToken cancellationToken = default)
    {
        Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

        return result ?? (await ClansApi.FetchClanAsync(tag, cancellationToken).ConfigureAwait(false)).Ok()!;
    }

    public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag, CancellationToken cancellationToken = default)
    {
        Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

        return result ?? (await ClansApi.FetchClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Ok();
    }

    private async Task OnClanUpdatedAsync(object sender, ClanUpdatedEventArgs eventArgs)
    {
        if (ClanUpdated == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanUpdatedAsync), async () =>
        {
            await ClanUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        }, 
        eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private readonly ConcurrentDictionary<string, DateTime> _announcedWars = new();

    private async Task OnClanWarAddedAsync(object sender, WarAddedEventArgs eventArgs)
    {
        if (ClanWarAdded == null || !_announcedWars.TryAdd($"{eventArgs.War.PreparationStartTime}{eventArgs.War.Clans.First().Key}", eventArgs.War.PreparationStartTime))
            return;

        var keys = _announcedWars.Keys.ToArray();

        foreach (string key in keys)
        {
            DateTime preparationStartTime = _announcedWars.GetValueOrDefault(key);

            if (DateTime.UtcNow.AddDays(-1) > preparationStartTime)
                // prevent memory leak
                _announcedWars.TryRemove(key, out DateTime _);
        }

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarAddedAsync), async () =>
        {
            await ClanWarAdded.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarEndingSoonAsync(object sender, WarEventArgs eventArgs)
    {
        if (ClanWarEndingSoon == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarEndingSoonAsync), async () => 
        {
            await ClanWarEndingSoon.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarEndNotSeenAsync(object sender, WarEventArgs eventArgs)
    {
        if (ClanWarEndNotSeen == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarEndNotSeenAsync), async () => 
        { 
            await ClanWarEndNotSeen.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarEndedAsync(object sender, WarEventArgs eventArgs)
    {
        if (ClanWarEnded == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarEndedAsync), async () =>
        { 
            await ClanWarEnded.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarLeagueGroupUpdatedAsync(object sender, ClanWarLeagueGroupUpdatedEventArgs eventArgs)
    {
        if (ClanWarLeagueGroupUpdated == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarLeagueGroupUpdatedAsync), async () =>
        {
            await ClanWarLeagueGroupUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        },
            eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarLogUpdatedAsync(object sender, ClanWarLogUpdatedEventArgs eventArgs)
    {
        if (ClanWarLogUpdated == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarLogUpdatedAsync), async () => 
        {
            await ClanWarLogUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        },
            eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarStartingSoonAsync(object sender, WarEventArgs eventArgs)
    {
        if (ClanWarStartingSoon == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarStartingSoonAsync), async () => 
        {
            await ClanWarStartingSoon.Invoke(this, eventArgs).ConfigureAwait(false);
        },
            eventArgs.CancellationToken).ConfigureAwait(false);
    }

    private async Task OnClanWarUpdatedAsync(object sender, ClanWarUpdatedEventArgs eventArgs)
    {
        if (ClanWarUpdated == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnClanWarUpdatedAsync), async () => 
        {
            await ClanWarUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        },
            eventArgs.CancellationToken).ConfigureAwait(false);
    }
}