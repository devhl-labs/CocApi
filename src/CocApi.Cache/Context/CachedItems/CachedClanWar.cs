﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;
using System.Linq;

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        internal static async Task<CachedClanWar> FromCurrentWarResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            TimeSpan ttl;
            try
            {
                ApiResponse<ClanWar> apiResponse = await clansApi.FetchCurrentWarResponseAsync(tag, cancellationToken);

                ttl = await clansCacheBase.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false);

                CachedClanWar result = new CachedClanWar(tag, apiResponse, ttl);

                return result;
            }
            catch (Exception e)
            {
                ttl = await clansCacheBase.TimeToLiveOrDefaultAsync<ClanWar>(e).ConfigureAwait(false);

                return new CachedClanWar(ttl);
            }
        }

        public bool Added { get; internal set; }

        public string Key { get { return $"{Content.PreparationStartTime};{Content.Clan.Tag};{Content.Opponent.Tag}"; } }

        internal static bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
        {
            if (fetched.Content == null || fetched.Content.State == WarState.NotInWar)
                return false;

            if (stored.Content == null)
                return true;

            return stored.Content.PreparationStartTime != fetched.Content.PreparationStartTime;
        }

        private string? _enemyTag;

        public string? EnemyTag { get { return _enemyTag; } internal set { _enemyTag = value == null ? null : Clash.FormatTag(value); } }

        public WarState? State { get; internal set; }

        public DateTime? PreparationStartTime { get; internal set; }

        public WarType? Type { get; internal set; }

        private CachedClanWar(string tag, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            if (apiResponse.Content != null && apiResponse.Content.State != WarState.NotInWar)
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
}
