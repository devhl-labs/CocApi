using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Context;

public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
{
    internal static async Task<CachedClanWarLeagueGroup> FromClanWarLeagueGroupResponseAsync(string tag, bool? realtime, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken cancellationToken = default)
    {
        try
        {
            ApiResponse<ClanWarLeagueGroup> apiResponse = await clansApi.FetchClanWarLeagueGroupAsync(tag, realtime, cancellationToken).ConfigureAwait(false);

            return new CachedClanWarLeagueGroup(apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new CachedClanWarLeagueGroup(await ttl.TimeToLiveOrDefaultAsync<ClanWarLeagueGroup>(e).ConfigureAwait(false));
        }
    }

    internal static bool HasUpdated(CachedClanWarLeagueGroup stored, CachedClanWarLeagueGroup fetched)
    {
        if (stored.ExpiresAt > fetched.ExpiresAt)
            return false;

        if (fetched.Content == null)
            return false;

        return !fetched.Content.Equals(stored.Content);
    }

    public DateTime? Season { get; internal set; }

    public Rest.Models.GroupState? State { get; internal set; }

    public bool Added { get; internal set; }

    internal CachedClanWarLeagueGroup()
    {
    }

    private CachedClanWarLeagueGroup(ApiResponse<ClanWarLeagueGroup> apiResponse, TimeSpan localExpiration)
    {
        UpdateFrom(apiResponse, localExpiration);

        ClanWarLeagueGroup? model = apiResponse.AsModel();

        Season = model?.Season;

        State = model?.State;
    }

    private CachedClanWarLeagueGroup(TimeSpan localExpiration)
    {
        UpdateFrom(localExpiration);
    }

    internal void UpdateFrom(CachedClanWarLeagueGroup fetched)
    {
        if (ExpiresAt > fetched.ExpiresAt)
            return;

        base.UpdateFrom(fetched);

        if (fetched.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Season = fetched.Season;

            State = fetched.State;
        }
    }
}
