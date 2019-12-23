using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Location;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    public delegate Task ClanChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task VillagesJoinedEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    public delegate Task VillagesLeftEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    public delegate Task ClanBadgeUrlChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task ClanLocationChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task NewWarEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarChangedEventHandler(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar);
    public delegate Task NewAttacksEventHandler(ICurrentWarApiModel currentWarApiModel, IReadOnlyList<AttackApiModel> newAttacks);
    public delegate Task WarEndingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarStartingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task ClanVersusPointsChangedEventHandler(ClanApiModel oldClan, int newClanVersusPoints);
    public delegate Task ClanPointsChangedEventHandler(ClanApiModel oldClan, int newClanPoints);
    public delegate Task WarIsAccessibleChangedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndNotSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task VillageChangedEventHandler(VillageApiModel oldVillage, VillageApiModel newVillage);
    public delegate Task VillageDefenseWinsChangedEventHandler(VillageApiModel oldVillage, int newDefenseWins);
    public delegate Task VillageExpLevelChangedEventHandler(VillageApiModel oldVillage, int newExpLevel);
    public delegate Task VillageTrophiesChangedEventHandler(VillageApiModel oldVillage, int newTrophies);
    public delegate Task VillageVersusBattleWinCountChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWinCount);
    public delegate Task VillageVersusBattleWinsChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWins);
    public delegate Task VillageVersusTrophiesChangedEventHandler(VillageApiModel oldVillage, int newVersusTrophies);
    public delegate Task VillageAchievementsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<AchievementApiModel> newAchievements);
    public delegate Task VillageTroopsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newTroops);
    public delegate Task VillageHeroesChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newHeroes);
    public delegate Task VillageSpellsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<VillageSpellApiModel> newSpells);
    public delegate Task WarStartedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroupApiModel leagueGroupApiModel);
    public delegate Task ClanLabelsChangedEventHandler(ClanApiModel newClanApiModel, IReadOnlyList<ClanLabelApiModel> addedLabels, IReadOnlyList<ClanLabelApiModel> removedLables);
    public delegate Task VillageLabelsChangedEventHandler(VillageApiModel newVillageApiModel, IReadOnlyList<VillageLabelApiModel> addedLabels, IReadOnlyList<VillageLabelApiModel> removedLabels);
    public delegate Task VillageReachedLegendsLeagueEventHandler(VillageApiModel villageApiModel);
    public delegate Task ClanDonationsEventHandler(ClanApiModel oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    public delegate Task ClanVillageNameChangedEventHandler(ClanVillageApiModel oldVillage, string newName);
    public delegate Task ClanVillagesLeagueChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<LeagueChange> leagueChanged);
    public delegate Task ClanVillagesRoleChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<RoleChange> roleChanges);
    public delegate Task ClanDonationsResetEventHandler(ClanApiModel oldClan, ClanApiModel newClan);


    public sealed class CocApi : IDisposable
    {
        private volatile bool _isAvailable = true;

        private readonly List<UpdateService> _updateServices = new List<UpdateService>();

        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        //private readonly object _cancellationTokenSourcesLock = new object();

        internal Dictionary<string, ClanApiModel> AllClans { get; } = new Dictionary<string, ClanApiModel>();

        internal Dictionary<string, IWar> AllWarsByClanTag { get; } = new Dictionary<string, IWar>();

        internal Dictionary<string, ICurrentWarApiModel> AllWarsByWarId { get; } = new Dictionary<string, ICurrentWarApiModel>();

        internal Dictionary<string, LeagueWarApiModel> AllWarsByWarTag { get; } = new Dictionary<string, LeagueWarApiModel>();

        internal Dictionary<string, ILeagueGroup> AllLeagueGroups { get; } = new Dictionary<string, ILeagueGroup>();

        internal Dictionary<string, VillageApiModel> AllVillages { get; } = new Dictionary<string, VillageApiModel>();


        //private readonly object _allVillagesLock = new object();

        //private readonly object _allLeagueGroupsLock = new object();

        //private readonly object _allWarsByWarTagLock = new object();

        //private readonly object _allWarsByWarIdLock = new object();

        //private readonly object _allWarsByClanTagLock = new object();

        //internal readonly object _allClansLock = new object();

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Fires if you query the Api during an outage.
        /// If the service is not available, you may still try to query the Api if you wish.
        /// </summary>
        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="ClanApiModel.ClanLevel"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Description"/></description></item>
        ///    <item><description><see cref="ClanApiModel.IsWarLogPublic"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Name"/></description></item>
        ///    <item><description><see cref="ClanApiModel.RequiredTrophies"/>RequiredTrophies</description></item>
        ///    <item><description><see cref="ClanApiModel.Recruitment"/></description></item>
        ///    <item><description><see cref="ClanApiModel.VillageCount"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarLosses"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarWins"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Wars"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarTies"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarFrequency"/></description></item>
        /// </list>
        /// </summary>
        public event ClanChangedEventHandler? ClanChanged;
        public event VillagesJoinedEventHandler? VillagesJoined;
        public event VillagesLeftEventHandler? VillagesLeft;
        public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
        public event ClanLocationChangedEventHandler? ClanLocationChanged;
        public event NewWarEventHandler? NewWar;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///     <item><description><see cref="ICurrentWarApiModel.EndTimeUtc"/></description></item>
        ///     <item><description><see cref="ICurrentWarApiModel.StartTimeUtc"/></description></item>
        ///     <item><description><see cref="ICurrentWarApiModel.State"/></description></item>
        /// 
        /// </list>
        /// </summary>
        public event WarChangedEventHandler? WarChanged;
        public event NewAttacksEventHandler? NewAttacks;
        public event WarEndingSoonEventHandler? WarEndingSoon;
        public event WarStartingSoonEventHandler? WarStartingSoon;
        public event ClanVersusPointsChangedEventHandler? ClanVersusPointsChanged;
        public event ClanPointsChangedEventHandler? ClanPointsChanged;

        /// <summary>
        /// Fires when the donations decrease.
        /// </summary>
        public event ClanDonationsResetEventHandler? ClanDonationsReset;


        /// <summary>
        /// Fires if the war cannot be found from either clanTags or warTag.  Private war logs can also fire this.
        /// </summary>
        public event WarIsAccessibleChangedEventHandler? WarIsAccessibleChanged;
        /// <summary>
        /// Fires when the war is not accessible and the end time has passed.  
        /// This war may still become available if one of the clans does not spin and opens their war log.
        /// </summary>
        public event WarEndNotSeenEventHandler? WarEndNotSeen;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="VillageApiModel.AttackWins"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BestTrophies"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BestVersusTrophies"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BuilderHallLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.TownHallLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.TownHallWeaponLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.WarStars"/></description></item>
        /// </list>
        /// </summary>
        public event VillageChangedEventHandler? VillageChanged;
        public event VillageDefenseWinsChangedEventHandler? VillageDefenseWinsChanged;
        //public event VillageDonationsChangedEventHandler? VillageDonationsChanged;
        //public event VillageDonationsReceivedChangedEventHandler? VillageDonationsReceivedChanged;
        public event VillageExpLevelChangedEventHandler? VillageExpLevelChanged;
        public event VillageTrophiesChangedEventHandler? VillageTrophiesChanged;
        public event VillageVersusBattleWinCountChangedEventHandler? VillageVersusBattleWinCountChanged;
        public event VillageVersusBattleWinsChangedEventHandler? VillageVersusBattleWinsChanged;
        public event VillageVersusTrophiesChangedEventHandler? VillageVersusTrophiesChanged;
        public event VillageAchievementsChangedEventHandler? VillageAchievementsChanged;
        public event VillageTroopsChangedEventHandler? VillageTroopsChanged;
        public event VillageHeroesChangedEventHandler? VillageHeroesChanged;
        public event VillageSpellsChangedEventHandler? VillageSpellsChanged;
        public event WarStartedEventHandler? WarStarted;
        /// <summary>
        /// Fires when the <see cref="ICurrentWarApiModel.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        /// </summary>
        public event WarEndedEventHandler? WarEnded;
        /// <summary>
        /// Fires when the Api shows <see cref="ICurrentWarApiModel.State"/> is <see cref="Enums.WarState.WarEnded"/>
        /// </summary>
        public event WarEndSeenEventHandler? WarEndSeen;
        /// <summary>
        /// Fires when any clan in a league group has more than 15 attacks.
        /// </summary>
        public event LeagueGroupTeamSizeChangedEventHandler? LeagueGroupTeamSizeChanged;
        public event ClanLabelsChangedEventHandler? ClanLabelsChanged;
        public event VillageLabelsChangedEventHandler? VillageLabelsChanged;
        public event VillageReachedLegendsLeagueEventHandler? VillageReachedLegendsLeague;
        public event ClanDonationsEventHandler? ClanDonations;
        public event ClanVillageNameChangedEventHandler? ClanVillageNameChanged;
        public event ClanVillagesLeagueChangedEventHandler? ClanVillagesLeagueChanged;
        public event ClanVillagesRoleChangedEventHandler? ClanVillagesRoleChanged;
        /// <summary>
        /// Fires when an update task encounters an error.  Recommended fix action is to <see cref="StartUpdatingClans()"/> or restart.
        /// </summary>


        

        public Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public ILogger? Logger { get; set; }


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
        public void Initialize(CocApiConfiguration cfg, ILogger? logger)
        {            
            Logger = logger;

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

        internal void CrashDetectedEvent()
        {
            try
            {
                Task.Run(async () =>
                {
                    //wait to allow the updater to finish crashing
                    await Task.Delay(5000).ConfigureAwait(false);

                    StartUpdatingClans();

                    Logger.LogInformation(LoggingEvents.None, "{source} Update services restarted.", _source);
                });    
            }
            catch (Exception e)
            { 
                Logger.LogWarning(LoggingEvents.UnhandledError, "{source} {message}", _source, e.Message);
            }
        }

        internal void ClanDonationsResetEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanDonationsReset?.Invoke(oldClan, newClan);
        }

        internal void ClanVillagesRoleChangedEvent(ClanApiModel clan, List<RoleChange> roleChanges)
        {
            if (roleChanges.Count > 0)
            {

                ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
            }
        }

        internal void ClanVillagesLeagueChangedEvent(ClanApiModel oldClan, List<LeagueChange> leagueChanged)
        {
            if (leagueChanged.Count > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(oldClan, leagueChanged.ToImmutableArray());

            }
        }

        internal void ClanVillageNameChangedEvent(ClanVillageApiModel oldVillage, string newName)
        {
            ClanVillageNameChanged?.Invoke(oldVillage, newName);
        }

        internal void ClanDonationsEvent(ClanApiModel oldClan, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonations?.Invoke(oldClan, received.ToImmutableArray(), donated.ToImmutableArray());
            }
        }

        internal void VillageReachedLegendsLeagueEvent(VillageApiModel villageApiModel)
        {
            VillageReachedLegendsLeague?.Invoke(villageApiModel);
        }

        internal void ClanLabelsChangedEvent(ClanApiModel newClan, List<ClanLabelApiModel> addedLabels, List<ClanLabelApiModel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            ClanLabelsChanged?.Invoke(newClan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        internal void VillageLabelsChangedEvent(VillageApiModel newVillage, List<VillageLabelApiModel> addedLabels, List<VillageLabelApiModel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            VillageLabelsChanged?.Invoke(newVillage, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());

        }

        internal void LeagueGroupTeamSizeChangedEvent(LeagueGroupApiModel leagueGroupApiModel)
        {
            LeagueGroupTeamSizeChanged?.Invoke(leagueGroupApiModel);
        }

        internal void WarEndSeenEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndSeen?.Invoke(currentWarApiModel);
        }

        internal void WarEndedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEnded?.Invoke(currentWarApiModel);
        }

        internal void WarStartedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarStarted?.Invoke(currentWarApiModel);
        }

        internal void VillageSpellsChangedEvent(VillageApiModel oldVillage, List<VillageSpellApiModel> newSpells)
        {
            VillageSpellsChanged?.Invoke(oldVillage, newSpells.ToImmutableArray());
        }

        internal void VillageHeroesChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newHeroes)
        {
            VillageHeroesChanged?.Invoke(oldVillage, newHeroes.ToImmutableArray());
        }

        internal void VillageTroopsChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newTroops)
        {
            VillageTroopsChanged?.Invoke(oldVillage, newTroops.ToImmutableArray());
        }

        internal void VillageAchievementsChangedEvent(VillageApiModel oldVillage, List<AchievementApiModel> newAchievements)
        {
            VillageAchievementsChanged?.Invoke(oldVillage, newAchievements.ToImmutableArray());
        }

        internal void VillageVersusTrophiesChangedEvent(VillageApiModel oldVillage, int newVersusTrophies)
        {
            VillageVersusTrophiesChanged?.Invoke(oldVillage, newVersusTrophies);
        }

        internal void VillageVersusBattleWinsChangedEvent(VillageApiModel oldVillage, int newVersusBattleWins)
        {
            VillageVersusBattleWinsChanged?.Invoke(oldVillage, newVersusBattleWins);
        }

        internal void VillageVersusBattleWinCountChangedEvent(VillageApiModel oldVillage, int newVersusBattleWinCount)
        {
            VillageVersusBattleWinCountChanged?.Invoke(oldVillage, newVersusBattleWinCount);
        }

        internal void VillageTrophiesChangedEvent(VillageApiModel oldVillage, int newTrophies)
        {
            VillageTrophiesChanged?.Invoke(oldVillage, newTrophies);
        }

        internal void VillageExpLevelChangedEvent(VillageApiModel oldVillage, int newExpLevel)
        {
            VillageExpLevelChanged?.Invoke(oldVillage, newExpLevel);
        }

        internal void VillageDefenseWinsChangedEvent(VillageApiModel oldVillage, int newDefenseWinsChanged)
        {
            VillageDefenseWinsChanged?.Invoke(oldVillage, newDefenseWinsChanged);
        }

        internal void VillageChangedEvent(VillageApiModel oldVillage, VillageApiModel newVillage)
        {
            VillageChanged?.Invoke(oldVillage, newVillage);
        }

        internal void WarEndNotSeenEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndNotSeen?.Invoke(currentWarApiModel);
        }

        internal void WarIsAccessibleChangedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarIsAccessibleChanged?.Invoke(currentWarApiModel);
        }

        internal void ClanPointsChangedEvent(ClanApiModel oldClan, int newClanPoints)
        {
            ClanPointsChanged?.Invoke(oldClan, newClanPoints);
        }

        internal void ClanVersusPointsChangedEvent(ClanApiModel oldClan, int newClanVersusPoints)
        {
            ClanVersusPointsChanged?.Invoke(oldClan, newClanVersusPoints);
        }

        internal void WarStartingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarStartingSoon?.Invoke(currentWarApiModel);
        }

        internal void WarEndingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndingSoon?.Invoke(currentWarApiModel);
        }

        internal void NewAttacksEvent(ICurrentWarApiModel currentWarApiModel, List<AttackApiModel> attackApiModels)
        {
            if (attackApiModels.Count > 0)
            {
                NewAttacks?.Invoke(currentWarApiModel, attackApiModels.ToImmutableArray());
            }
        }

        internal void WarChangedEvent(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar)
        {
            WarChanged?.Invoke(oldWar, newWar);
        }

        internal void NewWarEvent(ICurrentWarApiModel currentWarApiModel)
        {
            NewWar?.Invoke(currentWarApiModel);
        }

        internal void VillagesLeftEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesLeft?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }            
        }

        internal void ClanLocationChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanLocationChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanBadgeUrlChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanBadgeUrlChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanChanged?.Invoke(oldClan, newClan);
        }

        internal void VillagesJoinedEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesJoined?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }
        }




        //public async Task<IDownloadable> GetAsync<TResult>(string url, EndPoint endPoint, CancellationTokenSource? cancellationTokenSource = null) where TResult : class, IDownloadable, new()
        //{
        //    using CancellationTokenSource cts = GetCancellationTokenSource();

        //    cancellationTokenSource?.Token.Register(() => cts.Cancel());

        //    IDownloadable result = await WebResponse.DownloadAsync<TResult>(endPoint, url, cts);

        //    RemoveCancellationTokenSource(cts);

        //    return result;
        //}

        public async Task<IDownloadable> GetAsync<TResult>(string url, EndPoint endPoint, CancellationToken? cancellationToken = null) where TResult : class, IDownloadable, new()
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

            if (cancellationToken == null) cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

            AddCancellationTokenSource(cts);

            IDownloadable result = await WebResponse.GetIDownloadableAsync<TResult>(endPoint, url, cts.Token).ConfigureAwait(false);

            RemoveCancellationTokenSource(cts);

            return result;
        }



        public async Task<ClanApiModel> GetClanAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();
            
            try
            {
                ThrowIfInvalidTag(clanTag);

                ClanApiModel? clan = GetClanOrDefault(clanTag);

                if (clan != null && (allowExpiredItem || clan.IsExpired() == false)) return clan;                    

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                clan = (ClanApiModel) await GetAsync<ClanApiModel>(url, EndPoint.Clan, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses) return clan;
                
                lock (AllClans)
                {
                    AllClans[clan.ClanTag] = clan;
                }                

                return clan;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<IWar> GetCurrentWarAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {         
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(clanTag);

                IWar? war = GetWarByClanTagOrDefault(clanTag);

                if (war != null && (allowExpiredItem || !war.IsExpired())) return war;

                if (war is ICurrentWarApiModel currentWar && currentWar.StartTimeUtc > DateTime.UtcNow) return currentWar;

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                IDownloadable downloadable = await GetAsync<CurrentWarApiModel>(url, EndPoint.CurrentWar, cancellationToken).ConfigureAwait(false);

                if (downloadable is NotInWar notInWar)
                {
                    if (CocApiConfiguration.CacheHttpResponses)
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clanTag] = notInWar;
                        }
                    }

                    return notInWar;
                }

                ICurrentWarApiModel downloadedWar = (ICurrentWarApiModel) downloadable;

                if (!CocApiConfiguration.CacheHttpResponses) return downloadedWar;
                
                foreach(var clan in downloadedWar.Clans)
                {
                    AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar storedWar, AllWarsByClanTag);

                    if (storedWar == null || storedWar.CacheExpiresAtUtc < downloadedWar.CacheExpiresAtUtc)
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clan.ClanTag] = downloadedWar;
                        }
                    }

                    if (AllClans.TryGetValue(clan.ClanTag, out ClanApiModel storedClan, AllClans))
                    {
                        storedClan.Wars.TryAdd(downloadedWar.WarId, downloadedWar, storedClan.Wars);
                    }
                }

                lock (AllWarsByWarId)
                {
                    AllWarsByWarId[downloadedWar.WarId] = downloadedWar;
                }                                                        

                return downloadedWar;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// Returns <see cref="LeagueGroupApiModel"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ILeagueGroup> GetLeagueGroupAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

            try
            {
                ThrowIfInvalidTag(clanTag);

                ILeagueGroup? leagueGroup = GetLeagueGroupOrDefault(clanTag);

                if (leagueGroup != null && (allowExpiredItem || !leagueGroup.IsExpired())) return leagueGroup;
                
                IDownloadable downloadable = await GetAsync<LeagueGroupApiModel>(url, EndPoint.LeagueGroup, cancellationToken).ConfigureAwait(false);

                if (downloadable is LeagueGroupNotFound notFound)
                {
                    lock (AllLeagueGroups)
                    {
                        AllLeagueGroups[clanTag] = notFound;
                    }

                    return notFound;
                }

                if (!(downloadable is LeagueGroupApiModel leagueGroupApiModel)) throw new CocApiException("Unknown Type");

                if (!CocApiConfiguration.CacheHttpResponses) return leagueGroupApiModel;

                foreach(var clan in leagueGroupApiModel.Clans.EmptyIfNull())
                {
                    if (AllLeagueGroups.TryAdd(clan.ClanTag, leagueGroupApiModel, AllLeagueGroups)) continue;

                    if (!AllLeagueGroups.TryGetValue(clan.ClanTag, out ILeagueGroup storedLeagueGroup, AllLeagueGroups)) continue;

                    //the league group already exists.  Lets check if the existing one is from last month
                    if (storedLeagueGroup is LeagueGroupApiModel storedLeagueGroupApiModel && leagueGroupApiModel.Season > storedLeagueGroupApiModel.Season)
                    {
                        lock (AllLeagueGroups)
                        {
                            AllLeagueGroups[clan.ClanTag] = storedLeagueGroupApiModel;
                        }
                    }
                }

                return leagueGroupApiModel;
            }

            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<LeagueWarApiModel> GetLeagueWarAsync(string warTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(warTag);

                LeagueWarApiModel? leagueWar = GetLeagueWarOrDefault(warTag);

                if (leagueWar != null)
                {
                    if (allowExpiredItem || !leagueWar.IsExpired()) return leagueWar;

                    if (leagueWar.State == WarState.WarEnded) return leagueWar;

                    if (leagueWar.StartTimeUtc > DateTime.UtcNow) return leagueWar;
                }

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                LeagueWarApiModel leagueWarApiModel = (LeagueWarApiModel) await GetAsync<LeagueWarApiModel>(url, EndPoint.LeagueWar, cancellationToken).ConfigureAwait(false);

                leagueWarApiModel.WarTag = warTag;

                leagueWarApiModel.WarType = WarType.SCCWL;

                if (!CocApiConfiguration.CacheHttpResponses) return leagueWarApiModel;

                lock (AllWarsByWarTag)
                {
                    AllWarsByWarTag[leagueWarApiModel.WarTag] = leagueWarApiModel;
                }

                lock (AllWarsByWarId)
                {
                    AllWarsByWarId[leagueWarApiModel.WarId] = leagueWarApiModel;
                }

                foreach(var clan in leagueWarApiModel.Clans)
                {
                    if (AllClans.TryGetValue(clan.ClanTag, out ClanApiModel storedClan, AllClans))
                    {
                        storedClan.Wars.TryAdd(leagueWarApiModel.WarId, leagueWarApiModel, storedClan.Wars);
                    }
                }

                foreach(var clan in leagueWarApiModel.Clans)
                {
                    if (AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar war, AllWarsByClanTag))
                    {
                        if (war is NotInWar || leagueWarApiModel.State == WarState.InWar)
                        {
                            lock (AllWarsByClanTag)
                            {
                                AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                            }
                        }
                        else if (war is ICurrentWarApiModel currentWar && (DateTime.UtcNow > currentWar.EndTimeUtc && DateTime.UtcNow < leagueWarApiModel.EndTimeUtc))
                        {
                            lock (AllWarsByClanTag)
                            {
                                AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                            }
                        }
                    }
                    else
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                        }
                    }
                }

                return leagueWarApiModel;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<VillageApiModel> GetVillageAsync(string villageTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(villageTag);

                VillageApiModel? villageApiModel = GetVillageOrDefault(villageTag);

                if (villageApiModel != null && (allowExpiredItem || !villageApiModel.IsExpired())) return villageApiModel;

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                villageApiModel = (VillageApiModel) await GetAsync<VillageApiModel>(url, EndPoint.Village, cancellationToken).ConfigureAwait(false);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    lock (AllVillages)
                    {
                        AllVillages[villageTag] = villageApiModel;
                    }
                }

                return villageApiModel;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<WarLogEntryModel>> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/";

                url = $"{url}{Uri.EscapeDataString(clanTag)}/warlog?";

                if (limit != null)
                {
                    url = $"{url}limit={limit}&";
                }

                if (after != null)
                {
                    url = $"{url}after={after}&";
                }

                if (before != null)
                {
                    url = $"{url}before={before}&";
                }

                if (url.EndsWith("&"))
                {
                    url = url[0..^1];
                }

                if (url.EndsWith("?"))
                {
                    url = url[0..^1];
                }

                return (PaginatedApiModel<WarLogEntryModel>) await GetAsync<PaginatedApiModel<WarLogEntryModel>>(url, EndPoint.WarLog, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<ClanApiModel>> GetClansAsync(string? clanName = null
                                                        , WarFrequency? warFrequency = null
                                                        , int? locationId = null
                                                        , int? minVillages = null
                                                        , int? maxVillages = null
                                                        , int? minClanPoints = null
                                                        , int? minClanLevel = null
                                                        , int? limit = null
                                                        , int? after = null
                                                        , int? before = null
                                                        , CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                if (!string.IsNullOrEmpty(clanName) && clanName.Length < 3)
                {
                    throw new ArgumentException("The clan name must be longer than three characters.");
                }

                string url = $"https://api.clashofclans.com/v1/clans?";

                if (clanName != null)
                {
                    url = $"{url}name={Uri.EscapeDataString(clanName)}&";
                }
                if (warFrequency != null)
                {
                    url = $"{url}warFrequency={warFrequency.ToString()}&";
                }
                if (locationId != null)
                {
                    url = $"{url}locationId={locationId}&";
                }
                if (minVillages != null)
                {
                    url = $"{url}minMembers={minVillages}&";
                }
                if (maxVillages != null)
                {
                    url = $"{url}maxMembers={maxVillages}&";
                }
                if (minClanPoints != null)
                {
                    url = $"{url}minClanPoints={minClanPoints}&";
                }
                if (minClanLevel != null)
                {
                    url = $"{url}minClanLevel={minClanLevel}&";
                }
                if (limit != null)
                {
                    url = $"{url}limit={limit}&";
                }
                if (after != null)
                {
                    url = $"{url}after={after}&";
                }
                if (before != null)
                {
                    url = $"{url}before={before}&";
                }

                if (url.EndsWith("&"))
                {
                    url = url[0..^1];
                }

                return (PaginatedApiModel<ClanApiModel>) await GetAsync<PaginatedApiModel<ClanApiModel>>(url, EndPoint.Clans, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<VillageLeagueApiModel>> GetVillageLeaguesAsync(CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/leagues?limit=500";

                return (PaginatedApiModel<VillageLeagueApiModel>) await GetAsync<PaginatedApiModel<VillageLeagueApiModel>>(url, EndPoint.VillageLeagues, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<LocationApiModel>> GetLocationsAsync(CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations?limit=10000";

                return (PaginatedApiModel<LocationApiModel>) await GetAsync<PaginatedApiModel<LocationApiModel>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }        
        
        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<TopMainClan>> GetTopMainClansAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/clans";

                return (PaginatedApiModel<TopMainClan>) await GetAsync<PaginatedApiModel<TopMainClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);  //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<PaginatedApiModel<TopBuilderClan>> GetTopBuilderClansAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/clans-versus";

                return (PaginatedApiModel<TopBuilderClan>) await GetAsync<PaginatedApiModel<TopBuilderClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);  //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }
        
        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<PaginatedApiModel<TopMainVillage>> GetTopMainVillagesAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/players";

                return (PaginatedApiModel<TopMainVillage>) await GetAsync<PaginatedApiModel<TopMainVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<PaginatedApiModel<TopBuilderVillage>> GetTopBuilderVillagesAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/players-versus";

                return (PaginatedApiModel<TopBuilderVillage>) await GetAsync<PaginatedApiModel<TopBuilderVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<PaginatedApiModel<LabelApiModel>> GetClanLabelsAsync(CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/clans?limit=10000";

                return (PaginatedApiModel<LabelApiModel>) await GetAsync<PaginatedApiModel<LabelApiModel>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<PaginatedApiModel<LabelApiModel>> GetVillageLabelsAsync(CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/players?limit=10000";

                return (PaginatedApiModel<LabelApiModel>) await GetAsync<PaginatedApiModel<LabelApiModel>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }













        /// <summary>
        /// Returns null if the clanTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ClanApiModel?> GetClanOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            ClanApiModel? result = null;

            try
            {
                result = await GetClanAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This will not throw a <see cref="ServerResponseException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<ClanApiModel>?> GetClansOrDefaultAsync(string? clanName = null
                                                        , WarFrequency? warFrequency = null
                                                        , int? locationId = null
                                                        , int? minVillages = null
                                                        , int? maxVillages = null
                                                        , int? minClanPoints = null
                                                        , int? minClanLevel = null
                                                        , int? limit = null
                                                        , int? after = null
                                                        , int? before = null
                                                        , CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            PaginatedApiModel<ClanApiModel>? result = null;

            try
            {
                result = await GetClansAsync(clanName, warFrequency, locationId, minVillages, maxVillages, minClanPoints, minClanLevel, limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            //catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the clan is not in a league.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<ILeagueGroup?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            ILeagueGroup? result = null;

            try
            {
                result = await GetLeagueGroupAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns null if the war log is private.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<IWar?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            IWar? result = null;

            try
            {
                result = await GetCurrentWarAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the warTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="warTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<LeagueWarApiModel?> GetLeagueWarOrDefaultAsync(string warTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            LeagueWarApiModel? result = null;

            try
            {
                result = await GetLeagueWarAsync(warTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the villageTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<VillageApiModel?> GetVillageOrDefaultAsync(string villageTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VillageApiModel? result = null;

            try
            {
                result = await GetVillageAsync(villageTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the clan is not in a league.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<WarLogEntryModel>?> GetWarLogOrDefaultAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<WarLogEntryModel>? result = null;

            try
            {
                result = await GetWarLogAsync(clanTag, limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns the most recent download available for the given war.  Will use both clanTags when the war log is private.  Returns null or the most recent <see cref="ICurrentWarApiModel"/> available.
        /// </summary>
        /// <param name="storedWar"></param>
        /// <returns></returns>
        public async Task<ICurrentWarApiModel?> GetCurrentWarOrDefaultAsync(ICurrentWarApiModel storedWar)
        {
            try
            {
                ICurrentWarApiModel? warByWarId = GetWarByWarIdOrDefault(storedWar.WarId);

                if (warByWarId?.IsExpired() == false) return warByWarId;

                if (warByWarId?.State == WarState.WarEnded) return warByWarId;

                if (warByWarId?.StartTimeUtc > DateTime.UtcNow) return warByWarId;

                IWar? war = null;

                if (storedWar is LeagueWarApiModel leagueWar)
                {
                    return await GetLeagueWarOrDefaultAsync(leagueWar.WarTag, allowExpiredItem: false).ConfigureAwait(false);
                }
                else
                {
                    foreach (var clan in storedWar.Clans)
                    {
                        war = await GetCurrentWarOrDefaultAsync(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);

                        if (war is ICurrentWarApiModel currentWar1 && currentWar1?.WarId == storedWar.WarId) return currentWar1;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<LabelApiModel>?> GetClanLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<LabelApiModel>? result = null;

            try
            {
                return await GetClanLabelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<LabelApiModel>?> GetVillageLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<LabelApiModel>? result = null;

            try
            {
                result = await GetVillageLabelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<VillageLeagueApiModel>?> GetVillageLeaguesOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<VillageLeagueApiModel>? result = null;

            try
            {
                result = await GetVillageLeaguesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }















        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clanTag is not found.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ClanApiModel? GetClanOrDefault(string clanTag)
        {
            AllClans.TryGetValue(clanTag, out ClanApiModel? result, AllClans);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the war log is private.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public IWar? GetWarByClanTagOrDefault(string clanTag)
        {
            AllWarsByClanTag.TryGetValue(clanTag, out IWar? result, AllWarsByClanTag);

            return result;
        }

        public ICurrentWarApiModel? GetWarByWarIdOrDefault(string warId)
        {
            AllWarsByWarId.TryGetValue(warId, out ICurrentWarApiModel? currentWar, AllWarsByWarId);

            return currentWar;
        }
 

        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clan is not in a clan is not found to be in a league.  
        /// Returns <see cref="LeagueGroupApiModel"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ILeagueGroup? GetLeagueGroupOrDefault(string clanTag)
        {
            AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup? result, AllLeagueGroups);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the warTag is not found.
        /// </summary>
        /// <param name="warTag"></param>
        /// <returns></returns>
        public LeagueWarApiModel? GetLeagueWarOrDefault(string warTag)
        {
            AllWarsByWarTag.TryGetValue(warTag, out LeagueWarApiModel? result, AllWarsByWarTag);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the villageTag is not found.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <returns></returns>
        public VillageApiModel? GetVillageOrDefault(string villageTag)
        {            
            AllVillages.TryGetValue(villageTag, out VillageApiModel? result, AllVillages);

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
