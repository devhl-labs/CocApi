using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        private readonly object _isAvailableLock = new object();

        private readonly List<UpdateService> _updateServices = new List<UpdateService>();

        private volatile bool _isAvailable = true;

        private bool _isInitialized = false;

        public CocApi(ILogger? logger = null)
        {
            Logger = logger;
        }

        /// <summary>
        /// Controls whether any clan will download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.Auto;

        /// <summary>
        /// Controls whether any clan will be able to download villages.
        /// </summary>
        public bool DownloadVillages { get; set; } = false;

        /// <summary>
        /// Controls whether any clan will download the current war.
        /// </summary>
        public bool DownloadCurrentWar { get; set; } = true;

        public bool IsAvailable
        {
            get
            {
                lock (_isAvailableLock)
                {
                    return _isAvailable;
                }
            }

            internal set
            {
                lock (_isAvailableLock)
                {
                    if (_isAvailable != value)
                    {
                        _isAvailable = value;

                        ApiIsAvailableChanged?.Invoke(_isAvailable);
                    }
                }
            }
        }

        public ILogger? Logger { get; set; }

        public Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        internal ConcurrentDictionary<string, Clan> AllClans { get; } = new ConcurrentDictionary<string, Clan>();

        internal ConcurrentDictionary<string, ILeagueGroup> AllLeagueGroups { get; } = new ConcurrentDictionary<string, ILeagueGroup>();

        internal ConcurrentDictionary<string, Village> AllVillages { get; } = new ConcurrentDictionary<string, Village>();

        internal ConcurrentDictionary<string, IWar> AllWarsByClanTag { get; } = new ConcurrentDictionary<string, IWar>();

        internal ConcurrentDictionary<string, IActiveWar> AllWarsByWarId { get; } = new ConcurrentDictionary<string, IActiveWar>();

        internal ConcurrentDictionary<string, LeagueWar> AllWarsByWarTag { get; } = new ConcurrentDictionary<string, LeagueWar>();

        internal Paginated<League> AllLeagues { get; private set; } = new Paginated<League>();

        internal Paginated<Label> AllVillageLabels { get; private set; } = new Paginated<Label>();

        internal Paginated<Label> AllClanLabels { get; private set; } = new Paginated<Label>();

        internal Paginated<Location> AllLocations { get; private set; } = new Paginated<Location>();

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach (UpdateService updateService in _updateServices)
            {
                updateService.StopUpdatingClans();
            }

            foreach (CancellationTokenSource cancellationTokenSource in _cancellationTokenSources)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception)
                {
                }

                cancellationTokenSource.Dispose();
            }

            WebResponse.ApiClient.Dispose();

            WebResponse.SemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public ConcurrentBag<WebResponseTimer> GetTimers() => WebResponse.GetTimers();

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

        /// <summary>
        /// Initializes the CocApi library.  A configuration with SC Api tokens is required.  A logger may also be provided.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="logger"></param>
        /// <exception cref="CocApiException"></exception>
        public async Task InitializeAsync(CocApiConfiguration cfg)
        {
            if (cfg != null)
            {
                CocApiConfiguration = cfg;
            }

            if (cfg == null || cfg.Tokens.Count == 0)
            {
                throw new CocApiException("You did not provide any tokens to access the SC Api.");
            }

            DownloadLeagueWars = cfg.DownloadLeagueWars;

            DownloadVillages = cfg.DownloadVillages;

            DownloadCurrentWar = cfg.DownloadCurrentWar;

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            CreateUpdaters();

            await GetLeaguesAsync().ConfigureAwait(false);

            await GetClanLabelsAsync().ConfigureAwait(false);

            await GetVillageLabelsAsync().ConfigureAwait(false);

            await GetLocationsAsync().ConfigureAwait(false);

            _isInitialized = true;
        }

        /// <summary>
        /// Determines whether CWL should be downloading.
        /// When DownloadLeagueWars is set to Auto, this returns true during the first week of the month
        /// and the first three hours of day 8.  This is just to give you time to complete the downloads.
        /// </summary>
        /// <returns></returns>
        public bool IsDownloadingLeagueWars()
        {
            if (DownloadLeagueWars == DownloadLeagueWars.False) return false;

            if (DownloadLeagueWars == DownloadLeagueWars.True) return true;

            if (DownloadLeagueWars == DownloadLeagueWars.Auto)
            {
                int day = DateTime.UtcNow.Day;

                if (day > 0 && day < 11)
                {
                    return true;
                }

                //just to ensure we get everything we need
                if (day == 11 && DateTime.UtcNow.Hour < 3)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a string appears to be a SuperCell tag. Will not do any formatting to the input.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            if (tag == "#0")
            {
                return false;
            }

            return ValidTagCharacters.IsMatch(tag);
        }

        /// <summary>
        /// Check if user input could be a valid tag. Will add leading #, replace O with 0, and check against a regular expression.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="formattedTag"></param>
        /// <returns></returns>
        public bool IsValidTag(string userInput, out string formattedTag)
        {
            formattedTag = string.Empty;

            if (string.IsNullOrEmpty(userInput)) return false;

            formattedTag = userInput.ToUpper();

            formattedTag = formattedTag.Replace("O", "0");

            if (!formattedTag.StartsWith("#")) formattedTag = $"#{formattedTag}";

            var result = IsValidTag(formattedTag);

            if (!result) formattedTag = string.Empty;

            return result;
        }

        /// <summary>
        /// Poll the Api and fire off events when a change is noticed.
        /// </summary>
        public void StartUpdatingClans()
        {
            ThrowIfNotInitialized();

            foreach (UpdateService clanUpdateService in _updateServices.Where(u => !u.ObjectsAreBeingUpdated))
            {
                clanUpdateService.StartUpdatingClans();
            }
        }

        /// <summary>
        /// Stop polling the Api.  Events will not fire.  Returns immediately, but will take time to complete.
        /// </summary>
        /// <returns></returns>
        public void StopUpdatingClans()
        {
            foreach (UpdateService updateService in _updateServices)
            {
                updateService.StopUpdatingClans();
            }
        }

        /// <summary>
        /// Stop polling the Api.  Events will not fire.  This could take some time to finish if updating villages or league wars.
        /// </summary>
        /// <returns></returns>
        public async Task StopUpdatingClansAsync()
        {
            var tasks = new List<Task>();

            foreach (UpdateService clanUpdateService in _updateServices)
            {
                tasks.Add(clanUpdateService.StopUpdatingClansAsync());
            }

            Task t = Task.WhenAll(tasks);

            await t;
        }

        /// <summary>
        /// Check if a string appears to be a SuperCell tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <exception cref="InvalidTagException"></exception>
        public void ThrowIfInvalidTag(string tag)
        {
            if (!IsValidTag(tag))
            {
                //Logger.LogWarning(LoggingEvents.InvalidTag, "{source} The provided tag is not valid {tag}", _source, tag);

                _ = Logger?.Log<CocApi>(LoggingEvent.InvalidTag, $"The provided tag {tag} is not valid.");

                throw new InvalidTagException();
            }
        }

        private void ThrowIfNoUpdatersCreated()
        {
            if (_updateServices.Count == 0) throw new CocApiException("There are no update services created.");
        }

        /// <summary>
        /// Begin watching a new clan.  This is to add new clans to be watched after your program has started.
        /// </summary>
        /// <param name="clanTag"></param>
        public void WatchClan(string clanTag)
        {
            ThrowIfNotInitialized();

            ThrowIfInvalidTag(clanTag);

            ThrowIfNoUpdatersCreated();

            try
            {
                foreach(var updater in _updateServices)
                {
                    if (updater.ClanStrings.Contains(clanTag)) return;
                }

                UpdateService clanUpdateService = _updateServices.OrderBy(c => c.ClanStrings.Count).First();

                clanUpdateService.ClanStrings.Add(clanTag);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// Establish the clans that you would like to poll for updates.  Run this when your program starts.  After running this, run <see cref="StartUpdatingClans"/>.  Watching a large number of clans will take a lot of memory.  If you watch clans, you should have caching enabled.
        /// </summary>
        /// <param name="clanTags"></param>
        public void WatchClans(IEnumerable<string> clanTags)
        {
            ThrowIfNotInitialized();

            ThrowIfNoUpdatersCreated();

            try
            {
                int j = 0;

                foreach (string clanTag in clanTags)
                {
                    if (IsValidTag(clanTag))
                    {
                        _updateServices[j].ClanStrings.Add(clanTag); 
                    }

                    j++;

                    if (j >= CocApiConfiguration.NumberOfUpdaters) { j = 0; }
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }
        
        /// <summary>
        /// Establish the clans that you would like to poll for updates.  Run this when your program starts.  After running this, run <see cref="StartUpdatingClans"/>.  Watching a large number of clans will take a lot of memory.  If you watch clans, you should have caching enabled.
        /// </summary>
        /// <param name="clans"></param>
        public void WatchClans(IEnumerable<Clan> clans)
        {
            ThrowIfNotInitialized();

            ThrowIfNoUpdatersCreated();

            try
            {
                int j = 0;

                foreach (Clan clan in clans)
                {
                    if (IsValidTag(clan.ClanTag))
                    {
                        _updateServices[j].ClanStrings.Add(clan.ClanTag);
                    }

                    j++;

                    if (j >= CocApiConfiguration.NumberOfUpdaters) { j = 0; }
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        internal async Task<Downloadable> GetAsync<TResult>(string url, EndPoint endPoint, CancellationToken? cancellationToken = null) where TResult : Downloadable, new()
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

            if (cancellationToken == null) cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

            AddCancellationTokenSource(cts);

            Downloadable result = await WebResponse.GetDownloadableAsync<TResult>(endPoint, url, cts.Token).ConfigureAwait(false);

            RemoveCancellationTokenSource(cts);

            return result;
        }

        /// <summary>
        /// Load this library's stored objects with objects from your database.  
        /// <para>Running this will enable the program to fire events for actions that occured while the program was not running.</para>
        /// <para>After running this, run <see cref="WatchClans(IEnumerable{Clan})"/>, then <see cref="StartUpdatingClans"/></para>
        /// <para>If your bot was off for an extended period of time, you probably do not want to run this.</para>
        /// </summary>
        /// <param name="clans"></param>
        /// <param name="badgeUrls"></param>
        /// <param name="clanLabels"></param>
        /// <param name="clanVillages"></param>
        public void LoadFromDatabase(
                            IEnumerable<Clan> clans, 
                            IEnumerable<BadgeUrl>? badgeUrls, 
                            IEnumerable<ClanLabel>? clanLabels, 
                            IEnumerable<ClanVillage>? clanVillages)
        {
            ThrowIfNotInitialized();

            try
            {
                foreach(ClanVillage village in clanVillages.Where(v => v.LeagueId != null))
                {
                    League league = AllLeagues.Items.FirstOrDefault(l => l.Id == village.LeagueId);

                    if (league != null) village.League = league;
                }

                foreach(ClanLabel clanLabel in clanLabels.EmptyIfNull())
                {
                    clanLabel.LabelUrl = AllClanLabels.Items.FirstOrDefault(l => l.Id == clanLabel.Id).LabelUrl;
                }

                //lock (AllClans)
                //{
                foreach (Clan clan in clans.EmptyIfNull())
                {
                    if (Logger != null) clan.Logger = Logger;

                    clan.AnnounceWars = true;

                    clan.BadgeUrl = badgeUrls.FirstOrDefault(b => b.ClanTag == clan.ClanTag);

                    clan.Location = AllLocations.Items.FirstOrDefault(l => l.Id == clan.LocationId);

                    clan.Villages = clanVillages.Where(v => v.ClanTag == clan.ClanTag).ToList();

                    clan.Labels = clanLabels.Where(c => c.ClanTag == clan.ClanTag).ToList();

                    AllClans.TryAdd(clan.ClanTag, clan);
                }
                //}
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// Load this library's stored objects with objects from your database.  
        /// <para>Running this will enable the program to fire events for actions that occured while the program was not running.</para>
        /// <para>After running this, run <see cref="WatchClans(IEnumerable{Clan})"/>, then <see cref="StartUpdatingClans"/></para>
        /// <para>If your bot was off for an extended period of time, you probably do not want to run this.</para>
        /// </summary>
        /// <param name="villages"></param>
        /// <param name="villageLabels"></param>
        /// <param name="achievements"></param>
        /// <param name="troops"></param>
        /// <param name="spells"></param>
        public void LoadFromDatabase(
                             IEnumerable<Village> villages, 
                             IEnumerable<VillageLabel>? villageLabels,
                             IEnumerable<Achievement>? achievements,
                             IEnumerable<Troop>? troops,
                             IEnumerable<Spell>? spells)
        {
            ThrowIfNotInitialized();

            try
            {
                foreach(VillageLabel villageLabel in villageLabels.EmptyIfNull())
                {
                    villageLabel.LabelUrl = AllVillageLabels.Items.FirstOrDefault(l => l.Id == villageLabel.Id).LabelUrl;
                }

                foreach (Village village in villages.EmptyIfNull())
                {
                    village.Labels = villageLabels.Where(l => l.VillageTag == village.VillageTag).ToList();                        

                    village.Achievements = achievements.Where(a => a.VillageTag == village.VillageTag);

                    village.Soldiers = troops.Where(t => t.VillageTag == village.VillageTag);

                    village.Spells = spells.Where(s => s.VillageTag == village.VillageTag);

                    village.League = AllLeagues.Items.FirstOrDefault(l => l.Id == village.LeagueId);

                    village.Initialize();

                    AllVillages.TryAdd(village.VillageTag, village);
                }                
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// Load this library's stored objects with objects from your database.  
        /// <para>Running this will enable the program to fire events for actions that occured while the program was not running.</para>
        /// <para>After running this, run <see cref="WatchClans(IEnumerable{Clan})"/>, then <see cref="StartUpdatingClans"/></para>
        /// <para>If your bot was off for an extended period of time, you probably do not want to run this.</para>
        /// </summary>
        /// <param name="wars"></param>
        /// <param name="leagueGroups"></param>
        /// <param name="leagueClans"></param>
        /// <param name="leagueVillages"></param>
        /// <param name="attacks"></param>
        /// <param name="warClans"></param>
        /// <param name="currentWarFlags"></param>
        public void LoadFromDatabase(
                             IEnumerable<IActiveWar> wars,
                             IEnumerable<ILeagueGroup>? leagueGroups,
                             IEnumerable<LeagueClan>? leagueClans,
                             IEnumerable<LeagueVillage>? leagueVillages,
                             IEnumerable<Attack>? attacks,
                             IEnumerable<WarClan>? warClans,
                             IEnumerable<CurrentWarFlags>? currentWarFlags)
        {
            ThrowIfNotInitialized();

            if (AllClans.Count == 0) throw new CocApiException("You must load the clans before loading the wars.");

            try
            {
                foreach (CurrentWar war in wars.EmptyIfNull())
                {
                    war.Attacks = attacks.Where(a => a.WarId == war.WarId).ToList();

                    war.Clans = warClans.Where(c => c.WarId == war.WarId).OrderBy(c => c.ClanTag).ToList();

                    war.Flags = currentWarFlags.FirstOrDefault(f => f.WarId == war.WarId);

                    war.Initialize();
                }

                foreach (LeagueClan leagueClan in leagueClans.EmptyIfNull())
                {
                    Clan? clan = GetClanOrDefault(leagueClan.ClanTag);

                    if (clan != null) leagueClan.BadgeUrl = clan.BadgeUrl;

                    leagueClan.Villages = leagueVillages.Where(v => v.LeagueClanId == leagueClan.LeagueClanId);

                    leagueClan.Initialize();
                }

                foreach (LeagueWar leagueWar in wars.EmptyIfNull())
                {
                    AllWarsByWarId.TryAdd(leagueWar.WarTag, leagueWar);                    
                }

                foreach (CurrentWar currentWar in wars.EmptyIfNull())
                {
                    AllWarsByClanTag.TryAdd(currentWar.Clans[0].ClanTag, currentWar);

                    AllWarsByClanTag.TryAdd(currentWar.Clans[1].ClanTag, currentWar);

                    AllWarsByWarId.TryAdd(currentWar.WarId, currentWar);

                    Clan? clan = GetClanOrDefault(currentWar.Clans[0].ClanTag);

                    if (clan != null) clan.Wars.TryAdd(currentWar.WarId, currentWar);

                    clan = GetClanOrDefault(currentWar.Clans[1].ClanTag);

                    if (clan != null) clan.Wars.TryAdd(currentWar.WarId, currentWar);
                }

                foreach (LeagueGroup leagueGroup in leagueGroups.EmptyIfNull())
                {
                    leagueGroup.Clans = leagueClans.Where(c => c.GroupId == leagueGroup.GroupId);

                    leagueGroup.Initialize();

                    AllLeagueGroups.TryAdd(leagueGroup.GroupId, leagueGroup);
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        private void AddCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Add(cts);
            }
        }

        private void CreateUpdaters()
        {
            try
            {
                if (CocApiConfiguration.NumberOfUpdaters < 1)
                {
                    return;
                }

                for (int i = 0; i < CocApiConfiguration.NumberOfUpdaters; i++)
                {
                    _updateServices.Add(new UpdateService(this));
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        private void RemoveCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Remove(cts);
            }
        }

        private void ThrowIfNotInitialized()
        {
            if (!_isInitialized || CocApiConfiguration.Tokens == null || CocApiConfiguration.Tokens.Count == 0)
            {
                throw new CocApiException("The library is not initialized, or you did not provide SC Api tokens.");
            }
        }
    }
}
