using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Client;
using CocApi.Rest.Models;
using System.Linq;
using CocApi.Rest.IBaseApis;

namespace CocApi.Cache.Context;

public class CachedClanWar : CachedItem<ClanWar>
{
    internal static async Task<CachedClanWar> FromCurrentWarResponseAsync(string tag, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken? cancellationToken = default)
    {
        try
        {
            ApiResponse<ClanWar> apiResponse = await clansApi.FetchCurrentWarResponseAsync(tag, cancellationToken);

            return new CachedClanWar(tag, apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            return new CachedClanWar(await ttl.TimeToLiveOrDefaultAsync<ClanWar>(e).ConfigureAwait(false));
        }
    }

    public bool Added { get; internal set; }

    public string Key { get { return $"{Content.PreparationStartTime};{Content.Clan.Tag};{Content.Opponent.Tag}"; } }

    internal static bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
    {
        if (fetched.Content == null || fetched.Content.State == Rest.Models.WarState.NotInWar)
            return false;

        if (stored.Content == null)
            return true;

        return stored.Content.PreparationStartTime != fetched.Content.PreparationStartTime;
    }

    private string? _enemyTag;

    public string? EnemyTag { get { return _enemyTag; } internal set { _enemyTag = value == null ? null : Clash.FormatTag(value); } }

    public Rest.Models.WarState? State { get; internal set; }

    public DateTime? PreparationStartTime { get; internal set; }

    public Rest.Models.WarType? Type { get; internal set; }

    public int CachedClanId { get; internal set; }

    private CachedClanWar(string tag, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
    {
        base.UpdateFrom(apiResponse, localExpiration);

        if (apiResponse.Content != null && apiResponse.Content.State != Rest.Models.WarState.NotInWar)
        {
            EnemyTag = apiResponse.Content.Clans.Keys.FirstOrDefault(k => k != tag);

            State = apiResponse.Content.State;

            PreparationStartTime = apiResponse.Content.PreparationStartTime;
        }
    }

    private CachedClanWar(TimeSpan localExpiration)
    {
        base.UpdateFrom(localExpiration);
    }

    internal void UpdateFrom(CachedClanWar fetched)
    {
        if (ExpiresAt > fetched.ExpiresAt)
            return;

        base.UpdateFrom(fetched);

        State = fetched.State;

        PreparationStartTime = fetched.PreparationStartTime;

        EnemyTag = fetched.EnemyTag;

        //Type = fetched.Type;  //only do this at the beginning of war
    }

    public CachedClanWar()
    {

    }
}
