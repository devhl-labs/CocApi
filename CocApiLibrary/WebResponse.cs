using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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

namespace devhl.CocApi
{
    internal static class WebResponse
    {
        public static SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        public static HttpClient ApiClient { get; } = new HttpClient();

        private readonly static string _source = "WebResponse   | ";

        private static readonly IList<TokenObject> _tokenObjects = new List<TokenObject>();

        private static CocApi _cocApi = new CocApi();

        private static CocApiConfiguration _cfg = new CocApiConfiguration();

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<WebResponseTimer> WebResponseTimers { get; } = new List<WebResponseTimer>();

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
        }

        public static string GetTokenStatus()
        {
            return $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";
        }

        private static async Task<TokenObject> GetTokenAsync(EndPoint endPoint, string url)
        {
            await SemaphoreSlim.WaitAsync();

            try
            {
                while (_tokenObjects.All(x => x.IsRateLimited))
                {
                    await Task.Delay(50);
                }

                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).FirstOrDefault().GetTokenAsync(endPoint, url);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        internal static List<WebResponseTimer> GetTimers() => WebResponseTimers;

        internal static async Task<IDownloadable> GetWebResponse<T>(EndPoint endPoint, string encodedUrl, CancellationTokenSource? cancellationTokenSource = null) where T : class, IDownloadable, new()
        {
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                _cocApi.Logger?.LogDebug("{source} {encodedUrl}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""));

                TokenObject token = await GetTokenAsync(endPoint, encodedUrl);

                using CancellationTokenSource cts = new CancellationTokenSource(_cfg.TimeToWaitForWebRequests);

                cancellationTokenSource?.Token.Register(() => cts?.Cancel());

                ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                stopwatch.Start();

                using HttpResponseMessage response = await ApiClient.GetAsync(encodedUrl, cts.Token);

                stopwatch.Stop();

                string responseText = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    _cocApi.IsAvailable = true;

                    T result = JsonSerializer.Deserialize<T>(responseText, _jsonSerializerOptions);

                    if (result != null)
                    {
                        if (result is CurrentWarApiModel currentWar && currentWar.PreparationStartTimeUtc == DateTime.MinValue)
                        {
                            var notInWar = new NotInWar();

                            PrepareResult(notInWar, endPoint, stopwatch, response, encodedUrl);

                            return notInWar;
                        }
                        else
                        {
                            PrepareResult(result, endPoint, stopwatch, response, encodedUrl);

                            return result;
                        }
                    }
                    else
                    {
                        throw new CocApiException("The response could not be parsed.");
                    }
                }
                else
                {
                    ResponseMessageApiModel ex = JsonSerializer.Deserialize<ResponseMessageApiModel>(responseText, _jsonSerializerOptions);

                    WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

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

                        PrepareResult(leagueGroupNotFound, EndPoint.LeagueGroup, stopwatch, response, encodedUrl);

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
            catch (Exception e)
            {
                stopwatch.Stop();

                WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed));

                if (e is ServerResponseException serverResponse)
                {
                    _cocApi.Logger?.LogWarning("{source} {encodedUrl} {httpStatusCode} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), serverResponse.HttpStatusCode, e.Message);
                }
                else
                {
                    _cocApi.Logger?.LogWarning("{source} {encodedUrl} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), e.Message);
                }

                if (e is TaskCanceledException taskCanceled && endPoint == EndPoint.LeagueGroup)
                {
                    //there is a bug while the clan is searching where the api returns nothing
                    var leagueGroupNotFound = new LeagueGroupNotFound();

                    PrepareResult(leagueGroupNotFound, EndPoint.LeagueGroup, stopwatch, null, encodedUrl);

                    return leagueGroupNotFound;
                }

                if (e is TaskCanceledException taskCanceledException)
                {
                    ResponseMessageApiModel responseMessageApiModel = new ResponseMessageApiModel
                    {
                        Message = e.Message,

                        Reason = e.ToString()
                    };

                    throw new ServerTookTooLongToRespondException(responseMessageApiModel, null);
                }

                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        private static void PrepareResult<T>(T result, EndPoint endPoint, Stopwatch stopwatch, HttpResponseMessage? response, string encodedUrl) where T : class, IDownloadable, new()
        {
            if (response == null)
            {
                WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, null));
            }
            else
            {
                WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));
            }

            SetIDownloadableProperties(result, encodedUrl, response);

            SetCacheExpiration(result, response);

            if (result is IInitialize initialize) initialize.Initialize();
        }

        private static void SetCacheExpiration(IDownloadable result, HttpResponseMessage? response)
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

        private static void SetIDownloadableProperties(IDownloadable result, string encodedURL, HttpResponseMessage? response)
        {
            result.EncodedUrl = encodedURL;

            switch (result)
            {
                case LeagueWarApiModel leagueWarApiModel:
                    if (leagueWarApiModel.State == WarState.WarEnded)
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        leagueWarApiModel.ExpiresAtUtc = DateTime.UtcNow.Add(_cfg.LeagueWarApiModelTimeToLive);
                    }

                    break;

                case CurrentWarApiModel currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.ExpiresAtUtc = DateTime.UtcNow.AddYears(1);
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
                    //throw new CocApiException($"Unhandled Type");
                    break;
            }
        }
    }
}