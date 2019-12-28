using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

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
        private volatile bool _isAvailable = true;

        private readonly List<UpdateService> _updateServices = new List<UpdateService>();

        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        internal Dictionary<string, ClanApiModel> AllClans { get; } = new Dictionary<string, ClanApiModel>();

        internal Dictionary<string, IWar> AllWarsByClanTag { get; } = new Dictionary<string, IWar>();

        internal Dictionary<string, ICurrentWarApiModel> AllWarsByWarId { get; } = new Dictionary<string, ICurrentWarApiModel>();

        internal Dictionary<string, LeagueWarApiModel> AllWarsByWarTag { get; } = new Dictionary<string, LeagueWarApiModel>();

        internal Dictionary<string, ILeagueGroup> AllLeagueGroups { get; } = new Dictionary<string, ILeagueGroup>();

        internal Dictionary<string, VillageApiModel> AllVillages { get; } = new Dictionary<string, VillageApiModel>();

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        public Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public ILogger? Logger { get; set; }

        public CocApi(ILogger? logger = null)
        {
            Logger = logger;
        }


        /// <summary>
        /// Controls whether any clan will be able to download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.False;

        /// <summary>
        /// Controls whether any clan will be able to download villages.
        /// </summary>
        public bool DownloadVillages { get; set; } = false;

        private readonly object _isAvailableLock = new object();

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


        private readonly string _source = "CocApi       | ";

        private bool _isInitialized = false;

        /// <summary>
        /// Initializes the CocApi library.  A configuration with SC Api tokens is required.  A logger may also be provided.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="logger"></param>
        /// <exception cref="CocApiException"></exception>
        public void Initialize(CocApiConfiguration cfg)
        {
            if (cfg != null)
            {
                CocApiConfiguration = cfg;
            }

            if (cfg == null || cfg.Tokens.Count == 0)
            {
                throw new CocApiException("You did not provide any tokens to access the SC Api.");
            }

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            CreateUpdaters();

            _isInitialized = true;
        }

        //internal void CrashDetectedEvent()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            //wait to allow the updater to finish crashing
        //            await Task.Delay(5000).ConfigureAwait(false);

        //            StartUpdatingClans();

        //            Logger.LogInformation(LoggingEvents.None, "{source} Update services restarted.", _source);
        //        });    
        //    }
        //    catch (Exception e)
        //    { 
        //        Logger.LogWarning(LoggingEvents.UnhandledError, "{source} {message}", _source, e.Message);
        //    }
        //}

        //internal void ClanDonationsResetEvent(ClanApiModel oldClan, ClanApiModel newClan)
        //{
        //    ClanDonationsReset?.Invoke(oldClan, newClan);
        //}

        //internal void ClanVillagesRoleChangedEvent(ClanApiModel clan, List<RoleChange> roleChanges)
        //{
        //    if (roleChanges.Count > 0)
        //    {

        //        ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
        //    }
        //}

        //internal void ClanVillagesLeagueChangedEvent(ClanApiModel oldClan, List<LeagueChange> leagueChanged)
        //{
        //    if (leagueChanged.Count > 0)
        //    {
        //        ClanVillagesLeagueChanged?.Invoke(oldClan, leagueChanged.ToImmutableArray());

        //    }
        //}

        //internal void ClanVillageNameChangedEvent(ClanVillageApiModel oldVillage, string newName)
        //{
        //    ClanVillageNameChanged?.Invoke(oldVillage, newName);
        //}

        //internal void ClanDonationsEvent(ClanApiModel oldClan, List<Donation> received, List<Donation> donated)
        //{
        //    if (received.Count > 0 || donated.Count > 0)
        //    {
        //        ClanDonations?.Invoke(oldClan, received.ToImmutableArray(), donated.ToImmutableArray());
        //    }
        //}

        //internal void VillageReachedLegendsLeagueEvent(VillageApiModel villageApiModel)
        //{
        //    VillageReachedLegendsLeague?.Invoke(villageApiModel);
        //}

        //internal void ClanLabelsChangedEvent(ClanApiModel newClan, List<ClanLabelApiModel> addedLabels, List<ClanLabelApiModel> removedLabels)
        //{
        //    if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

        //    ClanLabelsChanged?.Invoke(newClan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        //}

        //internal void VillageLabelsChangedEvent(VillageApiModel newVillage, List<VillageLabelApiModel> addedLabels, List<VillageLabelApiModel> removedLabels)
        //{
        //    if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

        //    VillageLabelsChanged?.Invoke(newVillage, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());

        //}

        //internal void LeagueGroupTeamSizeChangedEvent(LeagueGroupApiModel leagueGroupApiModel)
        //{
        //    LeagueGroupTeamSizeChanged?.Invoke(leagueGroupApiModel);
        //}

        //internal void WarEndSeenEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarEndSeen?.Invoke(currentWarApiModel);
        //}

        //internal void WarEndedEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarEnded?.Invoke(currentWarApiModel);
        //}

        //internal void WarStartedEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarStarted?.Invoke(currentWarApiModel);
        //}

        //internal void VillageSpellsChangedEvent(VillageApiModel oldVillage, List<VillageSpellApiModel> newSpells)
        //{
        //    VillageSpellsChanged?.Invoke(oldVillage, newSpells.ToImmutableArray());
        //}

        //internal void VillageHeroesChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newHeroes)
        //{
        //    VillageHeroesChanged?.Invoke(oldVillage, newHeroes.ToImmutableArray());
        //}

        //internal void VillageTroopsChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newTroops)
        //{
        //    VillageTroopsChanged?.Invoke(oldVillage, newTroops.ToImmutableArray());
        //}

        //internal void VillageAchievementsChangedEvent(VillageApiModel oldVillage, List<AchievementApiModel> newAchievements)
        //{
        //    VillageAchievementsChanged?.Invoke(oldVillage, newAchievements.ToImmutableArray());
        //}

        //internal void VillageVersusTrophiesChangedEvent(VillageApiModel oldVillage, int newVersusTrophies)
        //{
        //    VillageVersusTrophiesChanged?.Invoke(oldVillage, newVersusTrophies);
        //}

        //internal void VillageVersusBattleWinsChangedEvent(VillageApiModel oldVillage, int newVersusBattleWins)
        //{
        //    VillageVersusBattleWinsChanged?.Invoke(oldVillage, newVersusBattleWins);
        //}

        //internal void VillageVersusBattleWinCountChangedEvent(VillageApiModel oldVillage, int newVersusBattleWinCount)
        //{
        //    VillageVersusBattleWinCountChanged?.Invoke(oldVillage, newVersusBattleWinCount);
        //}

        //internal void VillageTrophiesChangedEvent(VillageApiModel oldVillage, int newTrophies)
        //{
        //    VillageTrophiesChanged?.Invoke(oldVillage, newTrophies);
        //}

        //internal void VillageExpLevelChangedEvent(VillageApiModel oldVillage, int newExpLevel)
        //{
        //    VillageExpLevelChanged?.Invoke(oldVillage, newExpLevel);
        //}

        //internal void VillageDefenseWinsChangedEvent(VillageApiModel oldVillage, int newDefenseWinsChanged)
        //{
        //    VillageDefenseWinsChanged?.Invoke(oldVillage, newDefenseWinsChanged);
        //}

        //internal void VillageChangedEvent(VillageApiModel oldVillage, VillageApiModel newVillage)
        //{
        //    VillageChanged?.Invoke(oldVillage, newVillage);
        //}

        //internal void WarEndNotSeenEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarEndNotSeen?.Invoke(currentWarApiModel);
        //}

        //internal void WarIsAccessibleChangedEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarIsAccessibleChanged?.Invoke(currentWarApiModel);
        //}

        //internal void ClanPointsChangedEvent(ClanApiModel oldClan, int newClanPoints)
        //{
        //    ClanPointsChanged?.Invoke(oldClan, newClanPoints);
        //}

        //internal void ClanVersusPointsChangedEvent(ClanApiModel oldClan, int newClanVersusPoints)
        //{
        //    ClanVersusPointsChanged?.Invoke(oldClan, newClanVersusPoints);
        //}

        //internal void WarStartingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarStartingSoon?.Invoke(currentWarApiModel);
        //}

        //internal void WarEndingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    WarEndingSoon?.Invoke(currentWarApiModel);
        //}

        //internal void NewAttacksEvent(ICurrentWarApiModel currentWarApiModel, List<AttackApiModel> attackApiModels)
        //{
        //    if (attackApiModels.Count > 0)
        //    {
        //        NewAttacks?.Invoke(currentWarApiModel, attackApiModels.ToImmutableArray());
        //    }
        //}

        //internal void WarChangedEvent(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar)
        //{
        //    WarChanged?.Invoke(oldWar, newWar);
        //}

        //internal void NewWarEvent(ICurrentWarApiModel currentWarApiModel)
        //{
        //    NewWar?.Invoke(currentWarApiModel);
        //}

        //internal void VillagesLeftEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        //{
        //    if (clanVillageApiModels.Count > 0)
        //    {
        //        VillagesLeft?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
        //    }            
        //}

        //internal void ClanLocationChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        //{
        //    ClanLocationChanged?.Invoke(oldClan, newClan);
        //}

        //internal void ClanBadgeUrlChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        //{
        //    ClanBadgeUrlChanged?.Invoke(oldClan, newClan);
        //}

        //internal void ClanChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        //{
        //    ClanChanged?.Invoke(oldClan, newClan);
        //}

        //internal void VillagesJoinedEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        //{
        //    if (clanVillageApiModels.Count > 0)
        //    {
        //        VillagesJoined?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
        //    }
        //}




        ////public async Task<IDownloadable> GetAsync<TResult>(string url, EndPoint endPoint, CancellationTokenSource? cancellationTokenSource = null) where TResult : class, IDownloadable, new()
        ////{
        ////    using CancellationTokenSource cts = GetCancellationTokenSource();

        ////    cancellationTokenSource?.Token.Register(() => cts.Cancel());

        ////    IDownloadable result = await WebResponse.DownloadAsync<TResult>(endPoint, url, cts);

        ////    RemoveCancellationTokenSource(cts);

        ////    return result;
        ////}

        internal async Task<IDownloadable> GetAsync<TResult>(string url, EndPoint endPoint, CancellationToken? cancellationToken = null) where TResult : class, IDownloadable, new()
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

            if (cancellationToken == null) cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

            AddCancellationTokenSource(cts);

            IDownloadable result = await WebResponse.GetIDownloadableAsync<TResult>(endPoint, url, cts.Token).ConfigureAwait(false);

            RemoveCancellationTokenSource(cts);

            return result;
        }


        /// <summary>
        /// Poll the Api and fire off events when a change is noticed.
        /// </summary>
        public void StartUpdatingClans()
        {
            VerifyInitialization();

            foreach (UpdateService clanUpdateService in _updateServices.Where(u => !u.ObjectsAreBeingUpdated))
            {
                clanUpdateService.StartUpdatingClans();
            }
        }

        /// <summary>
        /// Stop polling the Api.  Events will not fire.  This could take some time to finish if updating villages or league wars.
        /// </summary>
        /// <returns></returns>
        public async Task StopUpdatingClansAsync()
        {
            var tasks = new List<Task>();
                       
            foreach(UpdateService clanUpdateService in _updateServices)
            {
                tasks.Add(clanUpdateService.StopUpdatingClansAsync());
            }

            Task t = Task.WhenAll(tasks);

            await t;
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
        /// Establish the clans that you would like to poll for updates.  Run this when your program starts.  After running this, run <see cref="StartUpdatingClans"/>.  Watching a large number of clans will take a lot of memory.  If you watch clans, you should have caching enabled.
        /// </summary>
        /// <param name="clanTags"></param>
        public void WatchClans(IEnumerable<string> clanTags)
        {
            VerifyInitialization();

            try
            {
                int j = 0;

                foreach (string clanTag in clanTags)
                {
                    try
                    {
                        ThrowIfInvalidTag(clanTag);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    //_updateServices.ElementAt(j).ClanStrings.Add(clanTag);

                    _updateServices[j].ClanStrings.Add(clanTag);  //todo does this work?

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

        public void WatchClans(IEnumerable<ClanApiModel> clans)
        {
            VerifyInitialization();

            try
            {
                int j = 0;

                foreach (ClanApiModel clan in clans)
                {
                    try
                    {
                        ThrowIfInvalidTag(clan.ClanTag);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    //_updateServices.ElementAt(j).ClanStrings.Add(clan.ClanTag); //todo does this work?

                    _updateServices[j].ClanStrings.Add(clan.ClanTag);

                    j++;

                    if (j >= CocApiConfiguration.NumberOfUpdaters) { j = 0; }
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        ///// <summary>
        ///// Load this library's stored objects with objects from your database.  
        ///// You must still run WatchClans to establish which clans you want to keep updated.
        ///// Running this will enable the program to fire events for actions that occured while the program was not running.
        ///// </summary>
        ///// <param name="clans"></param>
        ///// <param name="wars">Enumeration of both league wars and currentwars.  Include all of this month's CWL wars to avoid false new war notifications.</param>
        ///// <param name="leagueGroups"></param>
        ///// <param name="villages"></param>
        //public void LoadFromDatabase(IEnumerable<ClanApiModel> clans, IEnumerable<ICurrentWarApiModel> wars, IEnumerable<LeagueGroupApiModel> leagueGroups, IEnumerable<VillageApiModel> villages)
        //{
        //    try
        //    {
        //        foreach (ClanApiModel clan in clans)
        //        {
        //            clan.AnnounceWars = true;

        //            AllClans.TryAdd(clan.Tag, clan);
        //        }

        //        foreach (CurrentWarApiModel war in wars.Where(w => w.State != WarState.NotInWar))
        //        {
        //            if (war is LeagueWarApiModel leagueWar)
        //            {
        //                //leagueWar.Initialize();  //todo should i be initializing the saved object?

        //                AllWars.TryAdd(leagueWar.WarTag, leagueWar);
        //            }
        //            //else
        //            //{
        //            //    war.Initialize();
        //            //}

        //            AllWars.TryAdd(war.WarID, war);

        //            foreach (WarClanApiModel warClan in war.Clans)
        //            {
        //                if (AllClans.TryGetValue(warClan.Tag, out ClanApiModel clan))
        //                {
        //                    if (clan.Wars == null) clan.Wars = new Dictionary<string, ICurrentWarApiModel>();

        //                    clan.Wars.TryAdd(war.WarID, war);
        //                }
        //            }
        //        }

        //        foreach (LeagueGroupApiModel leagueGroup in leagueGroups)
        //        {
        //            foreach (LeagueClanApiModel leagueClan in leagueGroup.Clans.EmptyIfNull())
        //            {
        //                if (AllLeagueGroups.TryGetValue(leagueClan.Tag, out LeagueGroupApiModel storedLeagueGroupApiModel))
        //                {
        //                    //the league group already exists.  Lets check if the existing one is from last month
        //                    if (leagueGroup.Season != storedLeagueGroupApiModel.Season && leagueGroup.State != LeagueState.WarsEnded)
        //                    {
        //                        storedLeagueGroupApiModel = leagueGroup;
        //                    }
        //                }
        //                else
        //                {
        //                    AllLeagueGroups.TryAdd(leagueClan.Tag, leagueGroup);
        //                }
        //            }
        //        }

        //        foreach (VillageApiModel village in villages)
        //        {
        //            AllVillages.TryAdd(village.Tag, village);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new CocApiException(e.Message, e);
        //    }
        //}

        /// <summary>
        /// Begin watching a new clan.  This is to add new clans to be watched after your program has started.
        /// </summary>
        /// <param name="clanTag"></param>
        public void WatchClan(string clanTag)
        {
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(clanTag);

                UpdateService clanUpdateService = _updateServices.OrderBy(c => c.ClanStrings.Count).First();

                clanUpdateService.ClanStrings.Add(clanTag);
            }
            catch (CocApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

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
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public ConcurrentBag<WebResponseTimer> GetTimers() => WebResponse.GetTimers();

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
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach(UpdateService updateService in _updateServices)
            {
                updateService.StopUpdatingClans();
            }

            foreach(CancellationTokenSource cancellationTokenSource in _cancellationTokenSources)
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
        /// Check if a string appears to be a SuperCell tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <exception cref="InvalidTagException"></exception>
        public void ThrowIfInvalidTag(string tag)
        {
            if (!IsValidTag(tag))
            {
                Logger.LogWarning(LoggingEvents.InvalidTag, "{source} The provided tag is not valid {tag}", _source, tag);

                throw new InvalidTagException();
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
                    _updateServices.Add(new UpdateService(this, Logger));
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        private void VerifyInitialization()
        {
            if (!_isInitialized || CocApiConfiguration.Tokens == null || CocApiConfiguration.Tokens.Count == 0)
            {
                throw new CocApiException("The library is not initialized, or you did not provide SC Api tokens.");
            }
        }

        private void AddCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Add(cts);
            }
        }

        private void RemoveCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Remove(cts);
            }
        }
    }
}
