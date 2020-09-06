using System;
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
        internal static async Task<CachedWar> FromClanWarLeagueWarResponseAsync(string warTag, DateTime season, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWar> apiResponse = await clansApi.GetClanWarLeagueWarResponseAsync(warTag, cancellationToken);

                CachedWar result = new CachedWar(apiResponse, clansCacheBase.ClanWarTimeToLive(apiResponse), warTag, season);

                result.Type = result.Data.Type;

                result.Season = season;
                
                return result;
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException)
            {
                return new CachedWar(warTag, e, clansCacheBase.ClanWarTimeToLive(e));
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

        public HttpStatusCode? StatusCodeOpponent { get; internal set; }

        public Announcements Announcements { get; internal set; }

        public WarType Type { get; internal set; }

#nullable disable

        private SortedSet<string> _clanTags;

#nullable enable

        public SortedSet<string> ClanTags
        {
            get
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

        public CachedWar(CachedClan cachedClan, CachedClanWar fetched, string? warTag = null)
        {
            if (fetched.Data == null)
                throw new ArgumentException("Data should not be null.");

            ClanTag = fetched.Data.Clans.First().Value.Tag;

            OpponentTag = fetched.Data.Clans.Skip(1).First().Value.Tag;

            PreparationStartTime = fetched.Data.PreparationStartTime;

            EndTime = fetched.Data.EndTime;

            State = fetched.Data.State;

            WarTag = warTag;

            RawContent = fetched.RawContent;

            Type = fetched.Data.Type;

            if (cachedClan.Tag == fetched.Data.Clans.First().Value.Tag)
                StatusCode = fetched.StatusCode;
            else
                StatusCodeOpponent = fetched.StatusCode;

            UpdateFrom(fetched);
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

            Type = apiResponse.Data.Type;

            if (State == WarState.WarEnded)
                IsFinal = true;

            StatusCode = apiResponse.StatusCode;

            WarTag = warTag;

            Season = season;
        }

        public CachedWar()
        {

        }

        private CachedWar(string warTag, Exception exception, TimeSpan localExpiration)
        {
            WarTag = warTag;

            UpdateFrom(exception, localExpiration);
        }

        internal void UpdateFrom(CachedClanWar fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            RawContent = fetched.RawContent;

            Downloaded = fetched.Downloaded;

            ServerExpiration = fetched.ServerExpiration;

            LocalExpiration = fetched.LocalExpiration;

            if (fetched.Data != null)
            {
                Data = fetched.Data;
                
                State = fetched.Data.State;

                EndTime = fetched.Data.EndTime;

                if (fetched.Data.State == WarState.WarEnded)
                    IsFinal = true;

                if (fetched.Data.Clans.First().Key == ClanTag)
                    StatusCode = fetched.StatusCode;
                else
                    StatusCodeOpponent = fetched.StatusCode;
            }
        }
        
        internal void UpdateFrom(CachedWar cachedWar)
        {
            base.UpdateFrom(cachedWar);

            if (cachedWar.Data != null)
                State = cachedWar.Data.State;

            if (State == WarState.WarEnded)
                IsFinal = true;

            StatusCode = cachedWar.StatusCode;

            StatusCodeOpponent = cachedWar.StatusCodeOpponent;
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
