using System;
using System.Net;
using System.Threading.Tasks;
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

    internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync<T>(ApiResponse<T?> apiResponse) where T : class
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
            Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(TimeToLiveAsync));

            return TimeSpan.FromMinutes(0);
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
            Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(TimeToLiveAsync));

            return TimeSpan.FromMinutes(0);
        }
    }

    protected virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception) where T : class
    {
        if (typeof(T) == typeof(ClanWarLeagueGroup))
        {
            if (Clash.IsCwlEnabled)
                return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(20));
            else
                return new ValueTask<TimeSpan>(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                        .AddMonths(1)
                        .Subtract(DateTime.UtcNow));
        }

        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
    }

    protected virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(ApiResponse<T?> apiResponse) where T : class
    {
        if (apiResponse is ApiResponse<Clan>)
            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        if (apiResponse is ApiResponse<ClanWarLog>)
            return apiResponse.StatusCode == HttpStatusCode.Forbidden
                ? new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2))
                : new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        if (apiResponse is ApiResponse<ClanWarLeagueGroup> group)
            if (!Clash.IsCwlEnabled ||
                (group.Content?.State == Rest.Models.GroupState.Ended && DateTime.UtcNow.Month == group.Content.Season.Month) ||
                (group.Content == null && DateTime.UtcNow.Day >= 3))
                return new ValueTask<TimeSpan>(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                        .AddMonths(1)
                        .Subtract(DateTime.UtcNow));

        if (apiResponse is ApiResponse<ClanWar> war)
        {
            if (war.StatusCode == HttpStatusCode.Forbidden)
                return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

            if (war.Content?.State == Rest.Models.WarState.Preparation)
                return new ValueTask<TimeSpan>(war.Content.StartTime.AddHours(-1) - DateTime.UtcNow);
        }

        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
    }
}
