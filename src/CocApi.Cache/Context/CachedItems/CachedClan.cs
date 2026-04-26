using System;
using CocApi.Rest.Models;
using CocApi.Rest.Client;
using System.Threading.Tasks;
using System.Threading;
using CocApi.Rest.Apis;
using System.Linq;

namespace CocApi.Cache.Context;

public class CachedClan : CachedItem<Clan>
{
    internal static async Task<CachedClan> FromClanResponseAsync(string tag, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken cancellationToken)
    {
        try
        {
            IOk<Clan?> apiResponse = await clansApi.FetchClanAsync(tag, cancellationToken).ConfigureAwait(false);

            return new CachedClan(tag, apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
        }
        catch (Exception e) 
        {
            cancellationToken.ThrowIfCancellationRequested();

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

    internal enum ClanActivityLevel { Active, Inactive, Dead }

    internal static ClanActivityLevel GetActivityLevel(CachedClan clan)
    {
        var mostRecent = new[]
        {
            clan.DownloadedAt,
            clan.WarLog.DownloadedAt,
            clan.Group.DownloadedAt,
            clan.CurrentWar.DownloadedAt
        }
        .Where(d => d.HasValue)
        .Select(d => d!.Value)
        .DefaultIfEmpty(DateTime.UtcNow)
        .Max();

        var age = DateTime.UtcNow - mostRecent;

        // Accelerated detection window for first run
        var cutoff = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc);
        if (DateTime.UtcNow < cutoff)
        {
            if (age < TimeSpan.FromHours(2)) return ClanActivityLevel.Active;
            if (age < TimeSpan.FromHours(8)) return ClanActivityLevel.Inactive;
            return ClanActivityLevel.Dead;
        }

        if (age < TimeSpan.FromHours(24)) return ClanActivityLevel.Active;
        if (age < TimeSpan.FromDays(7)) return ClanActivityLevel.Inactive;
        return ClanActivityLevel.Dead;
    }

    private CachedClan(string tag, IOk<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
    {
        Tag = tag;

        if (response.TryOk(out Clan? model))
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
