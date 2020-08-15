using Dapper.SqlWriter;
using CocApi.Cache.Exceptions;
using CocApi.Cache.Models;
using CocApi.Cache.Models.Cache;
using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using CocApi.Cache.Models.Wars;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    internal class WebResponse
    {
        public WebResponse(CocApiClient_old cocApi, CocApiConfiguration cfg, IEnumerable<string> tokens)
        {
            _cocApi = cocApi;

            HttpClient.BaseAddress = new Uri(cfg.ClashApiBaseAddress);

            _config = cfg;

            HttpClient.DefaultRequestHeaders.Accept.Clear();

            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (string token in tokens)
            {
                TokenObject_old tokenObject = new TokenObject_old(cocApi, token, _config.TokenTimeOut);

                _tokenObjects.Add(tokenObject);
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };
        }

        private readonly List<TokenObject_old> _tokenObjects = new List<TokenObject_old>();

        private readonly CocApiConfiguration _config;

        public HttpClient HttpClient { get; } = new HttpClient();

        public SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        public ConcurrentBag<WebResponseTimer> WebResponseTimers { get; } = new ConcurrentBag<WebResponseTimer>();

        private readonly CocApiClient_old _cocApi;

        private readonly ConcurrentDictionary<string, string> _updatingCache = new ConcurrentDictionary<string, string>();

        public async Task CacheAsync(string path, string json, EndPoint endPoint, DateTime serverExpiration)
        {
            if (!(_updatingCache.TryAdd(path, json)))
                return;

            try
            {
                CachedItem_old? cache = await _cocApi.SqlWriter.Select<CachedItem_old>()
                                                    .Where(c => c.Path == path)
                                                    .QueryFirstOrDefaultAsync()
                                                    .ConfigureAwait(false);

                if (cache == null)
                {
                    cache = new CachedItem_old(path, json, endPoint, serverExpiration);

                    await _cocApi.SqlWriter.Insert(cache).ExecuteAsync().ConfigureAwait(false);
                }
                else if (cache.ServerExpiration < serverExpiration)
                {
                    cache = new CachedItem_old(path, json, endPoint, serverExpiration);

                    await _cocApi.SqlWriter.Update(cache).ExecuteAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _updatingCache.TryRemove(path, out _);
            }
        }

        public async Task<IDownloadable?> GetDownloadableAsync<TValue>(string encodedUrl, TokenObject_old token, CancellationToken cancellationToken) where TValue : IDownloadable /*, new()*/
        {
            EndPoint endPoint = GetEndPoint<TValue>(encodedUrl);

            try
            {
                using HttpResponseMessage response = await GetHttpResponseAsync(endPoint, encodedUrl, token, cancellationToken).ConfigureAwait(false);
                
                IDownloadable? result = null;

                if (response.IsSuccessStatusCode)
                {
                    result = SuccessfulResponse<TValue>(response, encodedUrl, endPoint);
                }
                else
                {
                    result = UnSuccessfulResponse(response, encodedUrl, endPoint, token);
                }

                if (result != null)
                {
                    if (result is LeagueGroup leagueGroup)
                        foreach (LeagueClan? clan in leagueGroup.Clans.EmptyIfNull())
                            await CacheAsync(LeagueGroup.Url(clan.ClanTag), result.ToJson(), endPoint, result.ServerExpirationUtc).ConfigureAwait(false);
                    else if (result is LeagueWar leagueWar)
                    {
                        leagueWar.WarTag = LeagueWar.WarTagFromUrl(encodedUrl);

                        leagueWar.GroupKey = await _cocApi.Wars.GetWarKeyAsync(leagueWar);

                        await CacheAsync(encodedUrl, leagueWar.ToJson(), EndPoint.LeagueWar, leagueWar.ServerExpirationUtc).ConfigureAwait(false);
                    }
                    else
                        await CacheAsync(encodedUrl, result.ToJson(), endPoint, result.ServerExpirationUtc).ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception e)
            {
                return await ErrorInResponse(e, encodedUrl, endPoint).ConfigureAwait(false);
            }
        }

        public ImmutableArray<WebResponseTimer> GetTimers() => WebResponseTimers.ToImmutableArray();

        public string GetTokenStatus() => $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";

        internal async Task<TokenObject_old> GetTokenAsync()
        {
            await SemaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                while (_tokenObjects.All(x => x.IsRateLimited))
                    await Task.Delay(50).ConfigureAwait(false);

                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private async Task<IDownloadable?> ErrorInResponse(Exception e, string encodedUrl, EndPoint endPoint)
        {
            if (e is ServerResponseException serverResponse)
            {
                _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(ErrorInResponse), LogLevel.Debug, $"{encodedUrl} {serverResponse.HttpStatusCode} {e.Message}"));
            }
            else
            {
                _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(ErrorInResponse), LogLevel.Debug, $"{encodedUrl} {e.Message}"));
            }

            if (e is TaskCanceledException && endPoint == EndPoint.LeagueGroup)
            {
                //there is a bug while the clan is searching where the api returns nothing
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, null);

                await CacheAsync(encodedUrl, leagueGroupNotFound.ToJson(), EndPoint.LeagueGroup, leagueGroupNotFound.ServerExpirationUtc).ConfigureAwait(false);

                return leagueGroupNotFound;
            }
            else if (e is TaskCanceledException)
            {
                return null;
            }

            if (e is CocApiException)
                throw e;

            throw new CocApiException(e.Message, e);
        }

        private EndPoint GetEndPoint<T>(string encodedUrl)
        {
            if (typeof(T) == typeof(IWar) && encodedUrl.StartsWith("clanwarleagues/wars/"))
                return EndPoint.LeagueWar;

            if (typeof(T) == typeof(IWar) && encodedUrl.EndsWith("warlog"))
                return EndPoint.WarLogEntries;

            if (typeof(T) == typeof(IWar))
                return EndPoint.CurrentWar;

            //if (typeof(T) == typeof(CurrentWar))
            //    return EndPoint.CurrentWar;

            //if (typeof(T) == typeof(LeagueWar))
            //    return EndPoint.LeagueWar;

            if (typeof(T) == typeof(ILeagueGroup))
                return EndPoint.LeagueGroup;

            //if (typeof(T) == typeof(LeagueGroup))
            //    return EndPoint.LeagueGroup;

            if (typeof(T) == typeof(Clan))
                return EndPoint.Clan;

            if (typeof(T) == typeof(Village))
                return EndPoint.Village;

            if (typeof(T) == typeof(Paginated<Clan>))
                return EndPoint.Clans;

            if (typeof(T) == typeof(Paginated<Label>))
                return EndPoint.Labels;

            if (typeof(T) == typeof(Paginated<Location>))
                return EndPoint.Locations;

            if (typeof(T) == typeof(Paginated<WarLeague>))
                return EndPoint.WarLeagues;

            //if (typeof(T) == typeof(Paginated<WarLogEntry>))
            //    return EndPoint.WarLogEntries;

            if (typeof(T) == typeof(Paginated<TopBuilderVillage>))
                return EndPoint.TopBuilderVillages;

            if (typeof(T) == typeof(Paginated<TopMainVillage>))
                return EndPoint.TopMainVillages;

            if (typeof(T) == typeof(Paginated<TopBuilderClan>))
                return EndPoint.TopBuilderClans;

            if (typeof(T) == typeof(Paginated<TopMainClan>))
                return EndPoint.TopMainClans;

            if (typeof(T) == typeof(Paginated<League>))
                return EndPoint.VillageLeagues;

            throw new CocApiException("End point not found.");
        }

        private async Task<HttpResponseMessage> GetHttpResponseAsync(EndPoint endPoint, string encodedUrl, TokenObject_old token, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, new Uri(HttpClient.BaseAddress, encodedUrl));

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            HttpResponseMessage response;

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            stopwatch.Start();

            try
            {
                response = await HttpClient.SendAsync(message, cts.Token);
            }
            catch (Exception)
            {
                stopwatch.Stop();

                WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, null));

                throw;
            }

            stopwatch.Stop();

            WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

            return response;
        }

        private void InitializeCacheExpiration(IDownloadable iDownloadable, HttpResponseMessage? response)
        {
            Downloadable downloadable = (Downloadable)iDownloadable;

            downloadable.DownloadedAtUtc = DateTime.UtcNow;

            if (response == null || response.Headers == null || response.Headers.Date == null)
                return;

            downloadable.DownloadedAtUtc = response.Headers.Date.Value.UtcDateTime;

            if (response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge != null)
            {
                //adding 3 seconds incase the server clock is different than our clock
                downloadable.ServerExpirationUtc = response.Headers.Date.Value.DateTime.Add(response.Headers.CacheControl.MaxAge.Value) + TimeSpan.FromSeconds(3);
            }
        }

        private void InitializeResult<T>(T result, HttpResponseMessage? response) where T : IDownloadable
        {
            InitializeCacheExpiration(result, response);

            if (result is IInitialize initialize)
                initialize.Initialize(_cocApi);
        }

        private IDownloadable SuccessfulResponse<TValue>(HttpResponseMessage response, string encodedUrl, EndPoint endPoint) where TValue : IDownloadable
        {
            _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(SuccessfulResponse), LogLevel.Information, encodedUrl));

            _cocApi.IsAvailable = true;

            string responseText = response.Content.ReadAsStringAsync().Result;

            IDownloadable result;

            if (typeof(TValue) == typeof(IWar) && endPoint == EndPoint.LeagueWar)
                result = JsonConvert.DeserializeObject<LeagueWar>(responseText);
            else if (typeof(TValue) == typeof(IWar) && endPoint == EndPoint.CurrentWar)
                result = JsonConvert.DeserializeObject<CurrentWar>(responseText);
            else if (typeof(TValue) == typeof(ILeagueGroup))
                result = JsonConvert.DeserializeObject<LeagueGroup>(responseText);
            else if (typeof(TValue) == typeof(IWar) && endPoint == EndPoint.WarLogEntries)
                result = JsonConvert.DeserializeObject<WarLog>(responseText);
            else
                result = JsonConvert.DeserializeObject<TValue>(responseText);

            if (result != null)
                if (result is CurrentWar currentWar && currentWar.PreparationStartTimeUtc == DateTime.MinValue)
                {
                    var notInWar = new NotInWar();

                    InitializeResult(notInWar, response);

                    return notInWar;
                }
                else
                {
                    InitializeResult(result, response);

                    return result;
                }
            else
                throw new CocApiException("The response could not be parsed.");
        }

        private IDownloadable? UnSuccessfulResponse(HttpResponseMessage response, string encodedUrl, EndPoint endPoint, TokenObject_old token)
        {
            string responseText = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.BadGateway ||
                     response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                     response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                _cocApi.IsAvailable = false;

                _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Warning, $"{encodedUrl} {response.StatusCode}"));

                return null;
            }

            ResponseMessage? ex = GetResponseMessage(responseText);

            if (ex == null)
            {
                _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Warning, $"{encodedUrl} {response.StatusCode} An error occured while parsing the response."));

                return null;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                if (endPoint == EndPoint.CurrentWar || endPoint == EndPoint.WarLogEntries) //todo the WarLog method does not return an interface so we cant return a PrivateWarLog
                {
                    var privateWar = new PrivateWarLog();

                    InitializeResult(privateWar, response);

                    return privateWar;
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound && endPoint == EndPoint.LeagueGroup)
            {
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, response);

                _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Debug, $"{encodedUrl} league group not found"));

                return leagueGroupNotFound;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                token.IsRateLimited = true;
            }
            //else if (response.StatusCode == HttpStatusCode.BadGateway ||
            //         response.StatusCode == HttpStatusCode.ServiceUnavailable ||
            //         response.StatusCode == HttpStatusCode.GatewayTimeout)
            //{
            //    _cocApi.IsAvailable = false;
            //}

            _cocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Warning, $"{encodedUrl} {ex.Reason}: {ex.Message}"));

            return null;
        }

        private ResponseMessage? GetResponseMessage(string responseText)
        {
            try
            {
                return JsonConvert.DeserializeObject<ResponseMessage>(responseText);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}