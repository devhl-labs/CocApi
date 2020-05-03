using devhl.CocApi.Exceptions;
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
        public event AsyncEventHandler? ClanQueueCompleted;
        public event AsyncEventHandler<ChangedEventArgs<Clan, Clan>>? ClanBadgeUrlChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="Clan.ClanLevel"/></description></item>
        ///    <item><description><see cref="Clan.Description"/></description></item>
        ///    <item><description><see cref="Clan.IsWarLogPublic"/></description></item>
        ///    <item><description><see cref="Clan.Name"/></description></item>
        ///    <item><description><see cref="Clan.RequiredTrophies"/>RequiredTrophies</description></item>
        ///    <item><description><see cref="Clan.Recruitment"/></description></item>
        ///    <item><description><see cref="Clan.VillageCount"/></description></item>
        ///    <item><description><see cref="Clan.WarLosses"/></description></item>
        ///    <item><description><see cref="Clan.WarWins"/></description></item>
        ///    <item><description><see cref="Clan.Wars"/></description></item>
        ///    <item><description><see cref="Clan.WarTies"/></description></item>
        ///    <item><description><see cref="Clan.WarFrequency"/></description></item>
        /// </list>
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<Clan, Clan>>? ClanChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, Clan>>? ClanLocationChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, int>>? ClanPointsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, int>>? ClanVersusPointsChanged;
        public event AsyncEventHandler<ChangedEventArgs<ClanVillage, string>>? ClanVillageNameChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesJoined;
        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<LeagueChange>>>? ClanVillagesLeagueChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>>? ClanVillagesLeft;
        public event AsyncEventHandler<ChangedEventArgs<Clan, IReadOnlyList<RoleChange>>>? ClanVillagesRoleChanged;
        public event AsyncEventHandler<ChangedEventArgs<Clan, WarLeague?>>? WarLeagueChanged;
        public event AsyncEventHandler<ClanDonationEventArgs>? ClanDonation;
        public event AsyncEventHandler<LabelsChangedEventArgs<Clan, ClanLabel>>? ClanLabelsChanged;

        internal readonly List<ClanUpdateService> _clanUpdateServices = new List<ClanUpdateService>();

        private readonly CocApi _cocApi;

        internal Clans(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        internal ConcurrentDictionary<string, Clan?> Queued { get; } = new ConcurrentDictionary<string, Clan?>();

        internal ConcurrentDictionary<string, Clan> Fetched { get; } = new ConcurrentDictionary<string, Clan>();

        internal ConcurrentDictionary<string, Village?> QueuedVillage { get; } = new ConcurrentDictionary<string, Village?>();

        internal bool DownloadingClanVillages { get; private set; } = false;

        public async Task<Clan?> FetchAsync(string clanTag, CancellationToken? cancellationToken = null)
        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await _cocApi.FetchAsync<Clan>(Clan.Url(formattedTag), cancellationToken) as Clan;
        }

        public async Task<Paginated<Clan>?> FetchAsync(ClanQueryOptions clanQueryOptions, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<Clan>>(Clan.Url(clanQueryOptions), cancellationToken).ConfigureAwait(false) as Paginated<Clan>;

        public async Task<Paginated<TopBuilderClan>?> FetchTopBuilderClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<TopBuilderClan>>(TopBuilderClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderClan>;

        public async Task<Paginated<TopMainClan>?> FetchTopMainClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<TopMainClan>>(TopMainClan.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainClan>;

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

            Clan? queued = Get(clanTag);

            if (queued != null && (allowExpiredItem || queued.IsExpired() == false))
                return queued;

            Clan? fetched = await FetchAsync(clanTag, cancellationToken).ConfigureAwait(false);

            if (fetched != null)
                _cocApi.UpdateDictionary(Fetched, formattedTag, fetched);

            return fetched ?? queued;
        }

        public void Queue(string clanTag)
        {
            try
            {
                if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                    throw new InvalidTagException(clanTag);

                Queued.TryAdd(formattedTag, null);

                foreach (var updater in _clanUpdateServices)
                    if (updater.ClanTags.Contains(formattedTag))
                        return;

                ClanUpdateService clanUpdateService = _clanUpdateServices.OrderBy(c => c.ClanTags.Count).First();

                clanUpdateService.ClanTags.Add(formattedTag);
            }
            catch (Exception e)
            {
                if (e is CocApiException)
                    throw e;
                throw new CocApiException(e.Message, e);
            }
        }

        public void Queue(IClan clan) => Queue(clan.ClanTag);

        public void Queue(IEnumerable<string> clanTags)
        {
            foreach (string clanTag in clanTags)
                Queue(clanTag);
        }

        public void Queue(IEnumerable<IClan> clans)
        {
            foreach (IClan clan in clans)
                Queue(clan.ClanTag);
        }

        public void StopQueue()
        {
            foreach (ClanUpdateService updateService in _clanUpdateServices)
                updateService.StopUpdating();
        }

        public async Task StopQueueAsync()
        {
            var tasks = new List<Task>();

            foreach (ClanUpdateService clanUpdateService in _clanUpdateServices)
            {
                tasks.Add(clanUpdateService.StopUpdatingAsync());
            }

            Task t = Task.WhenAll(tasks);

            await t.ConfigureAwait(false);
        }

        public void DownloadClanVillages(bool enabled = true) => DownloadingClanVillages = enabled;

        public void StartQueue()
        {
            foreach (ClanUpdateService clanUpdateService in _clanUpdateServices.Where(u => !u.UpdatingClans))
                clanUpdateService.StartQueue();
        }

        public void UpdateClanVillages() => DownloadingClanVillages = true;

        internal void OnBadgeUrlChanged(Clan queued, Clan fetched) => ClanBadgeUrlChanged?.Invoke(this, new ChangedEventArgs<Clan, Clan>(fetched, queued));

        internal void OnClanChanged(Clan queued, Clan fetched) => ClanChanged?.Invoke(this, new ChangedEventArgs<Clan, Clan>(fetched, queued));

        internal void ClanPointsChangedEvent(Clan fetched, int increase) => ClanPointsChanged?.Invoke(this, new ChangedEventArgs<Clan, int>(fetched, increase));

        internal void ClanVersusPointsChangedEvent(Clan fetched, int increase) => ClanVersusPointsChanged?.Invoke(this, new ChangedEventArgs<Clan, int>(fetched, increase));

        internal void ClanVillageNameChangedEvent(ClanVillage fetched, string oldName) => ClanVillageNameChanged?.Invoke(this, new ChangedEventArgs<ClanVillage, string>(fetched, oldName));

        internal void OnClanVillagesJoined(Clan fetched, List<ClanVillage> fetchedClanVillages)
        {
            if (fetchedClanVillages.Count > 0)
            {
                ClanVillagesJoined?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
            }
        }

        internal void ClanVillagesLeagueChangedEvent(Clan fetched, List<LeagueChange> leagueChanges)
        {
            if (leagueChanges.Count > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<LeagueChange>>(fetched, leagueChanges.ToImmutableArray()));
            }
        }

        internal void OnClanVillagesLeft(Clan fetched, List<ClanVillage> fetchedClanVillages)
        {
            if (fetchedClanVillages.Count > 0)
            {
                ClanVillagesLeft?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
            }
        }

        internal void OnClanVillagesRoleChanged(Clan fetched, List<RoleChange> roleChanges)
        {
            if (roleChanges.Count > 0)
            {
                ClanVillagesRoleChanged?.Invoke(this, new ChangedEventArgs<Clan, IReadOnlyList<RoleChange>>(fetched, roleChanges.ToImmutableArray()));
            }
        }

        internal void CreateClanUpdateServices()
        {
            try
            {
                if (_cocApi.CocApiConfiguration.NumberOfUpdaters < 1)
                {
                    _clanUpdateServices.Add(new ClanUpdateService(_cocApi));
                }
                else
                {
                    for (int i = 0; i < _cocApi.CocApiConfiguration.NumberOfUpdaters; i++)
                    {
                        _clanUpdateServices.Add(new ClanUpdateService(_cocApi));
                    }
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        internal void OnDonation(Clan fetched, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonation?.Invoke(this, new ClanDonationEventArgs(fetched, received.ToImmutableArray(), donated.ToImmutableArray()));
            }
        }

        internal void OnLabelsChanged(Clan fetched, List<ClanLabel> added, List<ClanLabel> removed)
        {
            if (added.Count == 0 && removed.Count == 0)
                return;

            ClanLabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Clan, ClanLabel>(fetched, added.ToImmutableArray(), removed.ToImmutableArray()));
        }

        internal void LocationChangedEvent(Clan queued, Clan fetched) => ClanLocationChanged?.Invoke(this, new ChangedEventArgs<Clan, Clan>(fetched, queued));

        internal void OnWarLeagueChanged(Clan fetched, WarLeague? queuedWarLeague)
        {
            WarLeagueChanged?.Invoke(this, new ChangedEventArgs<Clan, WarLeague?>(fetched, queuedWarLeague));
        }

        internal void QueueCompletedEvent() => ClanQueueCompleted?.Invoke(this, EventArgs.Empty);












        internal Village? GetVillage(string villageTag)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            QueuedVillage.TryGetValue(formattedTag, out Village? queued);

            return queued;
        }

        internal async Task<Village?> GetVillageAsync(string villageTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            Village? queued = GetVillage(formattedTag);

            if (queued != null && (allowExpiredItem || queued.IsExpired() == false))
                return queued;

            Village? fetched = await _cocApi.Villages.FetchAsync(formattedTag, cancellationToken);

            if (fetched != null)
                _cocApi.UpdateDictionary(_cocApi.Clans.QueuedVillage, fetched.VillageTag, fetched);

            return fetched ?? queued;
        }
    }
}