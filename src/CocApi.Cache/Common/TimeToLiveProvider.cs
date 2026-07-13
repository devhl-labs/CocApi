using System;
using System.Net;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using CocApi.Cache.Logging;
using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache;

public class TimeToLiveProvider
{
    protected ILogger<TimeToLiveProvider> Logger { get; }

    public TimeToLiveProvider(ILogger<TimeToLiveProvider> logger)
    {
        Logger = logger;
    }

    internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync<T>(IOk<T> apiResponse) where T : class
    {
        try
        {
            TimeSpan result = await TimeToLiveAsync(apiResponse).ConfigureAwait(false);

            return result < TimeSpan.Zero
                ? TimeSpan.Zero
                : result;
        }
        catch (Exception e)
        {
            Logger.LogError(CacheLogEvents.TimeToLiveComputationFailed, e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(TimeToLiveAsync));

            return TimeSpan.FromMinutes(2);
        }
    }

    internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync<T>(Exception exception) where T : class
    {
        try
        {
            TimeSpan result = await TimeToLiveAsync<T>(exception).ConfigureAwait(false);

            return result < TimeSpan.Zero
                ? TimeSpan.Zero
                : result;
        }
        catch (Exception e)
        {
            Logger.LogError(CacheLogEvents.TimeToLiveComputationFailed, e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(TimeToLiveAsync));

            return TimeSpan.FromMinutes(2);
        }
    }

