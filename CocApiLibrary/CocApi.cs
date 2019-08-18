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

namespace CocApiLibrary
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);
    public delegate void ClanChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void MembersJoinedEventHandler(ClanAPIModel oldClan, List<MemberListAPIModel> memberListAPIModels);
    public delegate void MembersLeftEventHandler(ClanAPIModel oldClan, List<MemberListAPIModel> memberListAPIModels);
    public delegate void ClanBadgeUrlChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void ClanLocationChangedEventHandler(ClanAPIModel oldClan, ClanAPIModel newClan);
    public delegate void NewWarEventHandler(ClanAPIModel oldClan, ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarChangedEventHandler(ICurrentWarAPIModel oldWar, ICurrentWarAPIModel newWar);
    public delegate void NewAttacksEventHandler(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels);
    public delegate void WarEndingSoonEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarStartingSoonEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void ClanVersusPointsChangedEventHandler(ClanAPIModel oldClan, int newClanVersusPoints);
    public delegate void ClanPointsChangedEventHandler(ClanAPIModel oldClan, int newClanPoints);
    public delegate void WarIsAccessibleChangedEventHandler(ICurrentWarAPIModel currentWarAPIModel);
    public delegate void WarEndNotSeenEventHandler(ICurrentWarAPIModel currentWarAPIModel);

    public class CocApi
    {
        private bool _isAvailable = true;
        private readonly Timer _testConnection = new System.Timers.Timer();
        private readonly System.Timers.Timer _timer;
        private readonly List<ClanUpdateService> _clanUpdateServices = new List<ClanUpdateService>();

        internal readonly ConcurrentDictionary<string, ClanAPIModel> AllClans = new ConcurrentDictionary<string, ClanAPIModel>();
        internal readonly Dictionary<string, ICurrentWarAPIModel> AllWars = new Dictionary<string, ICurrentWarAPIModel>();


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

        public static readonly Regex ValidTagCharacters = new Regex(@"^#[PYLQGRJCUV0289]+$");

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




        public CocApi(IEnumerable<string> tokens, int tokenTimeOutInMilliseconds, int timeToWaitForWebRequests, VerbosityType verbosityType)
        {
            WebResponse.Initialize(timeToWaitForWebRequests, verbosityType, tokens, tokenTimeOutInMilliseconds);

            //_apiHelper = new ApiHelper(timeToWaitForWebRequests, verbosityType, tokens, tokenTimeOutInMilliseconds);

            _timer = new Timer();
            _timer.Elapsed += TimerElapsed;
            _timer.Interval = 1800000; //30 minutes
            _timer.AutoReset = true;
            _timer.Enabled = true;

            NextTimerResetUTC = DateTime.UtcNow.AddMilliseconds(_timer.Interval);

            //var config = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<ClanAPIModel, ClanAPIModel>().Ignore(c => c.BadgeUrls!).Ignore(c => c.Location!).Ignore(c => c.Members!).Ignore(c => c.Wars);
            //    cfg.CreateMap<BadgeUrlModel, BadgeUrlModel>();
            //    cfg.CreateMap<LocationModel, LocationModel>();
            //    cfg.CreateMap<CurrentWarAPIModel, CurrentWarAPIModel>().Ignore(c => c.Attacks);

            //});
            //Mapper = config.CreateMapper();

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

        internal void NewWarEvent(ClanAPIModel newClan, ICurrentWarAPIModel currentWarAPIModel)
        {
            NewWar?.Invoke(newClan, currentWarAPIModel);
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

                        clanAPIModel.Wars.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel);
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

        public async Task<LeagueGroupAPIModel> GetLeagueGroupAsync(string clanTag) //todo add a store
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

                return await WebResponse.GetWebResponse<LeagueGroupAPIModel>(this, url);
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

                AllWars.TryAdd(leagueWarAPIModel.WarTag, leagueWarAPIModel);

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

        public async Task<VillageAPIModel> GetVillageAsync(string villageTag)  //todo add a store for this
        {
            try
            {
                ValidateTag(villageTag);

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                return await WebResponse.GetWebResponse<VillageAPIModel>(this, url);
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





        public void Monitor(bool enabled)
        {
            foreach (ClanUpdateService clanStore in _clanUpdateServices)
            {
                clanStore.Update(enabled);
            }
        }

        public void Monitor(IEnumerable<string> clanTags, int numOfUpdaters)
        {
            if (numOfUpdaters < 1)
            {
                return;
            }

            for (int i = 0; i < numOfUpdaters; i++)
            {
                ClanUpdateService clanStore = new ClanUpdateService(this);
                _clanUpdateServices.Add(clanStore);
            }

            int j = 0;

            foreach (string clanTag in clanTags)
            {
                _clanUpdateServices.ElementAt(j).clanStrings.Add(clanTag);
                j++;
                if (j >= numOfUpdaters) { j = 0; }
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
