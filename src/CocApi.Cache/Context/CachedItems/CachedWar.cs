using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Context;

public class CachedWar : CachedItem<ClanWar>
{
    internal static async Task<CachedWar> FromClanWarLeagueWarResponseAsync(
        string warTag, DateTime season, bool? realtime, TimeToLiveProvider ttl,
        IClansApi clansApi, CancellationToken cancellationToken = default)
    {
        try
        {
            ApiResponse<ClanWar> apiResponse = await clansApi.FetchClanWarLeagueWarAsync(warTag, realtime, cancellationToken).ConfigureAwait(false);

            TimeSpan timeToLive = await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false);

            if (!apiResponse.IsSuccessStatusCode || !apiResponse.TryToModel(out ClanWar? model) || model.State == Rest.Models.WarState.NotInWar)
                return new CachedWar(warTag, timeToLive);

            CachedWar result = new(apiResponse, timeToLive, warTag, season)
            {
                Season = season
            };

            return result;
        }
        catch (Exception e)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new CachedWar(warTag, await ttl.TimeToLiveOrDefaultAsync<ClanWar>(e).ConfigureAwait(false));
        }
    }

    internal static bool HasUpdated(CachedWar stored, CachedWar fetched)
    {
        if (ReferenceEquals(stored, fetched))
            return false;

        if (stored.ExpiresAt > fetched.ExpiresAt)
            return false;

        if (stored.Content == null)
            throw new InvalidOperationException($"{nameof(stored)}.Data is null");

        if (fetched.Content == null)
            throw new InvalidOperationException($"{nameof(fetched)}.Data is null");

        if (!ClanWar.IsSameWar(stored.Content, fetched.Content))
            throw new InvalidOperationException("Provided wars are the same war.");

        return !fetched.Content.Equals(stored.Content);
    }

    internal static bool HasUpdated(CachedWar stored, CachedClanWar fetched)
    {
        if (stored.ExpiresAt > fetched.ExpiresAt)
            return false;

        if (stored.Content == null)
            throw new InvalidOperationException($"{nameof(stored)}.Data is null");

        if (fetched.Content == null)
            throw new InvalidOperationException($"{nameof(fetched)}.Data is null");

        return !fetched.Content.Equals(stored.Content);
    }

    public string Key { get { return $"{Content.PreparationStartTime};{Content.Clan.Tag};{Content.Opponent.Tag}"; } }

    public int Id { get; internal set; }

    private string _clanTag;

    public string ClanTag { get { return _clanTag; } internal set { _clanTag = CocApi.Clash.FormatTag(value); } }

    private string _opponentTag;

    public string OpponentTag { get { return _opponentTag; } internal set { _opponentTag = CocApi.Clash.FormatTag(value); } }

    public DateTime PreparationStartTime { get; internal set; }

    public DateTime EndTime { get; internal set; }

    private string? _warTag;

    public string? WarTag { get { return _warTag; } internal set { _warTag = value == null ? null : CocApi.Clash.FormatTag(value); } }

    public Rest.Models.WarState? State { get; internal set; }

    public bool IsFinal { get; internal set; }

    public DateTime? Season { get; internal set; } // can be private set after importing the old data

    public Announcements Announcements { get; internal set; }

    public Rest.Models.WarType Type { get; internal set; }

    private volatile SortedSet<string>? _clanTags;

    private readonly object _clanTagsLock = new();

    public SortedSet<string> ClanTags
    {
        get
        {
            if (_clanTags != null) // avoid the lock if we can
                return _clanTags;

            lock (_clanTagsLock)
            {
                if (_clanTags != null)
                    return _clanTags;

                _clanTags = new SortedSet<string>
                {
                    ClanTag,
                    OpponentTag
                };

                return _clanTags;
            }
        }
    }

    public CachedWar(CachedClanWar cachedClanWar)
    {
        if (cachedClanWar.Content == null)
            throw new InvalidOperationException("Data should not be null");

        ClanTag = cachedClanWar.Content.Clans.First().Value.Tag;

        OpponentTag = cachedClanWar.Content.Clans.Skip(1).First().Value.Tag;

        State = cachedClanWar.Content.State;

        PreparationStartTime = cachedClanWar.Content.PreparationStartTime;

        EndTime = cachedClanWar.Content.EndTime;

        Type = cachedClanWar.Type.Value;

        UpdateFrom(cachedClanWar);
    }

    internal CachedWar(ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration, string warTag, DateTime season)
    {
        base.UpdateFrom(apiResponse, localExpiration);

        ClanWar model = apiResponse.AsModel();

        ClanTag = model.Clans.First().Value.Tag;

        OpponentTag = model.Clans.Skip(1).First().Value.Tag;

        State = model.State;

        PreparationStartTime = model.PreparationStartTime;

        EndTime = model.EndTime;

        Type = model.GetWarType();

        //if (State == WarState.WarEnded)
        //    IsFinal = true;

        StatusCode = apiResponse.StatusCode;

        WarTag = warTag;

        Season = season;
    }

    public CachedWar()
    {

    }

    private CachedWar(string warTag, TimeSpan localExpiration)
    {
        WarTag = warTag;

        UpdateFrom(localExpiration);
    }

    private void ThrowIfNotTheSameWar(ClanWar? clanWar)
    {
        if (ClanWar.IsSameWar(Content, clanWar) == false)
            throw new InvalidOperationException("The fetched war must be the same war.");
    }

    internal void UpdateFrom(CachedClanWar fetched)
    {
        ThrowIfNotTheSameWar(fetched.Content);

        if (ExpiresAt > fetched.ExpiresAt)
            return;

        RawContent = fetched.RawContent;

        DownloadedAt = fetched.DownloadedAt;

        ExpiresAt = fetched.ExpiresAt;

        KeepUntil = fetched.KeepUntil;

        StatusCode = fetched.StatusCode;

        if (fetched.Content != null)
        {
            Content = fetched.Content;
            
            State = fetched.Content.State;

            EndTime = fetched.Content.EndTime;

            //if (fetched.Data.State == WarState.WarEnded)
            //    IsFinal = true;
        }
    }
    
    internal void UpdateFrom(CachedWar cachedWar)
    {
        ThrowIfNotTheSameWar(cachedWar.Content);

        base.UpdateFrom(cachedWar);

        if (cachedWar.Content != null)
            State = cachedWar.Content.State;

        //if (State == WarState.WarEnded)
        //    IsFinal = true;

        StatusCode = cachedWar.StatusCode;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PreparationStartTime);
        hash.Add(ClanTags.First());
        hash.Add(ClanTags.Skip(1).First());
        return hash.ToHashCode();
    }
}
