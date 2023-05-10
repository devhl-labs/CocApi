using System;
using CocApi.Rest.Models;
using CocApi.Rest.Client;
using System.Threading.Tasks;
using System.Threading;
using CocApi.Rest.IBaseApis;

namespace CocApi.Cache.Context;

public class CachedClan : CachedItem<Clan>
{
    internal static async Task<CachedClan> FromClanResponseAsync(string tag, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken? cancellationToken)
    {
        try
        {
            ApiResponse<Clan> apiResponse = await clansApi.FetchClanAsync(tag, cancellationToken).ConfigureAwait(false);

            return new CachedClan(tag, apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
        }
        catch (Exception e) 
        {
            cancellationToken?.ThrowIfCancellationRequested();

            return new CachedClan(tag, await ttl.TimeToLiveOrDefaultAsync<Clan>(e).ConfigureAwait(false));
        }
    }

    internal static bool HasUpdated(CachedClan stored, CachedClan fetched)
    {
        if (stored.ExpiresAt > fetched.ExpiresAt)
            return false;

        if (fetched.Content == null)
            return false;
        
        return !fetched.Content.Equals(stored.Content);
    }

    private CachedClan(string tag, ApiResponse<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
    {
        Tag = tag;

        if (response.TryToModel(out Clan? model))
            IsWarLogPublic = model.IsWarLogPublic;
    }

    private CachedClan(string tag, TimeSpan localExpiration) : base(localExpiration)
    {
        Tag = tag;
    }

    public void UpdateFrom(CachedClan fetched)
    {
        if (ExpiresAt > fetched.ExpiresAt)
            return;

        IsWarLogPublic = fetched.IsWarLogPublic;

        base.UpdateFrom(fetched);
    }









    public bool DownloadMembers { get; internal set; }

    public bool? IsWarLogPublic { get; internal set; }

    private string _tag;

    public string Tag
    {
        get
        {
            return _tag;
        }

        set { _tag = CocApi.Clash.FormatTag(value); }
    }

    public int Id { get; internal set; }

    public CachedClanWar CurrentWar { get; internal set; } = new();

    public CachedClanWarLog WarLog { get; internal set; } = new();

    public CachedClanWarLeagueGroup Group { get; internal set; } = new();

    public CachedClan(string tag, bool downloadClan, bool downloadWar, bool downloadLog, bool downloadGroup, bool downloadMembers)
    {
        Tag = tag;

        Download = downloadClan;

        CurrentWar.Download = downloadWar;

        WarLog.Download = downloadLog;

        Group.Download = downloadGroup;

        DownloadMembers = downloadMembers;
    }

    internal CachedClan()
    {

    }

}
