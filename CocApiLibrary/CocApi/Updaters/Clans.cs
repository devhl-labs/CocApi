﻿using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        public event AsyncEventHandler? QueuePopulated;

        public event AsyncEventHandler<ChangedEventArgs<Clan, ClanVillage, ClanVillage>>? ClanVillageChanged;

        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesJoined;

        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesLeft;

        public bool QueueClanVillages { get; set; } = false;

        internal List<ClanUpdateGroup> ClanUpdateGroups { get; } = new List<ClanUpdateGroup>();

        internal ConcurrentDictionary<string, Clan?> Queued { get; } = new ConcurrentDictionary<string, Clan?>();

        internal ConcurrentDictionary<string, Village?> QueuedVillage { get; } = new ConcurrentDictionary<string, Village?>();

        public async Task<Clan?> FetchAsync(string clanTag, CancellationToken? cancellationToken = null)
        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await CocApi.FetchAsync<Clan>(Clan.Url(formattedTag), cancellationToken) as Clan;
        }

        public async Task<Paginated<Clan>?> FetchAsync(ClanQueryOptions clanQueryOptions, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<Clan>>(Clan.Url(clanQueryOptions), cancellationToken).ConfigureAwait(false) as Paginated<Clan>;

        public async Task<Paginated<TopBuilderClan>?> FetchTopBuilderClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopBuilderClan>>(TopBuilderClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderClan>;

        public async Task<Paginated<TopMainClan>?> FetchTopMainClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopMainClan>>(TopMainClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainClan>;

        public Clan? Get(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            Queued.TryGetValue(formattedTag, out Clan? queued);

            return queued;
        }

        public async Task<Clan?> GetAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            Clan? queued = Get(formattedTag);

            if (queued != null && (allowExpiredItem || queued.IsExpired() == false))
                return queued;

            Clan? fetched = await FetchAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            return fetched ?? queued;
        }

        public void Queue(string clanTag) => Queue(clanTag, null);

        public void Queue(Clan clan) => Queue(clan.ClanTag, clan);

        public void Queue(IEnumerable<string> clanTags)
        {
            foreach (string clanTag in clanTags)
                Queue(clanTag, null);
        }

        public void Queue(IEnumerable<Clan> clans)
        {
            foreach (Clan clan in clans)
                Queue(clan.ClanTag, clan);
        }

        public void StartQueue()
        {
            foreach (ClanUpdateGroup clanUpdateGroup in ClanUpdateGroups.Where(u => !u.QueueRunning))
                clanUpdateGroup.StartQueue();
        }

        public void StopQueue()
        {
            foreach (ClanUpdateGroup updateService in ClanUpdateGroups)
                updateService.StopUpdating();
        }

        public async Task StopQueueAsync()
        {
            var tasks = new List<Task>();

            foreach (ClanUpdateGroup clanUpdateService in ClanUpdateGroups)
            {
                tasks.Add(clanUpdateService.StopUpdatingAsync());
            }

            Task t = Task.WhenAll(tasks);

            await t.ConfigureAwait(false);
        }

        internal void CreateClanUpdateServices()
        {
            try
            {
                if (CocApi.CocApiConfiguration.NumberOfUpdaters < 1)
                {
                    ClanUpdateGroups.Add(new ClanUpdateGroup(CocApi));
                }
                else
                {
                    for (int i = 0; i < CocApi.CocApiConfiguration.NumberOfUpdaters; i++)
                    {
                        ClanUpdateGroups.Add(new ClanUpdateGroup(CocApi));
                    }
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        internal Village? GetVillage(string villageTag)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            QueuedVillage.TryGetValue(formattedTag, out Village? queued);

            return queued;
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

        internal void OnQueuePopulated()
        {
            if (ClanUpdateGroups.All(c => c.QueueIsPopulated))           
                QueuePopulated?.Invoke(this, EventArgs.Empty);                           
        }

        private void Queue(string clanTag, Clan? clan)
        {
            try
            {
                if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                    throw new InvalidTagException(clanTag);

                Queued.TryAdd(formattedTag, clan);

                foreach (var updater in ClanUpdateGroups)
                    if (updater.ClanTags.Contains(formattedTag))
                        return;

                ClanUpdateGroup clanUpdateService = ClanUpdateGroups.OrderBy(c => c.ClanTags.Count).First();

                clanUpdateService.ClanTags.Add(formattedTag);
            }
            catch (Exception e)
            {
                if (e is CocApiException)
                    throw e;
                throw new CocApiException(e.Message, e);
            }
        }

        public Func<Clan, Clan, bool> IsClanChanged { get; set; } = new Func<Clan, Clan, bool>((fetched, stored) =>
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
    }
}