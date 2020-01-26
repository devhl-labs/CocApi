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

        public static HttpClient ApiClient { get; } = new HttpClient();

        public static SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        public static ConcurrentBag<WebResponseTimer> WebResponseTimers { get; } = new ConcurrentBag<WebResponseTimer>();

        public static async Task<Downloadable> GetDownloadableAsync<TValue>(EndPoint endPoint, string encodedUrl, CancellationToken cancellationToken) where TValue : Downloadable, new()
        {
            try
            {
                TokenObject token = await GetTokenAsync(endPoint, encodedUrl).ConfigureAwait(false);

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

        public static ConcurrentBag<WebResponseTimer> GetTimers() => WebResponseTimers;

        public static string GetTokenStatus() => $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";

        public static void Initialize(CocApi cocApi, CocApiConfiguration cfg, IEnumerable<string> tokens)
        {
            _cocApi = cocApi;

            _cfg = cfg;

            ApiClient.DefaultRequestHeaders.Accept.Clear();

            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
                    new DateTimeConverter() ,
                    new LeagueSeasonConverter(),
                    new StringEnumConverter()
                    //new LeagueStateConverter(),
                    //new ResultConverter(),
                    //new RoleConverter(),
                    //new WarStateConverter()
                }
            };
        }

        private static Downloadable ErrorInResponse(Exception e, string encodedUrl, EndPoint endPoint)
        {
            if (e is ServerResponseException serverResponse)
            {
                _ = _cocApi.Logger?.Log<CocApi>(LoggingEvent.HttpResponseError, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} {serverResponse.HttpStatusCode} {e.Message}");
            }
            else
            {
                _ = _cocApi.Logger?.Log<CocApi>(LoggingEvent.HttpResponseError, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} {e.Message}");
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

            ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            stopwatch.Start();

            HttpResponseMessage response;

            try
            {
                 response = await ApiClient.GetAsync(encodedUrl, cts.Token).ConfigureAwait(false);
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

        private static async Task<TokenObject> GetTokenAsync(EndPoint endPoint, string url)
        {
            await SemaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                while (_tokenObjects.All(x => x.IsRateLimited))
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }

                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync(endPoint, url).ConfigureAwait(false);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private static void InitializeCacheExpiration(Downloadable result, HttpResponseMessage? response)
        {
            result.UpdatedAtUtc = DateTime.UtcNow;

            if (response != null)
            {
                if (response.Headers?.Date.HasValue == true)
                {
                    result.UpdatedAtUtc = response!.Headers!.Date!.Value.UtcDateTime;
                }

                if (response?.Headers?.Date.HasValue == true && response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge.HasValue)
                {
                    //adding 3 seconds incase the server clock is different than our clock
                    result.CacheExpiresAtUtc = response!.Headers!.Date!.Value.DateTime.Add(response.Headers.CacheControl.MaxAge.Value) + TimeSpan.FromSeconds(3);
                }
            }
        }

        private static void InitializeDownloadableProperties(Downloadable result, string encodedURL)
        {
            result.EncodedUrl = encodedURL;

            switch (result)
            {
                case LeagueWar leagueWarApiModel:
                    if (leagueWarApiModel.State == WarState.WarEnded)
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueWarTimeToLive);
                    }

                    break;

                case CurrentWar currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.ExpiresAtUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        currentWar.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.CurrentWarTimeToLive);
                    }

                    break;

                case LeagueGroup leagueGroupApiModel:
                    if (leagueGroupApiModel.State == LeagueState.WarsEnded)
                    {
                        leagueGroupApiModel.ExpiresAtUtc = DateTime.UtcNow.AddHours(6);
                    }
                    else
                    {
                        leagueGroupApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupTimeToLive);
                    }

                    break;

                case LeagueGroupNotFound leagueGroupNotFound:
                    leagueGroupNotFound.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupNotFoundTimeToLive);
                    break;

                case Clan clanApiModel:
                    clanApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.ClanTimeToLive);
                    break;

                case Village villageApiModel:
                    villageApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.VillageTimeToLive);
                    break;

                case NotInWar notInWar:
                    notInWar.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.CurrentWarTimeToLive);
                    break;

                default:
                    result.ExpiresAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        private static void InitializeResult<T>(T result, HttpResponseMessage? response, string encodedUrl) where T : Downloadable, new()
        {
            InitializeDownloadableProperties(result, encodedUrl);

            InitializeCacheExpiration(result, response);

            if (result is IInitialize initialize) initialize.Initialize();
        }

        private static Downloadable SuccessfulResponse<TValue>(HttpResponseMessage response, string encodedUrl) where TValue : Downloadable, new()
        {
            _ = _cocApi.Logger?.Log<CocApi>(LoggingEvent.HttpResponseStatusCodeSuccessful, encodedUrl.Replace("https://api.clashofclans.com/v1", ""));

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

        private static Downloadable UnSuccessfulResponse(HttpResponseMessage response, string encodedUrl, EndPoint endPoint, TokenObject token)
        {
            string responseText = response.Content.ReadAsStringAsync().Result;

            ResponseMessage ex = JsonConvert.DeserializeObject<ResponseMessage>(responseText);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new BadRequestException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                //if (endPoint == EndPoint.CurrentWar)
                //{
                //    var privateWar = new WarLogIsPrivate();

                //    InitializeResult(privateWar, response, encodedUrl);

                //    return privateWar;
                //}

                throw new ForbiddenException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && endPoint == EndPoint.LeagueGroup)
            {
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, response, encodedUrl);

                _ = _cocApi.Logger?.Log<CocApi>(LoggingEvent.HttpResponseStatusCodeUnsuccessful, $"{encodedUrl.Replace("https://api.clashofclans.com/v1", "")} league group not found");

                return leagueGroupNotFound;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new NotFoundException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                token.IsRateLimited = true;

                throw new TooManyRequestsException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new InternalServerErrorException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
            {
                _cocApi.IsAvailable = false;

                throw new BadGateWayException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _cocApi.IsAvailable = false;

                throw new ServiceUnavailableException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            {
                _cocApi.IsAvailable = false;

                throw new GatewayTimeoutException(ex, response.StatusCode);
            }

            throw new ServerResponseException(ex, response.StatusCode);
        }
    }
}