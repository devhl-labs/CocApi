﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Context;

public class CachedClanWarLog : CachedItem<ClanWarLog>
{
    internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string tag, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken cancellationToken = default)
    {
        try
        {
            IFetchClanWarLogApiResponse apiResponse = await clansApi.FetchClanWarLogAsync(tag, cancellationToken: cancellationToken);

            return new CachedClanWarLog(apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new CachedClanWarLog(await ttl.TimeToLiveOrDefaultAsync<ClanWarLog>(e).ConfigureAwait(false));
        }
    }

    internal static bool HasUpdated(CachedClanWarLog stored, CachedClanWarLog fetched)
    {
        if (stored.ExpiresAt > fetched.ExpiresAt)
            return false;

        if (fetched.Content == null)
            return false;

        return !fetched.Content.Equals(stored.Content);
    }

    public CachedClanWarLog()
    {

    }

    private CachedClanWarLog(IFetchClanWarLogApiResponse apiResponse, TimeSpan localExpiration)
    {
        UpdateFrom(apiResponse, localExpiration);
    }

    private CachedClanWarLog(TimeSpan localExpiration)
    {
        UpdateFrom(localExpiration);
    }

    internal void UpdateFrom(CachedClanWarLog fetched)
    {
        if (ExpiresAt > fetched.ExpiresAt)
            return;

        base.UpdateFrom(fetched);
    }
}
