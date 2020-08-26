//using Dapper.SqlWriter;
//using CocApi.Cache.Exceptions;
//using CocApi.Cache.Models;
//using CocApi.Cache.Models.Cache;
//using CocApi.Cache.Models.Clans;
//using CocApi.Cache.Models.Villages;
//using Microsoft.SqlServer.Server;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Security.Policy;
//using System.Threading;
//using System.Threading.Tasks;

//namespace CocApi.Cache
//{
//    public sealed class ClanCache
//    {
//        private CocApiClient_old CocApi { get; }

//        internal ClanCache(CocApiClient_old cocApi)
//        {
//            CocApi = cocApi;
//        }

//        public event AsyncEventHandler<ChangedEventArgs<Clan>>? ClanBadgeUrlChanged;

//        public event AsyncEventHandler<ChangedEventArgs<Clan>>? ClanChanged;

//        public event AsyncEventHandler<DonationEventArgs>? ClanDonation;

//        public event AsyncEventHandler<LabelsChangedEventArgs<Clan>>? ClanLabelsChanged;

//        public event AsyncEventHandler<ChildChangedEventArgs<Clan, ClanVillage>>? ClanVillageChanged;

//        public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesJoined;

//        public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesLeft;



//        //public async Task<Paginated<Clan>?> GetAsync(ClanQueryOptions clanQueryOptions, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
//        //    => await CocApi.GetOrFetchAsync<Paginated<Clan>>(Clan.Url(clanQueryOptions), cacheOption, cancellationToken);

//        //public async Task<Paginated<TopBuilderClan>?> GetTopBuilderClansAsync(int? locationId = null, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
//        //    => await CocApi.GetOrFetchAsync<Paginated<TopBuilderClan>>(TopBuilderClan.Url(locationId), cacheOption, cancellationToken).ConfigureAwait(false);

//        //public async Task<Paginated<TopMainClan>?> GetTopMainClansAsync(int? locationId = null, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
//        //    => await CocApi.GetOrFetchAsync<Paginated<TopMainClan>>(TopMainClan.Url(locationId), cacheOption, cancellationToken).ConfigureAwait(false);

//        //public async Task<Clan?> GetAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
//        //    => await CocApi.GetOrFetchAsync<Clan>(Clan.Url(clanTag), cacheOption, cancellationToken).ConfigureAwait(false);



//        internal void OnBadgeUrlChanged(Clan fetched, Clan queued) => ClanBadgeUrlChanged?.Invoke(this, new ChangedEventArgs<Clan>(fetched, queued));

//        internal void OnClanChanged(Clan fetched, Clan queued) => ClanChanged?.Invoke(this, new ChangedEventArgs<Clan>(fetched, queued));

//        internal void OnClanVillageChanged(Clan fetched, ClanVillage fetchedVillage, ClanVillage queuedVillage) => ClanVillageChanged?.Invoke(this, new ChildChangedEventArgs<Clan, ClanVillage>(fetched, fetchedVillage, queuedVillage));

//        internal void OnClanVillagesJoined(Clan fetched, List<ClanVillage> fetchedClanVillages)
//        {
//            if (fetchedClanVillages.Count > 0)
//            {
//                ClanVillagesJoined?.Invoke(this, new ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
//            }
//        }

//        internal void OnClanVillagesLeft(Clan fetched, List<ClanVillage> fetchedClanVillages)
//        {
//            if (fetchedClanVillages.Count > 0)
//            {
//                ClanVillagesLeft?.Invoke(this, new ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
//            }
//        }

//        internal void OnDonation(Clan fetched, List<Donation> received, List<Donation> donated)
//        {
//            if (received.Count > 0 || donated.Count > 0)
//            {
//                ClanDonation?.Invoke(this, new DonationEventArgs(fetched, received.ToImmutableArray(), donated.ToImmutableArray()));
//            }
//        }

//        internal void OnLabelsChanged(Clan fetched, List<Label> added, List<Label> removed)
//        {
//            if (added.Count == 0 && removed.Count == 0)
//                return;

//            ClanLabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Clan>(fetched, added.ToImmutableArray(), removed.ToImmutableArray()));
//        }


//        public Func<Clan, Clan, bool> IsChanged { get; set; } = new Func<Clan, Clan, bool>((fetched, stored) =>
//        {
//            if (stored.ClanLevel != fetched.ClanLevel ||
//                stored.Description != fetched.Description ||
//                stored.IsWarLogPublic != fetched.IsWarLogPublic ||
//                stored.VillageCount != fetched.VillageCount ||
//                stored.Name != fetched.Name ||
//                stored.RequiredTrophies != fetched.RequiredTrophies ||
//                stored.Recruitment != fetched.Recruitment ||
//                stored.WarFrequency != fetched.WarFrequency ||
//                stored.WarLosses != fetched.WarLosses ||
//                stored.WarTies != fetched.WarTies ||
//                stored.WarWins != fetched.WarWins ||
//                stored.WarWinStreak != fetched.WarWinStreak ||
//                stored.ClanPoints != fetched.ClanPoints ||
//                stored.ClanVersusPoints != fetched.ClanVersusPoints ||
//                stored.Location?.Id != fetched.Location?.Id ||
//                stored.WarLeague?.Id != fetched.WarLeague?.Id
//)
//            {
//                return true;
//            }

//            return false;
//        });

//        public bool DownloadClanVillages { get; set; }
//    }
//}