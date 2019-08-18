using CocApiLibrary.Models;
using static CocApiLibrary.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Timers;
using CocApiLibrary.Exceptions;
using System.Text.Json;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Linq;
using AutoMapper;
//using System.Threading;

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
    public delegate void VillageDonationsChangedEventHandler(VillageAPIModel oldVillage, int newDonations);
    public delegate void VillageDonationsReceivedChangedEventHandler(VillageAPIModel oldVillage, int newDonationsReceived);
    public delegate void VillageExpLevelChangedEventHandler(VillageAPIModel oldVillage, int newExpLevel);
    public delegate void VillageTrophiesChangedEventHandler(VillageAPIModel oldVillage, int newTrophies);
    public delegate void VillageVersusBattleWinCountChangedEventHandler(VillageAPIModel oldVillage, int newVersusBattleWinCount);
    public delegate void VillageVersusBattleWinsChangedEventHandler(VillageAPIModel oldVillage, int newVersusBattleWins);
    public delegate void VillageVersusTrophiesChangedEventHandler(VillageAPIModel oldVillage, int newVersusTrophies);
    public delegate void VillageLeagueChangedEventHandler(VillageAPIModel oldVillage, LeagueAPIModel? newLeague);
    public delegate void VillageAchievementsChangedEventHandler(VillageAPIModel oldVillage, List<AchievementAPIModel> newAchievements);
    public delegate void VillageTroopsChangedEventHandler(VillageAPIModel oldVillage, List<TroopAPIModel> newTroops);
    public delegate void VillageHeroesChangedEventHandler(VillageAPIModel oldVillage, List<TroopAPIModel> newHeroes);
    public delegate void VillageSpellsChangedEventHandler(VillageAPIModel oldVillage, List<SpellModel> newSpells);


    public class CocApi
    {
        private bool _isAvailable = true;
        private readonly Timer _testConnection = new System.Timers.Timer();
        private readonly System.Timers.Timer _timer;
        private readonly List<ClanUpdateService> _clanUpdateServices = new List<ClanUpdateService>();

        internal readonly Dictionary<string, ClanAPIModel> AllClans = new Dictionary<string, ClanAPIModel>();
        internal readonly Dictionary<string, ICurrentWarAPIModel> AllWars = new Dictionary<string, ICurrentWarAPIModel>();
        internal readonly Dictionary<string, LeagueGroupAPIModel> AllLeagueGroups = new Dictionary<string, LeagueGroupAPIModel>();
        internal readonly Dictionary<string, VillageAPIModel> AllVillages = new Dictionary<string, VillageAPIModel>();
        internal readonly Configuration _cfg = new Configuration();

        public event IsAvailableChangedEventHandler? IsAvailableChanged;
        public event ClanChangedEventHandler? ClanChanged;
        public event MembersJoinedEventHandler? MembersJoined;
        public event MembersLeftEventHandler? MembersLeft;
        public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
        public event ClanLocationChangedEventHandler? ClanLocationChanged;
        public event NewWarEventHandler? NewWar;
        public event WarChangedEventHandler? WarChanged;
        public event NewAttacksEventHandler? NewAttacks;
        public event WarEndingSoonEventHandler? WarEndingSoon;
        public event WarStartingSoonEventHandler? WarStartingSoon;
        public event ClanVersusPointsChangedEventHandler? ClanVersusPointsChanged;
        public event ClanPointsChangedEventHandler? ClanPointsChanged;
        public event WarIsAccessibleChangedEventHandler? WarIsAccessibleChanged;
        public event WarEndNotSeenEventHandler? WarEndNotSeen;
        public event VillageChangedEventHandler? VillageChanged;
        public event VillageDefenseWinsChangedEventHandler? VillageDefenseWinsChanged;
        public event VillageDonationsChangedEventHandler? VillageDonationsChanged;
        public event VillageDonationsChangedEventHandler? VillageDonationsReceivedChanged;
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

        public static readonly Regex ValidTagCharacters = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public bool DownloadLeagueWars = false;
        public bool DownloadVillages = false;

        public DateTime NextTimerResetUTC { get; private set; }
        public bool IsAvailable
        {
            get { return _isAvailable; }
            internal set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    IsAvailableChanged?.Invoke(value);

                    if (!_isAvailable)
                    {
                        _testConnection.Interval = 6000;
                        _testConnection.Elapsed += TestConnection_Elapsed;
                        _testConnection.AutoReset = true;
                        _testConnection.Enabled = true;
                    }
                }
            }
        }




        public CocApi(IEnumerable<string> tokens, Configuration? cfg = null)
        {
            if(cfg != null)
            {
                _cfg = cfg;
            }

            WebResponse.Initialize(_cfg, tokens);

            _timer = new Timer();
            _timer.Elapsed += TimerElapsed;
            _timer.Interval = 1800000; //30 minutes
            _timer.AutoReset = true;
            _timer.Enabled = true;

            NextTimerResetUTC = DateTime.UtcNow.AddMilliseconds(_timer.Interval);

            CreateUpdaters();
        }

        internal void VillageSpellsChangedEvent(VillageAPIModel oldVillage, List<SpellModel> newSpells)
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

        internal void VillageDonationsReceivedChangedEvent(VillageAPIModel oldVillage, int newDonationsReceived)
        {
            VillageDonationsReceivedChanged?.Invoke(oldVillage, newDonationsReceived);
        }

        internal void VillageDonationsChangedEvent(VillageAPIModel oldVillage, int newDonations)
        {
            VillageDonationsChanged?.Invoke(oldVillage, newDonations);
        }

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
            if(attackAPIModels.Count() > 0)
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
            if(memberListAPIModels.Count() > 0)
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
            if(memberListAPIModels.Count() > 0)
            {
                MembersJoined?.Invoke(newClan, memberListAPIModels);
            }
        }









        public async Task<ClanAPIModel> GetClanAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = true)
        {
            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if(AllClans.TryGetValue(clanTag, out ClanAPIModel storedClan))
                    {
                        if(!storedClan.IsExpired() || allowExpiredItem)
                        {
                            return storedClan;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                ClanAPIModel downloadedClan = await WebResponse.GetWebResponse<ClanAPIModel>(this, url);

                AllClans.TryAdd(downloadedClan.Tag, downloadedClan);

                return downloadedClan;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
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
                                                        , int? before = null)
        {
            try
            {
                if(!string.IsNullOrEmpty(clanName) && clanName.Length < 3)
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

                return await WebResponse.GetWebResponse<ClanSearchModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<ICurrentWarAPIModel> GetCurrentWarAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = true)
        {
            try
            {
                ValidateTag(clanTag);

                if (allowStoredItem)
                {
                    if(AllClans.TryGetValue(clanTag, out var clan))
                    {
                        if (clan.Wars.Where(w => w.Value is CurrentWarAPIModel).OrderBy(w => w.Value.PreparationStartTimeUTC).FirstOrDefault(w => w.Value.EndTimeUTC > DateTime.UtcNow).Value is CurrentWarAPIModel war && (allowExpiredItem || !war.IsExpired()))
                        {
                            return war;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";
                    
                CurrentWarAPIModel currentWarAPIModel = await WebResponse.GetWebResponse<CurrentWarAPIModel>(this, url);

                if (currentWarAPIModel.State != State.NotInWar)
                {
                    AllWars.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel);

                    foreach (var clan in currentWarAPIModel.Clans)
                    {
                        ClanAPIModel clanAPIModel = await GetClanAsync(clan.Tag);

                        if(clanAPIModel.Wars.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel))
                        {
                            AnnounceNewWar(clanTag, currentWarAPIModel);
                        }
                    }
                }               

                return currentWarAPIModel;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<ICurrentWarAPIModel?> GetCurrentWarAsync(ICurrentWarAPIModel storedWar)
        {
            try
            {
                if (AllWars.TryGetValue(storedWar.WarID, out ICurrentWarAPIModel war))
                {
                    if (!war.IsExpired()) return war;
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
                        currentWarAPIModel = await GetCurrentWarAsync(storedWar.Clans[0].Tag, true, false);
                    }
                    catch (Exception)
                    {
                    }

                    if (currentWarAPIModel?.WarID != storedWar.WarID)
                    {
                        currentWarAPIModel = await GetCurrentWarAsync(storedWar.Clans[1].Tag, true, false);
                    }
                }

                if(currentWarAPIModel?.WarID == storedWar.WarID)
                {
                    return currentWarAPIModel;
                }

                return null;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<LeagueGroupAPIModel> GetLeagueGroupAsync(string clanTag, bool allowStoredItem = true, bool allowExpiredItem = true)
        {
            try
            {
                ValidateTag(clanTag);

                if(allowStoredItem && AllLeagueGroups.TryGetValue(clanTag, out LeagueGroupAPIModel leagueGroupAPIModel))
                {
                    if (leagueGroupAPIModel.State == LeagueState.Ended || allowExpiredItem || !leagueGroupAPIModel.IsExpired())
                    {
                        return leagueGroupAPIModel;
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

                leagueGroupAPIModel = await WebResponse.GetWebResponse<LeagueGroupAPIModel>(this, url);

                foreach(var clan in leagueGroupAPIModel.Clans.Where(x => x != null))
                {
                    AllLeagueGroups.TryAdd(clan.Tag, leagueGroupAPIModel);
                }

                return leagueGroupAPIModel;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<ICurrentWarAPIModel> GetLeagueWarAsync(string warTag, bool allowStoredItem = true, bool allowExpiredItem = true)
        {
            try
            {
                ValidateTag(warTag);

                if (allowStoredItem)
                {
                    if(AllWars.TryGetValue(warTag, out ICurrentWarAPIModel currentWarAPIModel))
                    {
                        if(allowExpiredItem || !currentWarAPIModel.IsExpired())
                        {
                            return currentWarAPIModel;
                        }
                    }
                }

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                LeagueWarAPIModel leagueWarAPIModel = await WebResponse.GetWebResponse<LeagueWarAPIModel>(this, url);

                leagueWarAPIModel.WarTag = warTag;

                if(AllWars.TryAdd(leagueWarAPIModel.WarTag, leagueWarAPIModel))
                {
                    AnnounceNewWar(leagueWarAPIModel.Clans.First().Tag, leagueWarAPIModel);  //prob doesn't matter what tag we use.  Small chance of a race condition when bot start though
                }

                foreach (var clan in leagueWarAPIModel.Clans)
                {
                    ClanAPIModel clanAPIModel = await GetClanAsync(clan.Tag);

                    clanAPIModel.Wars.TryAdd(leagueWarAPIModel.WarID, leagueWarAPIModel);
                }

                return leagueWarAPIModel;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<VillageAPIModel> GetVillageAsync(string villageTag, bool allowStoredItem = true, bool allowExpiredItem = true)
        {
            try
            {
                ValidateTag(villageTag);

                if(allowStoredItem && AllVillages.TryGetValue(villageTag, out VillageAPIModel villageAPIModel))
                {
                    if(allowExpiredItem || !villageAPIModel.IsExpired())
                    {
                        return villageAPIModel;
                    }
                }

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                villageAPIModel = await WebResponse.GetWebResponse<VillageAPIModel>(this, url);

                AllVillages.TryAdd(villageAPIModel.Tag, villageAPIModel);

                return villageAPIModel;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }            
        }

        public async Task<WarLogAPIModel> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null)
        {
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

                return await WebResponse.GetWebResponse<WarLogAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }





        public void BeginUpdatingClans()
        {
            foreach (ClanUpdateService clanUpdateService in _clanUpdateServices)
            {
                clanUpdateService.BeginUpdatingClans();
            }
        }

        public async Task StopUpdatingClans()
        {
            var tasks = new List<Task>();
                       
            foreach(ClanUpdateService clanUpdateService in _clanUpdateServices)
            {
                tasks.Add(clanUpdateService.StopUpdatingClans());
            }

            Task t = Task.WhenAll(tasks);

            await t;
        }

        public void UpdateClans(IEnumerable<string> clanTags)
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

                _clanUpdateServices.ElementAt(j).clanStrings.Add(clanTag);
                j++;
                if (j >= _cfg.NumberOfUpdaters) { j = 0; }
            }
        }

        public void UpdateClan(string clanTag)
        {
            try
            {
                ValidateTag(clanTag);
            }
            catch (Exception)
            {

                throw;
            }

            ClanUpdateService clanUpdateService = _clanUpdateServices.OrderBy(c => c.clanStrings.Count()).FirstOrDefault();

            clanUpdateService.clanStrings.Add(clanTag);
        }

        private void CreateUpdaters()
        {
            if (_cfg.NumberOfUpdaters < 1)
            {
                return;
            }

            for (int i = 0; i < _cfg.NumberOfUpdaters; i++)
            {
                ClanUpdateService clanStore = new ClanUpdateService(this);
                _clanUpdateServices.Add(clanStore);
            }
        }





        public string GetTokenStatus()
        {
            return WebResponse.GetTokenStatus();

            //return _apiHelper.GetTokenStatus();
        }

        public bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            //if (!tag.StartsWith("#"))
            //{
            //    return false;
            //}

            if(tag == "#0")
            {
                return false;
            }

            return ValidTagCharacters.IsMatch(tag);
        }

        private void AnnounceNewWar(string clanTag, ICurrentWarAPIModel currentWarAPIModel)
        {
            if(currentWarAPIModel.State == State.NotInWar || currentWarAPIModel.State == State.WarEnded)
            {
                return;
            }

            if (AllClans.TryGetValue(clanTag, out ClanAPIModel clanAPIModel))
            {
                if (clanAPIModel.AnnounceWars)
                {
                    NewWarEvent(currentWarAPIModel);
                }
            }
        }












        private void ValidateTag(string tag)
        {
            if (!IsValidTag(tag))
            {
                throw new ArgumentException("Tags must not be null nor empty and must start with a #.");
            }
        }

        private void TestConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            var _ = Task.Run(async () =>
            {
                await GetClanAsync("#929YJPYJ");
            });
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            NextTimerResetUTC = DateTime.UtcNow.AddMilliseconds(_timer.Interval);
        }

        
 

    }
}
