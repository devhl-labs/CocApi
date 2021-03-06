﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
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

                TimeSpan timeToLive = await clansCacheBase.ClanWarTimeToLiveAsync(apiResponse).ConfigureAwait(false);

                if (!apiResponse.IsSuccessStatusCode || apiResponse.Data?.State == WarState.NotInWar)
                    return new CachedWar(warTag, timeToLive);

                CachedWar result = new CachedWar(apiResponse, timeToLive, warTag, season)
                {
                    Season = season
                };

                return result;
            }
            catch (Exception e)
            {
                return new CachedWar(warTag, await clansCacheBase.ClanWarTimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        public string ClanTag { get; internal set; }

        public string OpponentTag { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public DateTime EndTime { get; internal set; }

        public string? WarTag { get; internal set; }

        public WarState? State { get; internal set; }

        public bool IsFinal { get; internal set; }

        public DateTime? Season { get; private set; }

        public Announcements Announcements { get; internal set; }

        public WarType Type { get; internal set; }

        private readonly SortedSet<string> _clanTags = new();

        public SortedSet<string> ClanTags
        {
            get
            {
                if (_clanTags.Count != 0)
                    return _clanTags;

                _clanTags.Add(ClanTag);
                _clanTags.Add(OpponentTag);

                return _clanTags;
            }
        }

        public CachedWar(CachedClanWar cachedClanWar)
        {
            if (cachedClanWar.Data == null)
                throw new ArgumentException("Data should not be null");

            ClanTag = cachedClanWar.Data.Clans.First().Value.Tag;

            OpponentTag = cachedClanWar.Data.Clans.Skip(1).First().Value.Tag;

            State = cachedClanWar.Data.State;

            PreparationStartTime = cachedClanWar.Data.PreparationStartTime;

            EndTime = cachedClanWar.Data.EndTime;

            Type = cachedClanWar.Type;

            UpdateFrom(cachedClanWar);
        }

        private CachedWar(ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration, string warTag, DateTime season)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            ClanTag = apiResponse.Data.Clans.First().Value.Tag;

            OpponentTag = apiResponse.Data.Clans.Skip(1).First().Value.Tag;

            State = apiResponse.Data.State;

            PreparationStartTime = apiResponse.Data.PreparationStartTime;

            EndTime = apiResponse.Data.EndTime;

            //Type = apiResponse.Data.WarType;

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
            if (ClanWar.IsSameWar(Data, clanWar) == false)
                throw new Exception("The fetched war must be the same war.");
        }

        internal void UpdateFrom(CachedClanWar fetched)
        {
            ThrowIfNotTheSameWar(fetched.Data);

            if (ServerExpiration > fetched.ServerExpiration)
                return;

            RawContent = fetched.RawContent;

            Downloaded = fetched.Downloaded;

            ServerExpiration = fetched.ServerExpiration;

            LocalExpiration = fetched.LocalExpiration;

            StatusCode = fetched.StatusCode;

            if (fetched.Data != null)
            {
                Data = fetched.Data;
                
                State = fetched.Data.State;

                EndTime = fetched.Data.EndTime;

                //if (fetched.Data.State == WarState.WarEnded)
                //    IsFinal = true;
            }
        }
        
        internal void UpdateFrom(CachedWar cachedWar)
        {
            ThrowIfNotTheSameWar(cachedWar.Data);

            base.UpdateFrom(cachedWar);

            if (cachedWar.Data != null)
                State = cachedWar.Data.State;

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
