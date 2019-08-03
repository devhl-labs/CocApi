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

    public delegate void ClanChangedEventHandler(ClanAPIModel clanAPIModel);

    public delegate void MembersJoinedEventHandler(ClanAPIModel clanAPIModel, List<MemberListAPIModel> memberListAPIModels);

    public class CocApi
    {
        private bool _isAvailable = true;
        private readonly Timer _testConnection = new System.Timers.Timer();
        private readonly System.Timers.Timer _timer;
        private readonly ApiHelper _apiHelper;
        private readonly ConcurrentDictionary<string, IClanAPIModel> _clan = new ConcurrentDictionary<string, IClanAPIModel>();
        private readonly List<ClanStore> _clanStores = new List<ClanStore>();
        internal readonly ConcurrentDictionary<string, StoredItem> clans = new ConcurrentDictionary<string, StoredItem>();
        internal IMapper Mapper;
        public event IsAvailableChangedEventHandler IsAvailableChanged;
        public event ClanChangedEventHandler ClanChanged;
        public event MembersJoinedEventHandler MembersJoined;

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
                    IsAvailableChanged(value);

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


        public void ClanChangedMethod(ClanAPIModel clanAPIModel)
        {
            ClanChanged(clanAPIModel);
        }

        public void MembersJoinedEvent(ClanAPIModel clanAPIModel, List<MemberListAPIModel> memberListAPIModels)
        {
            MembersJoined(clanAPIModel, memberListAPIModels);
        }

        public CocApi(IEnumerable<string> tokens, int tokenTimeOutInMilliseconds, int timeToWaitForWebRequests, VerbosityType verbosityType)
        {
            WebResponse.Initialize(timeToWaitForWebRequests, verbosityType, tokens, tokenTimeOutInMilliseconds);

            _apiHelper = new ApiHelper(timeToWaitForWebRequests, verbosityType, tokens, tokenTimeOutInMilliseconds);

            _timer = new Timer();
            _timer.Elapsed += TimerElapsed;
            _timer.Interval = 1800000; //30 minutes
            _timer.AutoReset = true;
            _timer.Enabled = true;

            NextTimerResetUTC = DateTime.UtcNow.AddMilliseconds(_timer.Interval);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ClanAPIModel, ClanAPIModel>().ForMember(x => x.Members, opt => opt.Ignore()).ForMember(x => x.Wars, opt => opt.Ignore());
            });
            Mapper = config.CreateMapper();
            
        }





        public void Monitor(bool enabled)
        {
            foreach(ClanStore clanStore in _clanStores)
            {
                clanStore.Update(enabled);
            }
        }

        public void MonitorClans(IEnumerable<string> clanTags, int numOfUpdaters)
        {
            if (numOfUpdaters < 1)
            {
                return;
            }

            for (int i = 0; i < numOfUpdaters; i++)
            {
                ClanStore clanStore = new ClanStore(this, Mapper);
                _clanStores.Add(clanStore);
            }

            int j = 0;

            foreach (string clanTag in clanTags)
            {
                _clanStores.ElementAt(j).clanStrings.Add(clanTag);
                j++;
                if (j >= numOfUpdaters) { j = 0; }
            }
        }

        public async Task<ClanAPIModel?> GetClanAsync(string clanTag, bool allowCachedItem = true)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                //return await _apiHelper.GetResponse<ClanAPIModel>(this, url, allowCachedItem);


                clans.TryGetValue(clanTag, out var clanAPIModel);
                (clanAPIModel.DownloadedItem as ClanAPIModel).Process(this);
                return clanAPIModel.DownloadedItem as ClanAPIModel;
                
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<ClanSearchModel?> GetClansAsync(string? clanName = null
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

                //return (await _webResponse.GetWebResponse<ClanSearchModel>(this, url)).DownloadedItem;

                return await _apiHelper.GetResponse<ClanSearchModel>(this, url, true);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<CurrentWarAPIModel?> GetCurrentWarAsync(string clanTag)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                CurrentWarAPIModel? currentWarAPIModel = await _apiHelper.GetResponse<CurrentWarAPIModel>(this, url, true);

                if(currentWarAPIModel != null)
                {
                    currentWarAPIModel.Process();
                }

                return currentWarAPIModel;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<LeagueGroupAPIModel?> GetLeagueGroupAsync(string clanTag)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

                return await _apiHelper.GetResponse<LeagueGroupAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<LeagueWarAPIModel?> GetLeagueWarAsync(string warTag)
        {
            try
            {
                ValidateTag(warTag);

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                LeagueWarAPIModel? leagueWar = await _apiHelper.GetResponse<LeagueWarAPIModel>(this, url);

                if(leagueWar != null)
                {
                    leagueWar.Process();

                    leagueWar.WarTag = warTag;
                }

                return leagueWar;
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<VillageAPIModel?> GetVillageAsync(string villageTag)
        {
            try
            {
                ValidateTag(villageTag);

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                return await _apiHelper.GetResponse<VillageAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }            
        }

        public async Task<WarLogModel?> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null)
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

                return await _apiHelper.GetResponse<WarLogModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public string GetTokenStatus()
        {
            //return _webResponse.GetTokenStatus();

            return _apiHelper.GetTokenStatus();
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
