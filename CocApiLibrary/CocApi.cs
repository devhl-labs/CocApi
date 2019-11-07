using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Text.RegularExpressions;
using System.Linq;

using CocApiLibrary.Models;
using CocApiLibrary.Exceptions;
using static CocApiLibrary.Enums;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace CocApiLibrary
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);
    public delegate void ClanChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate void VillagesJoinedEventHandler(ClanApiModel oldClan, List<ClanVillageApiModel> villageListApiModels);
    public delegate void VillagesLeftEventHandler(ClanApiModel oldClan, List<ClanVillageApiModel> villageListApiModels);
    public delegate void ClanBadgeUrlChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate void ClanLocationChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate void NewWarEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void WarChangedEventHandler(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar);
    public delegate void NewAttacksEventHandler(ICurrentWarApiModel currentWarApiModel, List<AttackApiModel> attackApiModels);
    public delegate void WarEndingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void WarStartingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void ClanVersusPointsChangedEventHandler(ClanApiModel oldClan, int newClanVersusPoints);
    public delegate void ClanPointsChangedEventHandler(ClanApiModel oldClan, int newClanPoints);
    public delegate void WarIsAccessibleChangedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void WarEndNotSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void VillageChangedEventHandler(VillageApiModel oldVillage, VillageApiModel newVillage);
    public delegate void VillageDefenseWinsChangedEventHandler(VillageApiModel oldVillage, int newDefenseWins);
    //public delegate void VillageDonationsChangedEventHandler(VillageApiModel oldVillage, int newDonations);
    //public delegate void VillageDonationsReceivedChangedEventHandler(VillageApiModel oldVillage, int newDonationsReceived);
    public delegate void VillageExpLevelChangedEventHandler(VillageApiModel oldVillage, int newExpLevel);
    public delegate void VillageTrophiesChangedEventHandler(VillageApiModel oldVillage, int newTrophies);
    public delegate void VillageVersusBattleWinCountChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWinCount);
    public delegate void VillageVersusBattleWinsChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWins);
    public delegate void VillageVersusTrophiesChangedEventHandler(VillageApiModel oldVillage, int newVersusTrophies);
    public delegate void VillageLeagueChangedEventHandler(VillageApiModel oldVillage, VillageLeagueApiModel? newLeague);
    public delegate void VillageAchievementsChangedEventHandler(VillageApiModel oldVillage, List<AchievementApiModel> newAchievements);
    public delegate void VillageTroopsChangedEventHandler(VillageApiModel oldVillage, List<TroopApiModel> newTroops);
    public delegate void VillageHeroesChangedEventHandler(VillageApiModel oldVillage, List<TroopApiModel> newHeroes);
    public delegate void VillageSpellsChangedEventHandler(VillageApiModel oldVillage, List<VillageSpellApiModel> newSpells);
    public delegate void WarStartedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void WarEndedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void WarEndSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate void LeagueGroupTeamSizeChangeDetectedEventHandler(LeagueGroupApiModel leagueGroupApiModel);
    public delegate void ClanLabelsRemovedEventHandler(ClanApiModel newClanApiModel, IEnumerable<ClanLabelApiModel> labelApiModels);
    public delegate void ClanLabelsAddedEventHandler(ClanApiModel newClanApiModel, IEnumerable<ClanLabelApiModel> labelApiModels);
    public delegate void VillageLabelsRemovedEventHandler(VillageApiModel newVillageApiModel, IEnumerable<VillageLabelApiModel> labelApiModels);
    public delegate void VillageLabelsAddedEventHandler(VillageApiModel newVillageApiModel, IEnumerable<VillageLabelApiModel> labelApiModels);
    public delegate void VillageReachedLegendsLeagueEventHandler(VillageApiModel villageApiModel);
    public delegate void ClanDonationsEventHandler(Dictionary<string, Tuple<ClanVillageApiModel, int>> receivedDonations, Dictionary<string, Tuple<ClanVillageApiModel, int>> gaveDonations);
    public delegate void ClanVillageNameChangedEventHandler(ClanVillageApiModel oldVillage, string newName);
    public delegate void ClanVillagesLeagueChangedEventHandler(Dictionary<string, Tuple<ClanVillageApiModel, VillageLeagueApiModel>> leagueChanged);
    public delegate void ClanVillagesRoleChangedEventHandler(Dictionary<string, Tuple<ClanVillageApiModel, Role>> roleChanges);
    public delegate void ClanDonationsResetEventHandler(ClanApiModel oldClan, ClanApiModel newClan);


    public sealed class CocApi : IDisposable
    {
        private bool? _isAvailable;
        private readonly System.Timers.Timer _testConnection = new System.Timers.Timer();
        private readonly List<UpdateService> _updateServices = new List<UpdateService>();
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        internal Dictionary<string, ClanApiModel> AllClans { get; } = new Dictionary<string, ClanApiModel>();
        internal Dictionary<string, IWar> AllWarsByClanTag { get; } = new Dictionary<string, IWar>();
        internal Dictionary<string, IWar> AllWarsByWarId { get; } = new Dictionary<string, IWar>();
        internal Dictionary<string, LeagueWarApiModel> AllWarsByWarTag { get; } = new Dictionary<string, LeagueWarApiModel>();
        internal Dictionary<string, ILeagueGroup> AllLeagueGroups { get; } = new Dictionary<string, ILeagueGroup>();
        internal Dictionary<string, VillageApiModel> AllVillages { get; } = new Dictionary<string, VillageApiModel>();
        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Fires if you query the Api during an outage.  
        /// The Api will be polled every five seconds to see when service is restored. 
        /// If the service is not available, you may still try to query the Api if you wish.
        /// </summary>
        public event IsAvailableChangedEventHandler? IsAvailableChanged;
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
        public event VillageLeagueChangedEventHandler? VillageLeagueChanged;
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
        public event LeagueGroupTeamSizeChangeDetectedEventHandler? LeagueGroupTeamSizeChangeDetected;
        public event ClanLabelsAddedEventHandler? ClanLabelsAdded;
        public event ClanLabelsRemovedEventHandler? ClanLabelsRemoved;
        public event VillageLabelsAddedEventHandler? VillageLabelsAdded;
        public event VillageLabelsRemovedEventHandler? VillageLabelsRemoved;
        public event VillageReachedLegendsLeagueEventHandler? VillageReachedLegendsLeague;
        public event ClanDonationsEventHandler? ClanDonations;
        public event ClanVillageNameChangedEventHandler? ClanVillageNameChanged;
        public event ClanVillagesLeagueChangedEventHandler? ClanVillagesLeagueChanged;
        public event ClanVillagesRoleChangedEventHandler? ClanVillagesRoleChanged;


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

        public bool? IsAvailable
        {
            get { return _isAvailable; }
            internal set
            {
                if (_isAvailable != value && value != null)
                {
                    _isAvailable = value;
                    IsAvailableChanged?.Invoke(value.Value);

                    if (_isAvailable == false)
                    {
                        _testConnection.Interval = 5000;
                        _testConnection.Elapsed += TestConnection_Elapsed;
                        _testConnection.AutoReset = true;
                        _testConnection.Enabled = true;
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

            if (cfg == null || cfg.Tokens.Count() == 0)
            {
                throw new CocApiException("You did not provide any tokens to access the SC Api.");
            }

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            CreateUpdaters();

            _isInitialized = true;
        }


        internal void ClanDonationsResetEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanDonationsReset?.Invoke(oldClan, newClan);
        }
        
        internal void ClanVillagesRoleChangedEvent(Dictionary<string, Tuple<ClanVillageApiModel, Role>> roleChanges)
        {
            if (roleChanges.Count() > 0)
            {
                ClanVillagesRoleChanged?.Invoke(roleChanges);
            }
        }

        internal void ClanVillagesLeagueChangedEvent(Dictionary<string, Tuple<ClanVillageApiModel, VillageLeagueApiModel>> leagueChanged)
        {
            if (leagueChanged.Count() > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(leagueChanged);
            }
        }

        internal void ClanVillageNameChangedEvent(ClanVillageApiModel oldVillage, string newName)
        {
            ClanVillageNameChanged?.Invoke(oldVillage, newName);
        }

        internal void ClanDonationsEvent(Dictionary<string, Tuple<ClanVillageApiModel, int>> receivedDonations, Dictionary<string, Tuple<ClanVillageApiModel, int>> gaveDonations)
        {
            if (receivedDonations.Count() > 0 || gaveDonations.Count() > 0)
            {
                ClanDonations?.Invoke(receivedDonations, gaveDonations);
            }
        }

        internal void VillageReachedLegendsLeagueEvent(VillageApiModel villageApiModel)
        {
            VillageReachedLegendsLeague?.Invoke(villageApiModel);
        }

        internal void VillageLabelsRemovedEvent(VillageApiModel newVillage, IEnumerable<VillageLabelApiModel> labelApiModels)
        {
            if (labelApiModels != null && labelApiModels.Count() > 0)
            {
                VillageLabelsRemoved?.Invoke(newVillage, labelApiModels);
            }
        }

        internal void VillageLabelsAddedEvent(VillageApiModel newVillage, IEnumerable<VillageLabelApiModel> labelApiModels)
        {
            if (labelApiModels != null && labelApiModels.Count() > 0)
            {
                VillageLabelsAdded?.Invoke(newVillage, labelApiModels);
            }
        }

        internal void ClanLabelsRemovedEvent(ClanApiModel newClan, IEnumerable<ClanLabelApiModel> labelApiModels)
        {
            if (labelApiModels != null && labelApiModels.Count() > 0)
            {
                ClanLabelsRemoved?.Invoke(newClan, labelApiModels);
            }
        }

        internal void ClanLabelsAddedEvent(ClanApiModel newClan, IEnumerable<ClanLabelApiModel> labelApiModels)
        {
            if (labelApiModels != null && labelApiModels.Count() > 0)
            {
                ClanLabelsAdded?.Invoke(newClan, labelApiModels);
            }
        }

        internal void LeagueGroupTeamSizeChangeDetectedEvent(LeagueGroupApiModel leagueGroupApiModel)
        {
            LeagueGroupTeamSizeChangeDetected?.Invoke(leagueGroupApiModel);
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
            VillageSpellsChanged?.Invoke(oldVillage, newSpells);
        }

        internal void VillageHeroesChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newHeroes)
        {
            VillageHeroesChanged?.Invoke(oldVillage, newHeroes);
        }

        internal void VillageTroopsChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newTroops)
        {
            VillageTroopsChanged?.Invoke(oldVillage, newTroops);
        }

        internal void VillageAchievementsChangedEvent(VillageApiModel oldVillage, List<AchievementApiModel> newAchievements)
        {
            VillageAchievementsChanged?.Invoke(oldVillage, newAchievements);
        }

        internal void VillageLeagueChangedEvent(VillageApiModel oldVillage, VillageLeagueApiModel? newLeague)
        {
            VillageLeagueChanged?.Invoke(oldVillage, newLeague);
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

        //internal void VillageDonationsReceivedChangedEvent(VillageApiModel oldVillage, int newDonationsReceived)
        //{
        //    VillageDonationsReceivedChanged?.Invoke(oldVillage, newDonationsReceived);
        //}

        //internal void VillageDonationsChangedEvent(VillageApiModel oldVillage, int newDonations)
        //{
        //    VillageDonationsChanged?.Invoke(oldVillage, newDonations);
        //}

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
            if (attackApiModels.Count() > 0)
            {
                NewAttacks?.Invoke(currentWarApiModel, attackApiModels);
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
            if (clanVillageApiModels.Count() > 0)
            {
                VillagesLeft?.Invoke(newClan, clanVillageApiModels);
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
            if (clanVillageApiModels.Count() > 0)
            {
                VillagesJoined?.Invoke(newClan, clanVillageApiModels);
            }
        }














        public async Task<ClanApiModel> GetClanAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if (AllClans.TryGetValue(clanTag, out ClanApiModel storedClan))
                    {
                        if ((storedClan.CacheExpiresAtUtc != null && storedClan.CacheExpiresAtUtc > DateTime.UtcNow) || !storedClan.IsExpired() || allowExpiredItem)
                        {
                            return storedClan;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                ClanApiModel downloadedClan = (ClanApiModel) await WebResponse.GetWebResponse<ClanApiModel>(EndPoint.Clan, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    if (!AllClans.TryAdd(downloadedClan.ClanTag, downloadedClan) && !_updateServices.Any(c => c.ClanStrings.Any(t => t == downloadedClan.ClanTag)))
                    {
                        //clan is not watched so lets update it in the dictionary
                        AllClans[clanTag] = downloadedClan;
                    }
                }

                return downloadedClan;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<IWar> GetCurrentWarAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if (AllWarsByClanTag.TryGetValue(clanTag, out IWar warByClanTag))
                    {
                        if (warByClanTag.CacheExpiresAtUtc != null && warByClanTag.CacheExpiresAtUtc > DateTime.UtcNow) return warByClanTag;

                        if (!warByClanTag.IsExpired()) return warByClanTag;

                        if (allowExpiredItem) return warByClanTag;

                        if (warByClanTag is CurrentWarApiModel currentWar)
                        {
                            if (currentWar.StartTimeUtc > DateTime.UtcNow) return currentWar;

                            if (currentWar.State == WarState.WarEnded) return currentWar;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                IDownloadable downloadable = await WebResponse.GetWebResponse<CurrentWarApiModel>(EndPoint.CurrentWar, url, cts);
                
                _cancellationTokenSources.Remove(cts);

                if (downloadable is NotInWar notInWar)
                {
                    if (CocApiConfiguration.CacheHttpResponses) AllWarsByClanTag[clanTag] = notInWar;

                    return notInWar;
                }

                ICurrentWarApiModel downloadedWar = (ICurrentWarApiModel) downloadable;

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    foreach(var clan in downloadedWar.Clans)
                    {
                        AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar storedWar);

                        if (storedWar == null || storedWar.CacheExpiresAtUtc < downloadedWar.CacheExpiresAtUtc)
                        {
                            AllWarsByClanTag[clan.ClanTag] = downloadedWar;
                        }
                    }
                        
                    AllWarsByWarId[downloadedWar.WarId] = downloadedWar;                                       
                }

                return downloadedWar;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        //public async Task<IWar?> GetCurrentWarAsync(ICurrentWarApiModel storedWar)
        //{
        //    VerifyInitialization();

        //    try
        //    {
        //        if (AllWars.TryGetValue(storedWar.WarId, out IWar war))
        //        {
        //            //if (war is NotInWar notInWar) {

        //            //    if (notInWar.CacheExpiresAtUtc != null && notInWar.CacheExpiresAtUtc > DateTime.UtcNow) return notInWar;

        //            //    if (!notInWar.IsExpired()) return notInWar;
        //            //}

        //            if (war is ICurrentWarApiModel currentWar) 
        //            {
        //                if (currentWar.State == WarState.WarEnded) return currentWar;

        //                if ((currentWar.CacheExpiresAtUtc != null && currentWar.CacheExpiresAtUtc > DateTime.UtcNow)) return currentWar;

        //                if (!currentWar.IsExpired()) return currentWar;
        //            }
        //        }

        //        IWar? downloadedWar = null;

        //        if (storedWar is LeagueWarApiModel leagueWar)
        //        {
        //            try
        //            {
        //                downloadedWar = await GetLeagueWarAsync(leagueWar.WarTag, true, false);
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                downloadedWar = await GetCurrentWarAsync(storedWar.Clans[0].ClanTag, allowStoredItem: true, allowExpiredItem: false);
        //            }
        //            catch (Exception)
        //            {
        //            }

        //            if (downloadedWar is ICurrentWarApiModel currentWar && currentWar.WarId != storedWar.WarId)
        //            {
        //                downloadedWar = await GetCurrentWarAsync(storedWar.Clans[1].ClanTag, allowStoredItem: true, allowExpiredItem: false);
        //            }
        //        }

        //        if (downloadedWar is ICurrentWarApiModel currentWarApiModel && currentWarApiModel.WarId == storedWar.WarId)
        //        {
        //            return currentWarApiModel;
        //        }

        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        throw GetException(e);
        //    }

        //}

        public async Task<IWar?> GetCurrentWarOrDefaultAsync(ICurrentWarApiModel storedWar)
        {
            VerifyInitialization();

            try
            {
                if (AllWarsByWarId.TryGetValue(storedWar.WarId, out IWar warByWarId))
                {
                    if (warByWarId.CacheExpiresAtUtc > DateTime.UtcNow) return warByWarId;

                    if (!warByWarId.IsExpired()) return warByWarId;

                    if (warByWarId is ICurrentWarApiModel currentWarById)
                    {
                        if (currentWarById.State == WarState.WarEnded) return currentWarById;

                        if (currentWarById.StartTimeUtc > DateTime.UtcNow) return currentWarById;
                    }
                }

                foreach(var clan in storedWar.Clans)
                {
                    if (AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar warByClanTag))
                    {
                        if (!warByClanTag.IsExpired()) return warByClanTag;

                        if (warByClanTag.CacheExpiresAtUtc > DateTime.UtcNow) return warByClanTag;

                        if (warByClanTag is ICurrentWarApiModel currentWar)
                        {
                            if (currentWar.State == WarState.WarEnded) return currentWar;

                            if (currentWar.StartTimeUtc > DateTime.UtcNow) return currentWar;
                        }
                    }
                }

                ICurrentWarApiModel? currentWarApiModel = null;

                if (storedWar is LeagueWarApiModel leagueWar)
                {                     
                    //allow stored items is false becuase we already checked
                    currentWarApiModel = await GetLeagueWarOrDefaultAsync(leagueWar.WarTag, allowStoredItem: false, allowExpiredItem: false);
                }
                else
                {
                    foreach(var clan in storedWar.Clans)
                    {
                        //allow stored items is false becuase we already checked
                        currentWarApiModel = await GetCurrentWarOrDefaultAsync(clan.ClanTag, allowStoredItem: false, allowExpiredItem: false) as CurrentWarApiModel;

                        if (currentWarApiModel?.WarId == storedWar.WarId) break;
                    }
                    
                    //currentWarApiModel = await GetCurrentWarOrDefaultAsync(storedWar.Clans[0].ClanTag, allowStoredItem: false, allowExpiredItem: false) as CurrentWarApiModel;

                    //if (currentWarApiModel == null || currentWarApiModel.WarId != storedWar.WarId)
                    //{
                    //    currentWarApiModel = await GetCurrentWarOrDefaultAsync(storedWar.Clans[1].ClanTag, allowStoredItem: false, allowExpiredItem: false) as CurrentWarApiModel;
                    //}
                }

                if (currentWarApiModel?.WarId == storedWar.WarId)
                {
                    return currentWarApiModel;
                }

                return null;
            }
            catch (Exception e)
            {
                throw GetException(e);
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
        public async Task<ILeagueGroup> GetLeagueGroupAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem && AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup leagueGroup))
                {
                    if ((leagueGroup.CacheExpiresAtUtc != null && leagueGroup.CacheExpiresAtUtc > DateTime.UtcNow) || allowExpiredItem || !leagueGroup.IsExpired())
                    {
                        return leagueGroup;
                    }
                }

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                var downloadable = await WebResponse.GetWebResponse<LeagueGroupApiModel>(EndPoint.LeagueGroup, url, cts);
                
                _cancellationTokenSources.Remove(cts);
                
                if (downloadable is LeagueGroupNotFound notFound)
                {
                    AllLeagueGroups[clanTag] = notFound;

                    return notFound;
                }

                if (downloadable is LeagueGroupApiModel leagueGroupApiModel)
                {
                    if (CocApiConfiguration.CacheHttpResponses)
                    {
                        foreach(var clan in leagueGroupApiModel.Clans.EmptyIfNull())
                        {
                            if (!AllLeagueGroups.TryAdd(clan.ClanTag, leagueGroupApiModel))
                            {
                                if (AllLeagueGroups.TryGetValue(clan.ClanTag, out ILeagueGroup storedLeagueGroup))
                                {
                                    //the league group already exists.  Lets check if the existing one is from last month
                                    if (storedLeagueGroup is LeagueGroupApiModel storedLeagueGroupApiModel && leagueGroupApiModel.Season > storedLeagueGroupApiModel.Season)
                                    {
                                        AllLeagueGroups[clan.ClanTag] = storedLeagueGroupApiModel;
                                    }
                                }
                            }
                        }
                    }

                    return leagueGroupApiModel;
                }

                throw new CocApiException("Unknown type");
            }

            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<ICurrentWarApiModel> GetLeagueWarAsync(string warTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(warTag);

                if (allowStoredItem)
                {
                    if (AllWarsByWarTag.TryGetValue(warTag, out LeagueWarApiModel leagueWar))
                    {
                        if (leagueWar.CacheExpiresAtUtc != null && leagueWar.CacheExpiresAtUtc > DateTime.UtcNow) return leagueWar;

                        if (allowExpiredItem) return leagueWar;

                        if (leagueWar.State == WarState.WarEnded) return leagueWar;

                        if (!leagueWar.IsExpired()) return leagueWar;

                        if (leagueWar.StartTimeUtc > DateTime.UtcNow) return leagueWar;
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                LeagueWarApiModel leagueWarApiModel = (LeagueWarApiModel) await WebResponse.GetWebResponse<LeagueWarApiModel>(EndPoint.LeagueWar, url, cts);

                _cancellationTokenSources.Remove(cts);

                leagueWarApiModel.WarTag = warTag;

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    AllWarsByWarTag[leagueWarApiModel.WarTag] = leagueWarApiModel;

                    AllWarsByWarId[leagueWarApiModel.WarId] = leagueWarApiModel;

                    foreach(var clan in leagueWarApiModel.Clans)
                    {
                        AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                    }
                }

                return leagueWarApiModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<VillageApiModel> GetVillageAsync(string villageTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(villageTag);

                if (allowStoredItem && AllVillages.TryGetValue(villageTag, out VillageApiModel villageApiModel))
                {
                    if (villageApiModel.CacheExpiresAtUtc != null && villageApiModel.CacheExpiresAtUtc > DateTime.UtcNow) return villageApiModel;

                    if (allowExpiredItem) return villageApiModel;

                    if (!villageApiModel.IsExpired()) return villageApiModel;
                }

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                villageApiModel = (VillageApiModel) await WebResponse.GetWebResponse<VillageApiModel>(EndPoint.Village, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    //if (!AllVillages.TryAdd(villageApiModel.VillageTag, villageApiModel))
                    //{
                    //    if (villageApiModel.Clan == null || !_updateServices.Any(c => c.ClanStrings.Any(t => t == villageApiModel.Clan.ClanTag)))
                    //    {
                            ////we are not monitoring this clan so lets update it
                            AllVillages[villageTag] = villageApiModel;
                    //    }
                    //}
                }

                return villageApiModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<WarLogEntryModel>> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

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

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                var result = (PaginatedApiModel<WarLogEntryModel>) await WebResponse.GetWebResponse<PaginatedApiModel<WarLogEntryModel>>(EndPoint.WarLog, url, cts);

                _cancellationTokenSources.Remove(cts);

                return result;
            }
            catch (Exception e)
            {
                throw GetException(e);
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
                                                        , CancellationTokenSource? cancellationTokenSource = null)
        {
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


                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                var result = (PaginatedApiModel<ClanApiModel>) await WebResponse.GetWebResponse<PaginatedApiModel<ClanApiModel>>(EndPoint.Clans, url, cts);

                _cancellationTokenSources.Remove(cts);

                return result;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<VillageLeagueApiModel>> GetLeaguesAsync(CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/leagues?limit=500";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                var result = (PaginatedApiModel<VillageLeagueApiModel>) await WebResponse.GetWebResponse<PaginatedApiModel<VillageLeagueApiModel>>(EndPoint.VillageLeagues, url, cts);

                _cancellationTokenSources.Remove(cts);

                return result;
            }
            catch (Exception e)
            {

                throw GetException(e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<LocationApiModel>> GetLocationsAsync(CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations?limit=10000";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                var result = (PaginatedApiModel<LocationApiModel>) await WebResponse.GetWebResponse<PaginatedApiModel<LocationApiModel>>(EndPoint.Locations, url, cts);

                _cancellationTokenSources.Remove(cts);

                return result;
            }
            catch (Exception e)
            {

                throw GetException(e);
            }

        }














        /// <summary>
        /// Returns null if the clanTag is not found.  This will not throw a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ClanApiModel?> GetClanOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            ClanApiModel? result = null;

            try
            {
                result = await GetClanAsync(clanTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
            }
            catch (NotFoundException)
            {
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the war log is private.  This will not throw a <see cref="ForbiddenException"/> nor <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<IWar?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            IWar? result = null;

            try
            {
                result = await GetCurrentWarAsync(clanTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
            }
            catch (ForbiddenException) { }
            catch (NotFoundException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        ///// <summary>
        ///// Returns null if the clan is not in a clan is not found to be in a league.  This will not throw a <see cref="NotFoundException"/>.
        ///// </summary>
        ///// <param name="clanTag"></param>
        ///// <param name="allowStoredItem"></param>
        ///// <param name="allowExpiredItem"></param>
        ///// <param name="cancellationTokenSource"></param>
        ///// <returns></returns>
        //public async Task<ILeagueGroup?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        //{
        //    VerifyInitialization();

        //    ILeagueGroup? result = null;

        //    try
        //    {
        //        result = await GetLeagueGroupAsync(clanTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
        //    }
        //    catch (NotFoundException) { }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    return result;
        //}


        /// <summary>
        /// Returns null if the warTag is not found.  This will not throw a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="warTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ICurrentWarApiModel?> GetLeagueWarOrDefaultAsync(string warTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            ICurrentWarApiModel? result = null;

            try
            {
                result = await GetLeagueWarAsync(warTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
            }
            catch (NotFoundException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the villageTag is not found.  This will not throw a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<VillageApiModel?> GetVillageOrDefaultAsync(string villageTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            VillageApiModel? result = null;

            try
            {
                result = await GetVillageAsync(villageTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
            }
            catch (NotFoundException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the clan is not in a clan is not found to be in a league.  This will not throw a <see cref="ForbiddenException"/> nor a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<PaginatedApiModel<WarLogEntryModel>?> GetWarLogOrDefaultAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            PaginatedApiModel<WarLogEntryModel>? result = null;

            try
            {
                result = await GetWarLogAsync(clanTag, limit, after, before, cancellationTokenSource);
            }
            catch (ForbiddenException) { }
            catch (NotFoundException) { }
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
            VerifyInitialization();

            AllClans.TryGetValue(clanTag, out ClanApiModel? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the war log is private.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public IWar? GetCurrentWarByClanTagOrDefault(string clanTag)
        {
            VerifyInitialization();

            AllWarsByClanTag.TryGetValue(clanTag, out IWar? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clan is not in a clan is not found to be in a league.  
        /// Returns <see cref="LeagueGroupApiModel"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ILeagueGroup? GetLeagueGroupOrDefault(string clanTag)
        {
            VerifyInitialization();

            AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the warTag is not found.
        /// </summary>
        /// <param name="warTag"></param>
        /// <returns></returns>
        public ICurrentWarApiModel? GetLeagueWarOrDefault(string warTag)
        {
            VerifyInitialization();

            AllWarsByWarTag.TryGetValue(warTag, out LeagueWarApiModel? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the villageTag is not found.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <returns></returns>
        public VillageApiModel? GetVillageOrDefault(string villageTag)
        {
            VerifyInitialization();
            
            AllVillages.TryGetValue(villageTag, out VillageApiModel? result);

            return result;
        }









        /// <summary>
        /// Poll the Api and fire off events when a change is noticed.
        /// </summary>
        public void BeginUpdatingClans()
        {
            VerifyInitialization();

            foreach (UpdateService clanUpdateService in _updateServices)
            {
                clanUpdateService.BeginUpdatingClans();
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
        /// Establish the clans that you would like to poll for updates.  Run this when your program starts.  After running this, run <see cref="BeginUpdatingClans"/>.  Watching a large number of clans will take a lot of memory.  If you watch clans, you should have caching enabled.
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
                        ValidateTag(clanTag);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    _updateServices.ElementAt(j).ClanStrings.Add(clanTag);

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
                try
                {
                    ValidateTag(clanTag);
                }
                catch (Exception)
                {

                    throw;
                }

                UpdateService clanUpdateService = _updateServices.OrderBy(c => c.ClanStrings.Count()).FirstOrDefault();

                clanUpdateService.ClanStrings.Add(clanTag);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

        /// <summary>
        /// Check if a string appears to be a SuperCell tag.
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
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public List<WebResponseTimer> GetTimers() => WebResponse.GetTimers();

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

                if (day > 0 && day < 9)
                {
                    return true;
                }

                if (day == 9 && DateTime.UtcNow.Hour < 3)
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

            _testConnection.Dispose();

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
        /// <exception cref="ArgumentException"></exception>
        public void ValidateTag(string tag)
        {
            if (!IsValidTag(tag))
            {
                Logger.LogWarning(LoggingEvents.InvalidTag, "{source} The provided tag is not valid {tag}", _source, tag);

                throw new ArgumentException("Tags must not be null nor empty and must start with a #.");
            }
        }

        private void TestConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    await GetClanAsync("#929YJPYJ", allowStoredItem: false, allowExpiredItem: false);
                    _testConnection.Stop();
                }
                catch (Exception)
                {
                }
            });
        }

        internal Exception GetException(Exception e)
        {
            return e switch
            {
                BadGateWayException badGateWay => badGateWay,
                BadRequestException badRequestException => badRequestException,
                ForbiddenException forbiddenException => forbiddenException,
                GatewayTimeoutException  gatewayTimeoutException => gatewayTimeoutException,
                InternalServerErrorException internalServerErrorException => internalServerErrorException,
                NotFoundException notFoundExceptionn => notFoundExceptionn,
                ServiceUnavailableException serverUnavailableException => serverUnavailableException,
                TooManyRequestsException tooManyRequestsException => tooManyRequestsException,

                ServerResponseException serverResponseException => serverResponseException,
                ServerTookTooLongToRespondException serverTookTooLongToRespondException => serverTookTooLongToRespondException,
                CocApiException cocApiException => cocApiException,
                _ => new CocApiException(e.Message, e),
            };
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
                    UpdateService updateService = new UpdateService(this)
                    {
                        Logger = Logger
                    };

                    _updateServices.Add(updateService);
                }
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        private CancellationTokenSource GetCancellationTokenSource()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            _cancellationTokenSources.Add(cancellationTokenSource);

            return cancellationTokenSource;
        }

        private void VerifyInitialization()
        {
            if (!_isInitialized || CocApiConfiguration.Tokens == null || CocApiConfiguration.Tokens.Count() == 0)
            {
                throw new CocApiException("The library is not initialized, or you did not provide SC Api tokens.");
            }
        }
    }
}
