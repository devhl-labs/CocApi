using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    internal static class WebResponse
    {
        private static List<TokenObject> TokenObjects { get; } = new List<TokenObject>();

        private static CocApiConfiguration Config { get; set; } = new CocApiConfiguration();

        public static HttpClient HttpClient { get; } = new HttpClient();

        public static SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        public static ConcurrentBag<WebResponseTimer> WebResponseTimers { get; } = new ConcurrentBag<WebResponseTimer>();

#nullable disable

        private static CocApi CocApi { get; set; }

#nullable enable

        private static ConcurrentDictionary<string, string> UpdatingCache { get; set; } = new ConcurrentDictionary<string, string>();

        public static async Task CacheAsync(string path, string json, EndPoint endPoint, DateTime serverExpiration)
        {
            if (!(UpdatingCache.TryAdd(path, json)))
                return;

            try
            {
                Cache? cache = await CocApi.SqlWriter.Select<Cache>()
                                                    .Where(c => c.Path == path)
                                                    .QueryFirstOrDefaultAsync()
                                                    .ConfigureAwait(false);

                if (cache == null)
                {
                    cache = new Cache(path, json, endPoint, serverExpiration);

                    await CocApi.SqlWriter.Insert(cache).ExecuteAsync().ConfigureAwait(false);
                }
                else if (cache.ServerExpiration < serverExpiration)
                {
                    cache = new Cache(path, json, endPoint, serverExpiration);

                    await CocApi.SqlWriter.Update(cache).ExecuteAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                UpdatingCache.TryRemove(path, out _);
            }
        }

        public static async Task<IDownloadable?> GetDownloadableAsync<TValue>(string encodedUrl, TokenObject token, CancellationToken cancellationToken) where TValue : IDownloadable /*, new()*/
        {
            EndPoint endPoint = GetEndPoint<TValue>();

            try
            {
                using HttpResponseMessage response = await GetHttpResponseAsync(endPoint, encodedUrl, token, cancellationToken).ConfigureAwait(false);

                IDownloadable? result = null;

                if (response.IsSuccessStatusCode)
                {
                    result = SuccessfulResponse<TValue>(response, encodedUrl);
                }
                else
                {
                    result = await UnSuccessfulResponse(response, encodedUrl, endPoint, token).ConfigureAwait(false);
                }

                if (result != null)
                {
                    if (result is LeagueGroup leagueGroup)
                        foreach (LeagueClan? clan in leagueGroup.Clans.EmptyIfNull())
                            await CacheAsync(LeagueGroup.Url(clan.ClanTag), result.ToJson(), endPoint, result.ServerExpirationUtc).ConfigureAwait(false);
                    else if (!(result is LeagueWar)) //cache league wars later so the wartag can be populated
                        await CacheAsync(encodedUrl, result.ToJson(), endPoint, result.ServerExpirationUtc).ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception e)
            {
                return await ErrorInResponse(e, encodedUrl, endPoint).ConfigureAwait(false);
            }
        }

        public static ImmutableList<WebResponseTimer> GetTimers() => WebResponseTimers.ToImmutableList();

        public static string GetTokenStatus() => $"{TokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{TokenObjects.Count(x => !x.IsRateLimited)} not rate limited";

        public static void Initialize(CocApi cocApi, CocApiConfiguration cfg, IEnumerable<string> tokens)
        {
            HttpClient.BaseAddress = new Uri(cfg.ClashApiBaseAddress);

            CocApi = cocApi;

            Config = cfg;

            HttpClient.DefaultRequestHeaders.Accept.Clear();

            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (string token in tokens)
            {
                TokenObject tokenObject = new TokenObject(cocApi, token, Config.TokenTimeOut);

                TokenObjects.Add(tokenObject);
            }

            //_jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    //new DateTimeConverter() ,
                    //new LeagueSeasonConverter(),
                    new StringEnumConverter()
                    //new LeagueStateConverter(),
                    //new ResultConverter(),
                    //new RoleConverter(),
                    //new WarStateConverter()
                }
            };
        }

        internal static async Task<TokenObject> GetTokenAsync()
        {
            await SemaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                while (TokenObjects.All(x => x.IsRateLimited))
                    await Task.Delay(50).ConfigureAwait(false);

                return await TokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private static async Task<IDownloadable?> ErrorInResponse(Exception e, string encodedUrl, EndPoint endPoint)
        {
            if (e is ServerResponseException serverResponse)
            {
                CocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(ErrorInResponse), LogLevel.Debug, $"{encodedUrl} {serverResponse.HttpStatusCode} {e.Message}"));
            }
            else
            {
                CocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(ErrorInResponse), LogLevel.Debug, $"{encodedUrl} {e.Message}"));
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

        private static EndPoint GetEndPoint<T>()
        {
            if (typeof(T) == typeof(CurrentWar))
                return EndPoint.CurrentWar;

            if (typeof(T) == typeof(LeagueWar))
                return EndPoint.LeagueWar;

            if (typeof(T) == typeof(LeagueGroup))
                return EndPoint.LeagueGroup;

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

            if (typeof(T) == typeof(Paginated<WarLogEntry>))
                return EndPoint.WarLogEntries;

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

        private static async Task<HttpResponseMessage> GetHttpResponseAsync(EndPoint endPoint, string encodedUrl, TokenObject token, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            stopwatch.Start();

            HttpResponseMessage response;

            try
            {
                response = await HttpClient.GetAsync(encodedUrl, cts.Token).ConfigureAwait(false);
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

        private static void InitializeCacheExpiration(IDownloadable iDownloadable, HttpResponseMessage? response)
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

        private static void InitializeResult<T>(T result, HttpResponseMessage? response) where T : IDownloadable
        {
            InitializeCacheExpiration(result, response);

            if (result is IInitialize initialize)
                initialize.Initialize(CocApi);
        }

        private static IDownloadable SuccessfulResponse<TValue>(HttpResponseMessage response, string encodedUrl) where TValue : IDownloadable /*, new()*/
        {
            CocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(SuccessfulResponse), LogLevel.Information, encodedUrl));

            CocApi.IsAvailable = true;

            string responseText = response.Content.ReadAsStringAsync().Result;

            TValue result = JsonConvert.DeserializeObject<TValue>(responseText);

            if (result != null)
            {
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
            }
            else
            {
                throw new CocApiException("The response could not be parsed.");
            }
        }

        private static async Task<IDownloadable?> UnSuccessfulResponse(HttpResponseMessage response, string encodedUrl, EndPoint endPoint, TokenObject token)
        {
            string responseText = response.Content.ReadAsStringAsync().Result;

            ResponseMessage ex = JsonConvert.DeserializeObject<ResponseMessage>(responseText);

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

                CocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Debug, $"{encodedUrl} league group not found"));

                await CacheAsync(encodedUrl, leagueGroupNotFound.ToJson(), EndPoint.LeagueGroup, leagueGroupNotFound.ServerExpirationUtc);

                return leagueGroupNotFound;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                token.IsRateLimited = true;
            }
            else if (response.StatusCode == HttpStatusCode.BadGateway ||
                     response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                     response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                CocApi.IsAvailable = false;
            }

            CocApi.OnLog(new LogEventArgs(nameof(WebResponse), nameof(UnSuccessfulResponse), LogLevel.Warning, $"{encodedUrl} {ex.Reason}: {ex.Message}"));

            return null;
        }
    }
}