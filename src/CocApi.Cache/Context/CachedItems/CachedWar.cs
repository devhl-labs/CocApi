﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedWar : CachedItem<ClanWar>
    {
        internal static async Task<CachedWar> FromClanWarLeagueWarResponseAsync(
            string warTag, DateTime season, ClansClientBase clansCacheBase, 
            ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWar> apiResponse = await clansApi.FetchClanWarLeagueWarResponseAsync(warTag, cancellationToken).ConfigureAwait(false);

                TimeSpan timeToLive = await clansCacheBase.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false);

                if (!apiResponse.IsSuccessStatusCode || apiResponse.Content?.State == WarState.NotInWar)
                    return new CachedWar(warTag, timeToLive);

                CachedWar result = new CachedWar(apiResponse, timeToLive, warTag, season)
                {
                    Season = season
                };

                return result;
            }
            catch (Exception e)
            {
                return new CachedWar(warTag, await clansCacheBase.TimeToLiveOrDefaultAsync<ClanWar>(e).ConfigureAwait(false));
            }
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

        public WarState? State { get; internal set; }

        public bool IsFinal { get; internal set; }

        public DateTime? Season { get; internal set; } // can be private set after importing the old data

        public Announcements Announcements { get; internal set; }

        public WarType Type { get; internal set; }

        private readonly SortedSet<string> _clanTags = new();

        private readonly object _clanTagsLock = new();

        public SortedSet<string> ClanTags
        {
            get
            {
                lock (_clanTagsLock)
                {
                    if (_clanTags.Count == 2)
                        return _clanTags;

                    _clanTags.Add(ClanTag);
                    _clanTags.Add(OpponentTag);

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

        private CachedWar(ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration, string warTag, DateTime season)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            ClanTag = apiResponse.Content.Clans.First().Value.Tag;

            OpponentTag = apiResponse.Content.Clans.Skip(1).First().Value.Tag;

            State = apiResponse.Content.State;

            PreparationStartTime = apiResponse.Content.PreparationStartTime;

            EndTime = apiResponse.Content.EndTime;

            Type = apiResponse.Content.GetWarType();

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
            HashCode hash = new HashCode();
            hash.Add(PreparationStartTime);
            hash.Add(ClanTags.First());
            hash.Add(ClanTags.Skip(1).First());
            return hash.ToHashCode();
        }
    }
}