    private ValueTask<TimeSpan> ClanWarLeagueGroupTimeToLive() => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(60));

    protected virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception) where T : class =>
        typeof(T) == typeof(ClanWarLeagueGroup)
            ? ClanWarLeagueGroupTimeToLive()
            : new ValueTask<TimeSpan>(TimeSpan.FromSeconds(2));

    protected virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(IOk<T> apiResponse) where T : class
    {
        if (!apiResponse.IsSuccessStatusCode)
            return typeof(T) == typeof(ClanWarLeagueGroup)
                ? ClanWarLeagueGroupTimeToLive()
                :new ValueTask<TimeSpan>(TimeSpan.FromSeconds(2));

        if (typeof(T) == typeof(Clan))
            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        if (typeof(T) == typeof(ClanWarLog))
            return apiResponse.StatusCode == HttpStatusCode.Forbidden
                ? new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2))
                : new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        if (apiResponse is IOk<ClanWarLeagueGroup> group)
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(60));

        if (apiResponse is IOk<ClanWar> war)
        {
            ClanWar? model = war.Ok();

            if (war.StatusCode == HttpStatusCode.Forbidden)
                return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

            if (model?.State == Rest.Models.WarState.Preparation)
                return new ValueTask<TimeSpan>(model.StartTime.AddHours(-1) - DateTime.UtcNow);
        }

        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
    }

    protected virtual ValueTask<TimeSpan> NoChangeTimeToLiveAsync<T>(CachedItem<T> fetched, DateTime? lastChangedAt) where T : class
    {
        if (typeof(T) == typeof(Clan))
            return new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _clanTiers));

        if (typeof(T) == typeof(ClanWarLog))
            return new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _warLogTiers));

        if (typeof(T) == typeof(Player))
            return new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _playerTiers));

        if (typeof(T) == typeof(ClanWarLeagueGroup))
            return new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _groupTiers));

        if (typeof(T) == typeof(ClanWar))
        {
            ClanWar? war = (ClanWar?)(object?)fetched.Content;

            if (war?.EndTime < DateTime.UtcNow.AddHours(-24))
                return new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _clanWarTiers));

            return war?.State switch
            {
                WarState.InWar       => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2)),
                WarState.Preparation => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(10)),
                _                    => new ValueTask<TimeSpan>(TieredTtl(lastChangedAt, _clanWarTiers)),
            };
        }

        Logger.LogWarning("{typeName}.{methodName}() has no tier configured for type {cachedType}. Override this method to add support.", GetType().Name, nameof(NoChangeTimeToLiveAsync), typeof(T).Name);
        return new ValueTask<TimeSpan>(TimeSpan.Zero);
    }

    // stableFor = UtcNow - DownloadedAt (last content change). First matching threshold wins.
    private static readonly (TimeSpan StableFor, TimeSpan Ttl)[] _clanTiers =
    [
        (TimeSpan.FromHours(1),  TimeSpan.FromMinutes(2)),
        (TimeSpan.FromHours(4),  TimeSpan.FromMinutes(4)),
        (TimeSpan.FromHours(24), TimeSpan.FromMinutes(10)),
        (TimeSpan.FromHours(72), TimeSpan.FromHours(1)),
        (TimeSpan.FromDays(30),  TimeSpan.FromHours(12)),
        (TimeSpan.MaxValue,      TimeSpan.FromHours(24)),
    ];

    private static readonly (TimeSpan StableFor, TimeSpan Ttl)[] _warLogTiers =
    [
        (TimeSpan.FromHours(1),  TimeSpan.FromMinutes(2)),
        (TimeSpan.FromHours(4),  TimeSpan.FromMinutes(4)),
        (TimeSpan.FromHours(24), TimeSpan.FromMinutes(10)),
        (TimeSpan.FromHours(72), TimeSpan.FromHours(1)),
        (TimeSpan.FromDays(30),  TimeSpan.FromHours(12)),
        (TimeSpan.MaxValue,      TimeSpan.FromHours(24)),
    ];

    private static readonly (TimeSpan StableFor, TimeSpan Ttl)[] _playerTiers =
    [
        (TimeSpan.FromHours(1),  TimeSpan.FromMinutes(2)),
        (TimeSpan.FromHours(4),  TimeSpan.FromMinutes(4)),
        (TimeSpan.FromHours(24), TimeSpan.FromMinutes(10)),
        (TimeSpan.FromHours(72), TimeSpan.FromHours(1)),
        (TimeSpan.FromDays(30),  TimeSpan.FromHours(12)),
        (TimeSpan.MaxValue,      TimeSpan.FromHours(24)),
    ];

    private static readonly (TimeSpan StableFor, TimeSpan Ttl)[] _groupTiers =
    [
        (TimeSpan.FromHours(24), TimeSpan.FromMinutes(20)),
        (TimeSpan.FromDays(7),   TimeSpan.FromHours(4)),
        (TimeSpan.FromDays(30),  TimeSpan.FromHours(12)),
        (TimeSpan.MaxValue,      TimeSpan.FromHours(24)),
    ];

    private static readonly (TimeSpan StableFor, TimeSpan Ttl)[] _clanWarTiers =
    [
        (TimeSpan.FromHours(72), TimeSpan.FromHours(1)),
        (TimeSpan.FromDays(30),  TimeSpan.FromHours(2)),
        (TimeSpan.FromDays(90),  TimeSpan.FromHours(12)),
        (TimeSpan.FromDays(120), TimeSpan.FromHours(24)),
        (TimeSpan.MaxValue,      TimeSpan.FromHours(48)),
    ];

    private static TimeSpan TieredTtl(DateTime? downloadedAt, (TimeSpan StableFor, TimeSpan Ttl)[] tiers)
    {
        TimeSpan stableFor = DateTime.UtcNow - (downloadedAt ?? DateTime.UtcNow);
        foreach (var (threshold, ttl) in tiers)
            if (stableFor < threshold)
                return ttl;
        return tiers[^1].Ttl;
    }

    internal async ValueTask<TimeSpan> NoChangeTimeToLiveOrDefaultAsync<T>(CachedItem<T> fetched, DateTime? lastChangedAt) where T : class
    {
        try
        {
            TimeSpan result = await NoChangeTimeToLiveAsync<T>(fetched, lastChangedAt).ConfigureAwait(false);
            if (result < TimeSpan.Zero) return TimeSpan.Zero;
            if (result.Milliseconds != 0 || result.Microseconds != 0)
                Logger.LogWarning("{typeName}.{methodName}() should return a fixed duration (e.g. TimeSpan.FromMinutes(5)), not a value computed per-object (e.g. someDate - DateTime.UtcNow).", GetType().Name, nameof(NoChangeTimeToLiveAsync));
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(CacheLogEvents.TimeToLiveComputationFailed, e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(NoChangeTimeToLiveAsync));
            return TimeSpan.FromMinutes(2);
        }
    }
}
