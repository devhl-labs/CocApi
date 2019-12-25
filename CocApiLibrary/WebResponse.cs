using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.Location;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    internal static class WebResponse
    {
        public static SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);

        public static HttpClient ApiClient { get; } = new HttpClient();

        private readonly static string _source = "WebResponse   | ";

        private static readonly List<TokenObject> _tokenObjects = new List<TokenObject>();

        private static CocApi _cocApi = new CocApi();

        private static CocApiConfiguration _cfg = new CocApiConfiguration();

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static ConcurrentBag<WebResponseTimer> WebResponseTimers { get; } = new ConcurrentBag<WebResponseTimer>();

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

            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            //JsonSerializerSettings a = new JsonSerializerSettings
            //{
            //    Converters = new JsonConverterAttribute()
            //}

            //JsonConverterCollection converters = new JsonConverterCollection();
            //converters.Add()
        }

        public static string GetTokenStatus() => $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";

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

        public static ConcurrentBag<WebResponseTimer> GetTimers() => WebResponseTimers;

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

        public static async Task<IDownloadable> GetIDownloadableAsync<TValue>(EndPoint endPoint, string encodedUrl, CancellationToken cancellationToken) where TValue : class, IDownloadable, new()
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

        private static IDownloadable ErrorInResponse(Exception e, string encodedUrl, EndPoint endPoint)
        {
            if (e is ServerResponseException serverResponse)
            {
                _cocApi.Logger?.LogWarning("{source} {encodedUrl} {httpStatusCode} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), serverResponse.HttpStatusCode, e.Message);
            }
            else
            {
                _cocApi.Logger?.LogWarning("{source} {encodedUrl} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), e.Message);
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
                ResponseMessageApiModel responseMessageApiModel = new ResponseMessageApiModel
                {
                    Message = e.Message,

                    Reason = e.ToString()
                };

               throw new ServerTookTooLongToRespondException(responseMessageApiModel, null);
            }

            if (e is CocApiException) throw e;

            throw new CocApiException(e.Message, e);
        }

        private static IDownloadable UnSuccessfulResponse(HttpResponseMessage response, string encodedUrl, EndPoint endPoint, TokenObject token)
        {
            _cocApi.Logger?.LogDebug("{source} {encodedUrl} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), "unsuccessful http response");

            string responseText = response.Content.ReadAsStringAsync().Result;

            ResponseMessageApiModel ex = JsonSerializer.Deserialize<ResponseMessageApiModel>(responseText, _jsonSerializerOptions);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new BadRequestException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException(ex, response.StatusCode);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && endPoint == EndPoint.LeagueGroup)
            {
                var leagueGroupNotFound = new LeagueGroupNotFound();

                InitializeResult(leagueGroupNotFound, response, encodedUrl);

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

        private static IDownloadable SuccessfulResponse<TValue>(HttpResponseMessage response, string encodedUrl) where TValue : class, IDownloadable, new()
        {
            _cocApi.Logger?.LogDebug("{source} {encodedUrl}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""));

            _cocApi.IsAvailable = true;

            string responseText = response.Content.ReadAsStringAsync().Result;

            TValue result = JsonSerializer.Deserialize<TValue>(responseText, _jsonSerializerOptions);

            if (result != null)
            {
                if (result is CurrentWarApiModel currentWar && currentWar.PreparationStartTimeUtc == DateTime.MinValue)
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

        private static void InitializeResult<T>(T result, HttpResponseMessage? response, string encodedUrl) where T : class, IDownloadable, new()
        {
            InitializeIDownloadableProperties(result, encodedUrl);

            InitializeCacheExpiration(result, response);

            if (result is IInitialize initialize) initialize.Initialize();
        }

        private static void InitializeCacheExpiration(IDownloadable result, HttpResponseMessage? response)
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

        private static void InitializeIDownloadableProperties(IDownloadable result, string encodedURL)
        {
            result.EncodedUrl = encodedURL;

            switch (result)
            {
                case LeagueWarApiModel leagueWarApiModel:
                    if (leagueWarApiModel.State == WarState.WarEnded)
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueWarApiModelTimeToLive);
                    }

                    break;

                case CurrentWarApiModel currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.ExpiresAtUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        currentWar.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.CurrentWarApiModelTimeToLive);
                    }

                    break;

                case LeagueGroupApiModel leagueGroupApiModel:
                    if (leagueGroupApiModel.State == LeagueState.WarsEnded)
                    {
                        leagueGroupApiModel.ExpiresAtUtc = DateTime.UtcNow.AddHours(6);
                    }
                    else
                    {
                        leagueGroupApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupApiModelTimeToLive);
                    }

                    break;

                case LeagueGroupNotFound leagueGroupNotFound:
                    leagueGroupNotFound.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueGroupNotFoundTimeToLive);
                    break;


                case ClanApiModel clanApiModel:
                    clanApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.ClanApiModelTimeToLive);
                    break;


                case VillageApiModel villageApiModel:
                    villageApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.VillageApiModelTimeToLive);
                    break;


                case PaginatedApiModel<WarLogEntryModel> warLogApiModel:
                    warLogApiModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;


                case PaginatedApiModel<VillageLeagueApiModel> villageLeagueSearchModel:
                    villageLeagueSearchModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;


                case PaginatedApiModel<LocationApiModel> searchApiModel:
                    searchApiModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;


                case PaginatedApiModel<ClanApiModel> clanSearchModel:
                    clanSearchModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;

                case PaginatedApiModel<VillageApiModel> villageSearchModel:
                    villageSearchModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;

                case PaginatedApiModel<LabelApiModel> villageSearchModel:
                    villageSearchModel.ExpiresAtUtc = DateTime.UtcNow;
                    break;

                case NotInWar notInWar:
                    notInWar.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.CurrentWarApiModelTimeToLive);
                    break;

                default:
                    result.ExpiresAtUtc = DateTime.UtcNow;

                    _cocApi.Logger.LogWarning(LoggingEvents.UnhandledCase, "Unhandled case");

                    break;
            }
        }
    }
}