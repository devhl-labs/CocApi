using devhl.CocApi.Converters;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    internal static class WebResponse
    {
        private static readonly List<TokenObject> _tokenObjects = new List<TokenObject>();

        private static CocApiConfiguration _cfg = new CocApiConfiguration();

#nullable disable

        private static CocApi _cocApi;

#nullable enable

        public static HttpClient HttpClient { get; } = new HttpClient();

        public static SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        public static ConcurrentBag<WebResponseTimer> WebResponseTimers { get; } = new ConcurrentBag<WebResponseTimer>();

        public static async Task<IDownloadable?> GetDownloadableAsync<TValue>(string encodedUrl, TokenObject token, CancellationToken cancellationToken) where TValue : IDownloadable /*, new()*/
        {
            EndPoint endPoint = GetEndPoint<TValue>();

            try
            {
                using HttpResponseMessage response = await GetHttpResponseAsync(endPoint, encodedUrl, token, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return SuccessfulResponse<TValue>(response, encodedUrl);
                }
                else
                {
                    return UnSuccessfulResponse(response, encodedUrl, endPoint, token);
                }
            }
            catch (Exception e)
            {
                return ErrorInResponse(e, encodedUrl, endPoint);
            }
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

        public static ConcurrentBag<WebResponseTimer> GetTimers() => WebResponseTimers;

        public static string GetTokenStatus() => $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";

        public static void Initialize(CocApi cocApi, CocApiConfiguration cfg, IEnumerable<string> tokens)
        {
            _cocApi = cocApi;

            _cfg = cfg;

            HttpClient.DefaultRequestHeaders.Accept.Clear();

            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (string token in tokens)
            {
                TokenObject tokenObject = new TokenObject(cocApi, token, _cfg.TokenTimeOut);

                _tokenObjects.Add(tokenObject);
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

        private static IDownloadable ErrorInResponse(Exception e, string encodedUrl, EndPoint endPoint)
        {
            if (e is ServerResponseException serverResponse)
            {
                _cocApi.LogEvent("WebResponse", LogLevel.Debug, LoggingEvent.HttpResponseError, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} {serverResponse.HttpStatusCode} {e.Message}");
            }
            else
            {
                _cocApi.LogEvent("WebResponse", LogLevel.Debug, LoggingEvent.HttpResponseError, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} {e.Message}");
            }

            if (e is TaskCanceledException && endPoint == EndPoint.LeagueGroup)
            {
                //there is a bug while the clan is searching where the api returns nothing
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, null, encodedUrl);

                return leagueGroupNotFound;
            }
            else if (e is TaskCanceledException)
            {
                ResponseMessage responseMessageApiModel = new ResponseMessage
                {
                    Message = e.Message,

                    Reason = e.ToString()
                };

                throw new ServerTookTooLongToRespondException(responseMessageApiModel, null);
            }

            if (e is CocApiException) throw e;

            throw new CocApiException(e.Message, e);
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

        internal static async Task<TokenObject> GetTokenAsync()
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

        private static void InitializeCacheExpiration(IDownloadable iDownloadable, HttpResponseMessage? response)
        {
            Downloadable downloadable = (Downloadable) iDownloadable;

            downloadable.DownloadedAtUtc = DateTime.UtcNow;

            if (response == null || response.Headers == null || response.Headers.Date == null) return;

            downloadable.DownloadedAtUtc = response.Headers.Date.Value.UtcDateTime;

            if (response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge != null)
            {
                //adding 3 seconds incase the server clock is different than our clock
                downloadable.ServerExpirationUtc = response.Headers.Date.Value.DateTime.Add(response.Headers.CacheControl.MaxAge.Value) + TimeSpan.FromSeconds(3);
            }            
        }

        private static void InitializeDownloadableProperties(IDownloadable iDownloadable, string encodedURL)
        {
            Downloadable downloadable = (Downloadable)iDownloadable;

            downloadable.EncodedUrl = encodedURL;

            switch (downloadable)
            {
                //case ServiceUnavailable serviceUnavailable:  //todo didn't i get rid of this?
                //    serviceUnavailable.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.ServiceUnavailableTimeToLive);
                //    break;

                case LeagueWar leagueWarApiModel:
                    if (leagueWarApiModel.State == WarState.WarEnded)
                    {
                        leagueWarApiModel.LocalExpirationUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        leagueWarApiModel.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.LeagueWarTimeToLive);
                    }

                    break;

                case CurrentWar currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.LocalExpirationUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        currentWar.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.CurrentWarTimeToLive);
                    }

                    break;

                case LeagueGroup leagueGroupApiModel:
                    if (leagueGroupApiModel.State == LeagueState.WarsEnded)
                    {
                        leagueGroupApiModel.LocalExpirationUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
                    }
                    else
                    {
                        leagueGroupApiModel.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupTimeToLive);
                    }

                    break;

                case LeagueGroupNotFound leagueGroupNotFound:
                    leagueGroupNotFound.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupNotFoundTimeToLive);
                    break;

                case Clan clanApiModel:
                    clanApiModel.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.ClanTimeToLive);
                    break;

                case Village villageApiModel:
                    villageApiModel.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.VillageTimeToLive);
                    break;

                case NotInWar notInWar:
                    notInWar.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.NotInWarTimeToLive);
                    break;

                case PrivateWarLog privateWarLog:
                    privateWarLog.LocalExpirationUtc = DateTime.UtcNow.Add(_cfg.PrivateWarLogTimeToLive);
                    break;

                default:
                    downloadable.LocalExpirationUtc = DateTime.UtcNow;
                    break;
            }
        }

        private static void InitializeResult<T>(T result, HttpResponseMessage? response, string encodedUrl) where T : IDownloadable /*, new()*/
        {
            InitializeDownloadableProperties(result, encodedUrl);

            InitializeCacheExpiration(result, response);

            if (result is IInitialize initialize) initialize.Initialize(_cocApi);
        }

        private static IDownloadable SuccessfulResponse<TValue>(HttpResponseMessage response, string encodedUrl) where TValue : IDownloadable /*, new()*/
        {
            _cocApi.LogEvent("WebResponse", LogLevel.Information, LoggingEvent.HttpResponseStatusCodeSuccessful, encodedUrl.Replace("https://api.clashofclans.com/v1", ""));

            _cocApi.IsAvailable = true;

            string responseText = response.Content.ReadAsStringAsync().Result;

            TValue result = JsonConvert.DeserializeObject<TValue>(responseText);

            if (result != null)
            {
                if (result is CurrentWar currentWar && currentWar.PreparationStartTimeUtc == DateTime.MinValue)
                {
                    var notInWar = new NotInWar();

                    InitializeResult(notInWar, response, encodedUrl);

                    return notInWar;
                }
                else
                {
                    InitializeResult(result, response, encodedUrl);

                    return result;
                }
            }
            else
            {
                throw new CocApiException("The response could not be parsed.");
            }
        }

        private static IDownloadable? UnSuccessfulResponse(HttpResponseMessage response, string encodedUrl, EndPoint endPoint, TokenObject token)
        {
            string responseText = response.Content.ReadAsStringAsync().Result;

            ResponseMessage ex = JsonConvert.DeserializeObject<ResponseMessage>(responseText);

            //if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            //{
                //throw new BadRequestException(ex, response.StatusCode);

                //_cocApi.LogEvent("WebResponse", LogLevel.Information, LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{ex.Reason}: {ex.Message}");
            //}
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (endPoint == EndPoint.CurrentWar || endPoint == EndPoint.WarLogEntries) //todo the WarLog method does not return an interface
                {
                    var privateWar = new PrivateWarLog();

                    InitializeResult(privateWar, response, encodedUrl);

                    return privateWar;
                }

                //throw new ForbiddenException(ex, response.StatusCode);

                //_cocApi.LogEvent("WebResponse", LogLevel.Information, LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{ex.Reason}: {ex.Message}");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && endPoint == EndPoint.LeagueGroup)
            {
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, response, encodedUrl);

                _cocApi.LogEvent("WebResponse", LogLevel.Debug, LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} league group not found");

                return leagueGroupNotFound;
            }
            //else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            //{
                //throw new NotFoundException(ex, response.StatusCode);

                //_cocApi.LogEvent("WebResponse", LogLevel.Information, LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{ex.Reason}: {ex.Message}");
            //}
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                token.IsRateLimited = true;

                //throw new TooManyRequestsException(ex, response.StatusCode);
            }
            //else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            //{
                //throw new InternalServerErrorException(ex, response.StatusCode);
            //}
            else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
            {
                _cocApi.IsAvailable = false;

                //throw new BadGateWayException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _cocApi.IsAvailable = false;

                //ServiceUnavailable serviceUnavailable = new ServiceUnavailable();

                //InitializeResult(serviceUnavailable, response, encodedUrl);

                //return serviceUnavailable;

                //throw new ServiceUnavailableException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            {
                _cocApi.IsAvailable = false;

                //throw new GatewayTimeoutException(ex, response.StatusCode);
            }

            //throw new ServerResponseException(ex, response.StatusCode);

            _cocApi.LogEvent("WebResponse", LogLevel.Information, LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{ex.Reason}: {ex.Message}");

            return null;
        }
    }
}