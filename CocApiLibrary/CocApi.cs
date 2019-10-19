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
    public delegate void ClanChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void MembersJoinedEventHandler(ClanAPIModel oldClan, List<MemberListAPIModel> memberListAPIModels);
    public delegate void MembersLeftEventHandler(ClanAPIModel oldClan, List<MemberListAPIModel> memberListAPIModels);
    public delegate void ClanBadgeUrlChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void ClanLocationChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void NewWarEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarChangedEventHandler(ICurrentWarAPIModel oldWar, ICurrentWarAPIModel newWar);
    public delegate void NewAttacksEventHandler(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels);
    public delegate void WarEndingSoonEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarStartingSoonEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void ClanVersusPointsChangedEventHandler(ClanAPIModel oldClan, int newClanVersusPoints);
    public delegate void ClanPointsChangedEventHandler(ClanAPIModel oldClan, int newClanPoints);
    public delegate void WarIsAccessibleChangedEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarEndNotSeenEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void VillageChangedEventHandler(VillageAPIModel oldVillage, VillageAPIModel newVillage);
    public delegate void VillageDefenseWinsChangedEventHandler(VillageAPIModel oldVillage, int newDefenseWins);
    //public delegate void VillageDonationsChangedEventHandler(VillageAPIModel oldVillage, int newDonations);
    //public delegate void VillageDonationsReceivedChangedEventHandler(VillageAPIModel oldVillage, int newDonationsReceived);
    public delegate void VillageExpLevelChangedEventHandler(VillageAPIModel oldVillage, int newExpLevel);
    public delegate void VillageTrophiesChangedEventHandler(VillageAPIModel oldVillage, int newTrophies);
    public delegate void VillageVersusBattleWinCountChangedEventHandler(VillageAPIModel oldVillage, int newVersusBattleWinCount);
    public delegate void VillageVersusBattleWinsChangedEventHandler(VillageAPIModel oldVillage, int newVersusBattleWins);
    public delegate void VillageVersusTrophiesChangedEventHandler(VillageAPIModel oldVillage, int newVersusTrophies);
    public delegate void VillageLeagueChangedEventHandler(VillageAPIModel oldVillage, LeagueAPIModel? newLeague);
    public delegate void VillageAchievementsChangedEventHandler(VillageAPIModel oldVillage, List<AchievementAPIModel> newAchievements);
    public delegate void VillageTroopsChangedEventHandler(VillageAPIModel oldVillage, List<TroopAPIModel> newTroops);
    public delegate void VillageHeroesChangedEventHandler(VillageAPIModel oldVillage, List<TroopAPIModel> newHeroes);
    public delegate void VillageSpellsChangedEventHandler(VillageAPIModel oldVillage, List<SpellAPIModel> newSpells);
    public delegate void WarStartedEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarEndedEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarEndSeenEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void LeagueGroupTeamSizeChangeDetectedEventHandler(LeagueGroupAPIModel leagueGroupAPIModel);
    public delegate void ClanLabelsRemovedEventHandler(ClanAPIModel newClanAPIModel, IEnumerable<LabelAPIModel> labelAPIModels);
    public delegate void ClanLabelsAddedEventHandler(ClanAPIModel newClanAPIModel, IEnumerable<LabelAPIModel> labelAPIModels);
    public delegate void VillageLabelsRemovedEventHandler(VillageAPIModel newVillageAPIModel, IEnumerable<LabelAPIModel> labelAPIModels);
    public delegate void VillageLabelsAddedEventHandler(VillageAPIModel newVillageAPIModel, IEnumerable<LabelAPIModel> labelAPIModels);
    public delegate void VillageReachedLegendsLeagueEventHandler(VillageAPIModel villageAPIModel);
    public delegate void ClanDonationsEventHandler(Dictionary<string, Tuple<MemberListAPIModel, int>> receivedDonations, Dictionary<string, Tuple<MemberListAPIModel, int>> gaveDonations);
    public delegate void ClanMemberNameChanged(MemberListAPIModel oldMember, string newName);
    public delegate void ClanMembersLeagueChanged(Dictionary<string, Tuple<MemberListAPIModel, LeagueAPIModel>> leagueChanged);
    public delegate void ClanMembersRoleChanged(Dictionary<string, Tuple<MemberListAPIModel, Role>> roleChanges);



    public class CocApi : IDisposable
    {
        private bool? _isAvailable;
        private readonly System.Timers.Timer _testConnection = new System.Timers.Timer();
        private readonly List<UpdateService> _updateServices = new List<UpdateService>();
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        internal Dictionary<string, ClanAPIModel> AllClans { get; } = new Dictionary<string, ClanAPIModel>();
        internal Dictionary<string, ICurrentWarAPIModel> AllWars { get; } = new Dictionary<string, ICurrentWarAPIModel>();
        internal Dictionary<string, LeagueGroupAPIModel> AllLeagueGroups { get; } = new Dictionary<string, LeagueGroupAPIModel>();
        internal Dictionary<string, VillageAPIModel> AllVillages { get; } = new Dictionary<string, VillageAPIModel>();
        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Fires if you query the API during an outage.  
        /// The API will be polled every five seconds to see when service is restored. 
        /// If the service is not available, you may still try to query the API if you wish.
        /// </summary>
        public event IsAvailableChangedEventHandler? IsAvailableChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="ClanAPIModel.ClanLevel"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.Description"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.IsWarLogPublic"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.MemberCount"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.Name"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.RequiredTrophies"/>RequiredTrophies</description></item>
        ///    <item><description><see cref="ClanAPIModel.Type"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.WarFrequency"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.WarLosses"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.WarTies"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.WarWins"/></description></item>
        ///    <item><description><see cref="ClanAPIModel.WarWinStreak"/></description></item>
        /// </list>
        /// </summary>
        public event ClanChangedEventHandler? ClanChanged;
        public event MembersJoinedEventHandler? MembersJoined;
        public event MembersLeftEventHandler? MembersLeft;
        public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
        public event ClanLocationChangedEventHandler? ClanLocationChanged;
        public event NewWarEventHandler? NewWar;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///     <item><description><see cref="ICurrentWarAPIModel.EndTimeUTC"/></description></item>
        ///     <item><description><see cref="ICurrentWarAPIModel.StartTimeUTC"/></description></item>
        ///     <item><description><see cref="ICurrentWarAPIModel.State"/></description></item>
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
        ///    <item><description><see cref="VillageAPIModel.AttackWins"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.BestTrophies"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.BestVersusTrophies"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.BuilderHallLevel"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.TownHallLevel"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.TownHallWeaponLevel"/></description></item>
        ///    <item><description><see cref="VillageAPIModel.WarStars"/></description></item>
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
        /// Fires when the <see cref="ICurrentWarAPIModel.EndTimeUTC"/> has elapsed.  The API may or may not show the war end when this event occurs.
        /// </summary>
        public event WarEndedEventHandler? WarEnded;
        /// <summary>
        /// Fires when the API shows <see cref="ICurrentWarAPIModel.State"/> is <see cref="Enums.WarState.WarEnded"/>
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
        public event ClanMemberNameChanged? ClanMemberNameChanged;
        public event ClanMembersLeagueChanged? ClanMembersLeagueChanged;
        public event ClanMembersRoleChanged? ClanMembersRoleChanged;


        public Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public ILogger? Logger { get; set; }


        /// <summary>
        /// Controls whether any clan will be able to download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.False;

        /// <summary>
        /// Controls whether any clan will be able to download village members.
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
        /// Initializes the CocApi library.  A configuration with SC API tokens is required.  A logger may also be provided.
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
                throw new CocApiException("You did not provide any tokens to access the SC API.");
            }

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            CreateUpdaters();

            _isInitialized = true;
        }


        internal void ClanMembersRoleChangedEvent(Dictionary<string, Tuple<MemberListAPIModel, Role>> roleChanges)
        {
            if (roleChanges.Count() > 0)
            {
                ClanMembersRoleChanged?.Invoke(roleChanges);
            }
        }

        internal void ClanMembersLeagueChangedEvent(Dictionary<string, Tuple<MemberListAPIModel, LeagueAPIModel>> leagueChanged)
        {
            if (leagueChanged.Count() > 0)
            {
                ClanMembersLeagueChanged?.Invoke(leagueChanged);
            }
        }

        internal void ClanMemberNameChangedEvent(MemberListAPIModel oldMember, string newName)
        {
            ClanMemberNameChanged?.Invoke(oldMember, newName);
        }

        internal void ClanDonationsEvent(Dictionary<string, Tuple<MemberListAPIModel, int>> receivedDonations, Dictionary<string, Tuple<MemberListAPIModel, int>> gaveDonations)
        {
            if (receivedDonations.Count() > 0 || gaveDonations.Count() > 0)
            {
                ClanDonations?.Invoke(receivedDonations, gaveDonations);
            }
        }

        internal void VillageReachedLegendsLeagueEvent(VillageAPIModel villageAPIModel)
        {
            VillageReachedLegendsLeague?.Invoke(villageAPIModel);
        }

        internal void VillageLabelsRemovedEvent(VillageAPIModel newVillage, IEnumerable<LabelAPIModel> labelAPIModels)
        {
            if (labelAPIModels != null && labelAPIModels.Count() > 0)
            {
                VillageLabelsRemoved?.Invoke(newVillage, labelAPIModels);
            }
        }

        internal void VillageLabelsAddedEvent(VillageAPIModel newVillage, IEnumerable<LabelAPIModel> labelAPIModels)
        {
            if (labelAPIModels != null && labelAPIModels.Count() > 0)
            {
                VillageLabelsAdded?.Invoke(newVillage, labelAPIModels);
            }
        }

        internal void ClanLabelsRemovedEvent(ClanAPIModel newClan, IEnumerable<LabelAPIModel> labelAPIModels)
        {
            if (labelAPIModels != null && labelAPIModels.Count() > 0)
            {
                ClanLabelsRemoved?.Invoke(newClan, labelAPIModels);
            }
        }

        internal void ClanLabelsAddedEvent(ClanAPIModel newClan, IEnumerable<LabelAPIModel> labelAPIModels)
        {
            if (labelAPIModels != null && labelAPIModels.Count() > 0)
            {
                ClanLabelsAdded?.Invoke(newClan, labelAPIModels);
            }
        }

        internal void LeagueGroupTeamSizeChangeDetectedEvent(LeagueGroupAPIModel leagueGroupAPIModel)
        {
            LeagueGroupTeamSizeChangeDetected?.Invoke(leagueGroupAPIModel);
        }

        internal void WarEndSeenEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarEndSeen?.Invoke(currentWarAPIModel);
        }

        internal void WarEndedEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarEnded?.Invoke(currentWarAPIModel);
        }

        internal void WarStartedEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarStarted?.Invoke(currentWarAPIModel);
        }

        internal void VillageSpellsChangedEvent(VillageAPIModel oldVillage, List<SpellAPIModel> newSpells)
        {
            VillageSpellsChanged?.Invoke(oldVillage, newSpells);
        }

        internal void VillageHeroesChangedEvent(VillageAPIModel oldVillage, List<TroopAPIModel> newHeroes)
        {
            VillageHeroesChanged?.Invoke(oldVillage, newHeroes);
        }

        internal void VillageTroopsChangedEvent(VillageAPIModel oldVillage, List<TroopAPIModel> newTroops)
        {
            VillageTroopsChanged?.Invoke(oldVillage, newTroops);
        }

        internal void VillageAchievementsChangedEvent(VillageAPIModel oldVillage, List<AchievementAPIModel> newAchievements)
        {
            VillageAchievementsChanged?.Invoke(oldVillage, newAchievements);
        }

        internal void VillageLeagueChangedEvent(VillageAPIModel oldVillage, LeagueAPIModel? newLeague)
        {
            VillageLeagueChanged?.Invoke(oldVillage, newLeague);
        }

        internal void VillageVersusTrophiesChangedEvent(VillageAPIModel oldVillage, int newVersusTrophies)
        {
            VillageVersusTrophiesChanged?.Invoke(oldVillage, newVersusTrophies);
        }

        internal void VillageVersusBattleWinsChangedEvent(VillageAPIModel oldVillage, int newVersusBattleWins)
        {
            VillageVersusBattleWinsChanged?.Invoke(oldVillage, newVersusBattleWins);
        }

        internal void VillageVersusBattleWinCountChangedEvent(VillageAPIModel oldVillage, int newVersusBattleWinCount)
        {
            VillageVersusBattleWinCountChanged?.Invoke(oldVillage, newVersusBattleWinCount);
        }

        internal void VillageTrophiesChangedEvent(VillageAPIModel oldVillage, int newTrophies)
        {
            VillageTrophiesChanged?.Invoke(oldVillage, newTrophies);
        }

        internal void VillageExpLevelChangedEvent(VillageAPIModel oldVillage, int newExpLevel)
        {
            VillageExpLevelChanged?.Invoke(oldVillage, newExpLevel);
        }

        //internal void VillageDonationsReceivedChangedEvent(VillageAPIModel oldVillage, int newDonationsReceived)
        //{
        //    VillageDonationsReceivedChanged?.Invoke(oldVillage, newDonationsReceived);
        //}

        //internal void VillageDonationsChangedEvent(VillageAPIModel oldVillage, int newDonations)
        //{
        //    VillageDonationsChanged?.Invoke(oldVillage, newDonations);
        //}

        internal void VillageDefenseWinsChangedEvent(VillageAPIModel oldVillage, int newDefenseWinsChanged)
        {
            VillageDefenseWinsChanged?.Invoke(oldVillage, newDefenseWinsChanged);
        }

        internal void VillageChangedEvent(VillageAPIModel oldVillage, VillageAPIModel newVillage)
        {
            VillageChanged?.Invoke(oldVillage, newVillage);
        }

        internal void WarEndNotSeenEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarEndNotSeen?.Invoke(currentWarAPIModel);
        }

        internal void WarIsAccessibleChangedEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarIsAccessibleChanged?.Invoke(currentWarAPIModel);
        }

        internal void ClanPointsChangedEvent(ClanAPIModel oldClan, int newClanPoints)
        {
            ClanPointsChanged?.Invoke(oldClan, newClanPoints);
        }

        internal void ClanVersusPointsChangedEvent(ClanAPIModel oldClan, int newClanVersusPoints)
        {
            ClanVersusPointsChanged?.Invoke(oldClan, newClanVersusPoints);
        }

        internal void WarStartingSoonEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarStartingSoon?.Invoke(currentWarAPIModel);
        }

        internal void WarEndingSoonEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            WarEndingSoon?.Invoke(currentWarAPIModel);
        }

        internal void NewAttacksEvent(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels)
        {
            if (attackAPIModels.Count() > 0)
            {
                NewAttacks?.Invoke(currentWarAPIModel, attackAPIModels);
            }
        }

        internal void WarChangedEvent(ICurrentWarAPIModel oldWar, ICurrentWarAPIModel newWar)
        {
            WarChanged?.Invoke(oldWar, newWar);
        }

        internal void NewWarEvent(ICurrentWarAPIModel currentWarAPIModel)
        {
            NewWar?.Invoke(currentWarAPIModel);
        }

        internal void MembersLeftEvent(ClanAPIModel newClan, List<MemberListAPIModel> memberListAPIModels)
        {
            if (memberListAPIModels.Count() > 0)
            {
                MembersLeft?.Invoke(newClan, memberListAPIModels);
            }            
        }

        internal void ClanLocationChangedEvent(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            ClanLocationChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanBadgeUrlChangedEvent(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            ClanBadgeUrlChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanChangedEvent(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            ClanChanged?.Invoke(oldClan, newClan);
        }

        internal void MembersJoinedEvent(ClanAPIModel newClan, List<MemberListAPIModel> memberListAPIModels)
        {
            if (memberListAPIModels.Count() > 0)
            {
                MembersJoined?.Invoke(newClan, memberListAPIModels);
            }
        }














        public async Task<ClanAPIModel> GetClanAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if (AllClans.TryGetValue(clanTag, out ClanAPIModel storedClan))
                    {
                        if (!storedClan.IsExpired() || allowExpiredItem)
                        {
                            return storedClan;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                ClanAPIModel downloadedClan = await WebResponse.GetWebResponse<ClanAPIModel>(EndPoint.Clan, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    if (!AllClans.TryAdd(downloadedClan.Tag, downloadedClan) && !_updateServices.Any(c => c.ClanStrings.Any(t => t == downloadedClan.Tag)))
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

        public async Task<ICurrentWarAPIModel> GetCurrentWarAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if (AllWars.TryGetValue(clanTag, out var warFromAllWars))
                    {
                        if (!warFromAllWars.IsExpired() || allowExpiredItem)
                        {
                            return warFromAllWars;
                        }
                    }

                    if (AllClans.TryGetValue(clanTag, out var clan))
                    {
                        ICurrentWarAPIModel? warFromAllClans = clan.Wars.OrderBy(w => w.Value.PreparationStartTimeUTC).FirstOrDefault().Value;

                        if (warFromAllClans != null && (allowExpiredItem || !warFromAllClans.IsExpired()))
                        {
                            return warFromAllClans;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                CurrentWarAPIModel currentWarAPIModel = await WebResponse.GetWebResponse<CurrentWarAPIModel>(EndPoint.CurrentWar, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    if (currentWarAPIModel.State == WarState.NotInWar || !_updateServices.Any(c => c.ClanStrings.Any(t => t == clanTag))) 
                    {
                        //update the stored war if it is not in war or the clan is not being watched
                        AllWars[clanTag] = currentWarAPIModel;
                    }
                    else
                    {
                        if (AllWars.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel))
                        {
                            currentWarAPIModel.AnnounceNewWar(this);

                            foreach (var clan in currentWarAPIModel.Clans)
                            {
                                ClanAPIModel clanAPIModel = await GetClanAsync(clan.Tag, allowExpiredItem: true);

                                clanAPIModel.Wars.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel);
                            }
                        }
                    }
                }

                return currentWarAPIModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<ICurrentWarAPIModel?> GetCurrentWarAsync(ICurrentWarAPIModel storedWar)
        {
            VerifyInitialization();

            try
            {
                if (AllWars.TryGetValue(storedWar.WarID, out ICurrentWarAPIModel war))
                {
                    if (war.State == WarState.WarEnded || !war.IsExpired()) return war;
                }

                ICurrentWarAPIModel? currentWarAPIModel = null;

                if (storedWar is LeagueWarAPIModel leagueWar)
                {
                    try
                    {
                        currentWarAPIModel = await GetLeagueWarAsync(leagueWar.WarTag, true, false);
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    try
                    {
                        currentWarAPIModel = await GetCurrentWarAsync(storedWar.Clans[0].Tag, allowStoredItem: true, allowExpiredItem: false);
                    }
                    catch (Exception)
                    {
                    }

                    if (currentWarAPIModel != null && currentWarAPIModel.WarID != storedWar.WarID)
                    {
                        currentWarAPIModel = await GetCurrentWarAsync(storedWar.Clans[1].Tag, allowStoredItem: true, allowExpiredItem: false);
                    }
                }

                if (currentWarAPIModel != null && currentWarAPIModel.WarID == storedWar.WarID)
                {
                    return currentWarAPIModel;
                }

                return null;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }

        }

        public async Task<LeagueGroupAPIModel> GetLeagueGroupAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem && AllLeagueGroups.TryGetValue(clanTag, out LeagueGroupAPIModel leagueGroupAPIModel))
                {
                    if (allowExpiredItem || !leagueGroupAPIModel.IsExpired())
                    {
                        return leagueGroupAPIModel;
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                leagueGroupAPIModel = await WebResponse.GetWebResponse<LeagueGroupAPIModel>(EndPoint.LeagueGroup, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    foreach(var clan in leagueGroupAPIModel.Clans.EmptyIfNull())
                    {
                        if (!AllLeagueGroups.TryAdd(clan.Tag, leagueGroupAPIModel))
                        {
                            if (AllLeagueGroups.TryGetValue(clan.Tag, out LeagueGroupAPIModel storedLeagueGroupAPIModel))
                            {
                                //the league group already exists.  Lets check if the existing one is from last month
                                if (leagueGroupAPIModel.Season > storedLeagueGroupAPIModel.Season)
                                {
                                    AllLeagueGroups[clan.Tag] = leagueGroupAPIModel;
                                }
                            }
                        }
                    }
                }

                return leagueGroupAPIModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<ICurrentWarAPIModel> GetLeagueWarAsync(string warTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(warTag);

                if (allowStoredItem)
                {
                    if (AllWars.TryGetValue(warTag, out ICurrentWarAPIModel currentWarAPIModel))
                    {
                        if (allowExpiredItem || currentWarAPIModel.State == WarState.WarEnded || !currentWarAPIModel.IsExpired())
                        {
                            return currentWarAPIModel;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                LeagueWarAPIModel leagueWarAPIModel = await WebResponse.GetWebResponse<LeagueWarAPIModel>(EndPoint.LeagueWar, url, cts);

                _cancellationTokenSources.Remove(cts);

                leagueWarAPIModel.WarTag = warTag;

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    if (AllWars.TryAdd(leagueWarAPIModel.WarTag, leagueWarAPIModel))
                    {
                        leagueWarAPIModel.AnnounceNewWar(this);
                    }
                    else if (!_updateServices.Any(c => c.ClanStrings.Any(t => t == leagueWarAPIModel.Clans[0].Tag || t == leagueWarAPIModel.Clans[1].Tag)))
                    {
                        //it already exists but neither clan is being watched, lets update the cache
                        AllWars[warTag] = leagueWarAPIModel;
                    }

                    foreach (var clan in leagueWarAPIModel.Clans)
                    {
                        ClanAPIModel clanAPIModel = await GetClanAsync(clan.Tag, allowExpiredItem: true);

                        if (!clanAPIModel.Wars.TryAdd(leagueWarAPIModel.WarID, leagueWarAPIModel) && !_updateServices.Any(c => c.ClanStrings.Any(t => t == clanAPIModel.Tag)))
                        {
                            //the war exists in the stored clan but the clan is not being watched so lets update the cache
                            clanAPIModel.Wars[leagueWarAPIModel.WarID] = leagueWarAPIModel;
                        }
                    }
                }

                return leagueWarAPIModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<VillageAPIModel> GetVillageAsync(string villageTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            try
            {
                ValidateTag(villageTag);

                if (allowStoredItem && AllVillages.TryGetValue(villageTag, out VillageAPIModel villageAPIModel))
                {
                    if (allowExpiredItem || !villageAPIModel.IsExpired())
                    {
                        return villageAPIModel;
                    }
                }

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                using CancellationTokenSource cts = GetCancellationTokenSource();

                cancellationTokenSource?.Token.Register(() => cts.Cancel());

                villageAPIModel = await WebResponse.GetWebResponse<VillageAPIModel>(EndPoint.Village, url, cts);

                _cancellationTokenSources.Remove(cts);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    if (!AllVillages.TryAdd(villageAPIModel.Tag, villageAPIModel))
                    {
                        if (villageAPIModel.Clan == null || !_updateServices.Any(c => c.ClanStrings.Any(t => t == villageAPIModel.Clan.Tag)))
                        {
                            AllVillages[villageTag] = villageAPIModel;
                        }
                    }
                }

                return villageAPIModel;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<WarLogAPIModel> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationTokenSource? cancellationTokenSource = null)
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

                var result = await WebResponse.GetWebResponse<WarLogAPIModel>(EndPoint.WarLog, url, cts);

                _cancellationTokenSources.Remove(cts);

                return result;
            }
            catch (Exception e)
            {
                throw GetException(e);
            }
        }

        public async Task<ClanSearchModel> GetClansAsync(string? clanName = null
                                                        , WarFrequency? warFrequency = null
                                                        , int? locationID = null
                                                        , int? minMembers = null
                                                        , int? maxMembers = null
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
                if (locationID != null)
                {
                    url = $"{url}locationId={locationID}&";
                }
                if (minMembers != null)
                {
                    url = $"{url}minMembers={minMembers}&";
                }
                if (maxMembers != null)
                {
                    url = $"{url}maxMembers={maxMembers}&";
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

                var result = await WebResponse.GetWebResponse<ClanSearchModel>(EndPoint.Clans, url, cts);

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
        public async Task<ClanAPIModel?> GetClanOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            ClanAPIModel? result = null;

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
        public async Task<ICurrentWarAPIModel?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            ICurrentWarAPIModel? result = null;

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


        /// <summary>
        /// Returns null if the clan is not in a clan is not found to be in a league.  This will not throw a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<LeagueGroupAPIModel?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            LeagueGroupAPIModel? result = null;

            try
            {
                result = await GetLeagueGroupAsync(clanTag, allowStoredItem, allowExpiredItem, cancellationTokenSource);
            }
            catch (NotFoundException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the warTag is not found.  This will not throw a <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="warTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ICurrentWarAPIModel?> GetLeagueWarOrDefaultAsync(string warTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            ICurrentWarAPIModel? result = null;

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
        public async Task<VillageAPIModel?> GetVillageOrDefaultAsync(string villageTag, bool allowStoredItem = true, bool allowExpiredItem = false, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            VillageAPIModel? result = null;

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
        public async Task<WarLogAPIModel?> GetWarLogOrDefaultAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationTokenSource? cancellationTokenSource = null)
        {
            VerifyInitialization();

            WarLogAPIModel? result = null;

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
        /// Poll the API and fire off events when a change is noticed.
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
        /// Stop polling the API.  Events will not fire.  This could take some time to finish if updating villages or league wars.
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
        //public void LoadFromDatabase(IEnumerable<ClanAPIModel> clans, IEnumerable<ICurrentWarAPIModel> wars, IEnumerable<LeagueGroupAPIModel> leagueGroups, IEnumerable<VillageAPIModel> villages)
        //{
        //    try
        //    {
        //        foreach (ClanAPIModel clan in clans)
        //        {
        //            clan.AnnounceWars = true;

        //            AllClans.TryAdd(clan.Tag, clan);
        //        }

        //        foreach (CurrentWarAPIModel war in wars.Where(w => w.State != WarState.NotInWar))
        //        {
        //            if (war is LeagueWarAPIModel leagueWar)
        //            {
        //                //leagueWar.Initialize();  //todo should i be initializing the saved object?

        //                AllWars.TryAdd(leagueWar.WarTag, leagueWar);
        //            }
        //            //else
        //            //{
        //            //    war.Initialize();
        //            //}

        //            AllWars.TryAdd(war.WarID, war);

        //            foreach (WarClanAPIModel warClan in war.Clans)
        //            {
        //                if (AllClans.TryGetValue(warClan.Tag, out ClanAPIModel clan))
        //                {
        //                    if (clan.Wars == null) clan.Wars = new Dictionary<string, ICurrentWarAPIModel>();

        //                    clan.Wars.TryAdd(war.WarID, war);
        //                }
        //            }
        //        }

        //        foreach (LeagueGroupAPIModel leagueGroup in leagueGroups)
        //        {
        //            foreach (LeagueClanAPIModel leagueClan in leagueGroup.Clans.EmptyIfNull())
        //            {
        //                if (AllLeagueGroups.TryGetValue(leagueClan.Tag, out LeagueGroupAPIModel storedLeagueGroupAPIModel))
        //                {
        //                    //the league group already exists.  Lets check if the existing one is from last month
        //                    if (leagueGroup.Season != storedLeagueGroupAPIModel.Season && leagueGroup.State != LeagueState.WarsEnded)
        //                    {
        //                        storedLeagueGroupAPIModel = leagueGroup;
        //                    }
        //                }
        //                else
        //                {
        //                    AllLeagueGroups.TryAdd(leagueClan.Tag, leagueGroup);
        //                }
        //            }
        //        }

        //        foreach (VillageAPIModel village in villages)
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
        /// Use this to get statistics on how long the API takes to respond for diffent and points.
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




        //private void AnnounceNewWar(string clanTag, ICurrentWarAPIModel currentWarAPIModel)
        //{
        //    try
        //    {
        //        if (currentWarAPIModel.State == WarState.NotInWar || currentWarAPIModel.State == WarState.WarEnded || currentWarAPIModel.Flags.)
        //        {
        //            return;
        //        }

        //        if (AllClans.TryGetValue(clanTag, out ClanAPIModel clanAPIModel))
        //        {
        //            if (clanAPIModel.AnnounceWars)
        //            {
        //                NewWarEvent(currentWarAPIModel);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new CocApiException(e.Message, e);
        //    }
        //}



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
                    UpdateService updateService = new UpdateService(this);

                    updateService.Logger = Logger;

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
                throw new CocApiException("The library is not initialized, or you did not provide SC API tokens.");
            }
        }
    }
}
