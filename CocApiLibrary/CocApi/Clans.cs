using Dapper.SqlWriter;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Clans
    {
        private CocApi CocApi { get; }

        internal Clans(CocApi cocApi)
        {
            CocApi = cocApi;
        }

        public event AsyncEventHandler<ChangedEventArgs<Clan, Clan>>? ClanBadgeUrlChanged;

        public event AsyncEventHandler<ChangedEventArgs<Clan, Clan>>? ClanChanged;

        public event AsyncEventHandler<DonationEventArgs>? ClanDonation;

        public event AsyncEventHandler<LabelsChangedEventArgs<Clan, ClanLabel>>? ClanLabelsChanged;

        public event AsyncEventHandler<ChangedEventArgs<Clan, ClanVillage, ClanVillage>>? ClanVillageChanged;

        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesJoined;

        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesLeft;


        public async Task<Paginated<Clan>?> FetchAsync(ClanQueryOptions clanQueryOptions, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<Clan>>(Clan.Url(clanQueryOptions), cancellationToken).ConfigureAwait(false) as Paginated<Clan>;

        public async Task<Paginated<Clan>?> GetAsync(ClanQueryOptions clanQueryOptions)
            => await CocApi.GetAsync<Paginated<Clan>>(Clan.Url(clanQueryOptions)).ConfigureAwait(false);

        public async Task<Paginated<Clan>?> GetOrFetchAsync(ClanQueryOptions clanQueryOptions, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            if (clanQueryOptions.ClanName?.Length < 3)
                return null;

            Paginated<Clan>? cached = await GetAsync(clanQueryOptions).ConfigureAwait(false);

            if (cached == null)
                return await FetchAsync(clanQueryOptions, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return cached;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && cached.IsLocallyExpired(CocApi.CocApiConfiguration.ClanTimeToLive) == false)
                return cached;

            Paginated<Clan>? fetched = await FetchAsync(clanQueryOptions, cancellationToken).ConfigureAwait(false);

            return fetched ?? cached;
        }



        public async Task<Paginated<TopBuilderClan>?> FetchTopBuilderClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopBuilderClan>>(TopBuilderClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderClan>;

        public async Task<Paginated<TopBuilderClan>?> GetAsync(int? locationId = null)
            => await CocApi.GetAsync<Paginated<TopBuilderClan>>(TopBuilderClan.Url(locationId));

        public async Task<Paginated<TopBuilderClan>?> GetOrFetchAsync(int? locationId = null, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            Paginated<TopBuilderClan>? cached = await GetAsync(locationId).ConfigureAwait(false);

            if (cached == null)
                return await FetchTopBuilderClansAsync(locationId, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return cached;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && cached.IsLocallyExpired(CocApi.CocApiConfiguration.ClanTimeToLive) == false)
                return cached;

            Paginated<TopBuilderClan>? fetched = await FetchTopBuilderClansAsync(locationId, cancellationToken).ConfigureAwait(false);

            return fetched ?? cached;
        }





        public async Task<Paginated<TopMainClan>?> FetchTopMainClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopMainClan>>(TopMainClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainClan>;



        public async Task<Clan?> FetchAsync(string clanTag, CancellationToken? cancellationToken = null)

        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await CocApi.FetchAsync<Clan>(Clan.Url(formattedTag), cancellationToken).ConfigureAwait(false) as Clan;
        }

        public async Task<Clan?> GetAsync(string clanTag)
            => await CocApi.GetAsync<Clan>(Clan.Url(clanTag)).ConfigureAwait(false);

        public async Task<Clan?> GetOrFetchAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            return await CocApi.GetOrFetchAsync<Clan>(Clan.Url(clanTag), cacheOption, cancellationToken).ConfigureAwait(false);
            //if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
            //    throw new InvalidTagException(clanTag);

            //Clan? queued = await GetAsync(formattedTag).ConfigureAwait(false);

            //if (queued == null)
            //    return await FetchAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            //if (cacheOption == CacheOption.AllowAny)
            //    return queued;

            //if (cacheOption == CacheOption.AllowExpiredServerResponse && queued.IsLocallyExpired(CocApi.CocApiConfiguration.ClanTimeToLive) == false)
            //    return queued;

            //Clan? fetched = await FetchAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            //return fetched ?? queued;
        }


        internal void OnBadgeUrlChanged(Clan fetched, Clan queued) => ClanBadgeUrlChanged?.Invoke(this, new ChangedEventArgs<Clan, Clan>(fetched, queued));

        internal void OnClanChanged(Clan fetched, Clan queued) => ClanChanged?.Invoke(this, new ChangedEventArgs<Clan, Clan>(fetched, queued));

        internal void OnClanVillageChanged(Clan fetched, ClanVillage fetchedVillage, ClanVillage queuedVillage) => ClanVillageChanged?.Invoke(this, new ChangedEventArgs<Clan, ClanVillage, ClanVillage>(fetched, fetchedVillage, queuedVillage));

        internal void OnClanVillagesJoined(Clan fetched, List<ClanVillage> fetchedClanVillages)
        {
            if (fetchedClanVillages.Count > 0)
            {
                ClanVillagesJoined?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
            }
        }

        internal void OnClanVillagesLeft(Clan fetched, List<ClanVillage> fetchedClanVillages)
        {
            if (fetchedClanVillages.Count > 0)
            {
                ClanVillagesLeft?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
            }
        }

        internal void OnDonation(Clan fetched, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonation?.Invoke(this, new DonationEventArgs(fetched, received.ToImmutableArray(), donated.ToImmutableArray()));
            }
        }

        internal void OnLabelsChanged(Clan fetched, List<ClanLabel> added, List<ClanLabel> removed)
        {
            if (added.Count == 0 && removed.Count == 0)
                return;

            ClanLabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Clan, ClanLabel>(fetched, added.ToImmutableArray(), removed.ToImmutableArray()));
        }

        public Func<Clan, Clan, bool> IsChanged { get; set; } = new Func<Clan, Clan, bool>((fetched, stored) =>
        {
            if (stored.ClanLevel != fetched.ClanLevel ||
                stored.Description != fetched.Description ||
                stored.IsWarLogPublic != fetched.IsWarLogPublic ||
                stored.VillageCount != fetched.VillageCount ||
                stored.Name != fetched.Name ||
                stored.RequiredTrophies != fetched.RequiredTrophies ||
                stored.Recruitment != fetched.Recruitment ||
                stored.WarFrequency != fetched.WarFrequency ||
                stored.WarLosses != fetched.WarLosses ||
                stored.WarTies != fetched.WarTies ||
                stored.WarWins != fetched.WarWins ||
                stored.WarWinStreak != fetched.WarWinStreak ||
                stored.ClanPoints != fetched.ClanPoints ||
                stored.ClanVersusPoints != fetched.ClanVersusPoints ||
                stored.LocationId != fetched.LocationId ||
                stored.WarLeagueId != fetched.WarLeagueId
)
            {
                return true;
            }

            return false;
        });

        public bool DownloadClanVillages { get; set; }
    }
}