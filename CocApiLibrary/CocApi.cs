using CocApiLibrary.Models;
using static CocApiLibrary.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Timers;
using CocApiLibrary.Exceptions;

namespace CocApiLibrary
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);
    public class CocApi
    {
        private readonly ApiHelper apiHelper;
        private bool _isAvailable = true;
        private readonly Timer _testConnection = new Timer();
        private readonly Timer _timer;

        public event IsAvailableChangedEventHandler IsAvailableChanged;
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





        public CocApi(IEnumerable<string> tokens, int tokenTimeOutInMilliseconds, int timeToWaitForWebRequests, VerbosityType verbosityType)
        {
            try
            {
                apiHelper = new ApiHelper(timeToWaitForWebRequests, verbosityType, tokens, tokenTimeOutInMilliseconds);

                _timer = new Timer();
                _timer.Elapsed += TimerElapsed;
                //_timer.Interval = 10000;
                //_timer.Interval = 600000;
                _timer.Interval = 1800000; //30 minutes
                _timer.AutoReset = true;
                _timer.Enabled = true;
                NextTimerResetUTC = DateTime.UtcNow.AddMilliseconds(_timer.Interval);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }





        public async Task<ClanAPIModel> GetClanAsync(string clanTag, bool allowCachedItem = true)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                return await apiHelper.GetResponse<ClanAPIModel>(this, url, allowCachedItem);
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
                    url = url.Substring(0, url.Length - 1);
                }

                return await apiHelper.GetResponse<ClanSearchModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<CurrentWarAPIModel> GetCurrentWarAsync(string clanTag)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                return await apiHelper.GetResponse<CurrentWarAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<LeagueGroupAPIModel> GetLeagueGroupAsync(string clanTag)
        {
            try
            {
                ValidateTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

                return await apiHelper.GetResponse<LeagueGroupAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<CurrentWarAPIModel> GetLeagueWarAsync(string warTag)
        {
            try
            {
                ValidateTag(warTag);

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                return await apiHelper.GetResponse<CurrentWarAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }

        }

        public async Task<VillageAPIModel> GetVillageAsync(string villageTag)
        {
            try
            {
                ValidateTag(villageTag);

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                return await apiHelper.GetResponse<VillageAPIModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }            
        }

        public async Task<WarLogModel> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null)
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
                    url = url.Substring(0, url.Length - 1);
                }

                if (url.EndsWith("?"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                return await apiHelper.GetResponse<WarLogModel>(this, url);
            }
            catch (Exception e)
            {
                throw new CocApiException(e.Message, e);
            }
        }

        public string GetTokenStatus()
        {
            return apiHelper.GetTokenStatus();
        }

        public bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            if (!tag.StartsWith("#"))
            {
                return false;
            }

            if(tag == "#0")
            {
                return false;
            }

            return true;
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
