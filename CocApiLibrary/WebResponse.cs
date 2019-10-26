using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
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
using static CocApiLibrary.Enums;

namespace CocApiLibrary
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

        internal static async Task<T> GetWebResponse<T>(EndPoint endPoint, string encodedUrl, CancellationTokenSource? cancellationTokenSource = null) where T : class, IDownloadable, new()
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
                        WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

                        if (result is IInitialize process)
                        {
                            process.Initialize();
                        }

                        SetIDownloadableProperties(result, encodedUrl);

                        SetRelationalProperties(result);

                        return result;
                    }
                    else
                    {
                        throw new CocApiException("The response could not be parsed.");
                    }
                }
                else
                {
                    ResponseMessageAPIModel ex = JsonSerializer.Deserialize<ResponseMessageAPIModel>(responseText, _jsonSerializerOptions);

                    WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) //400
                    {
                        throw new BadRequestException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) //403
                    {
                        throw new ForbiddenException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) //404
                    {
                        throw new NotFoundException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) //429
                    {
                        token.IsRateLimited = true;

                        throw new TooManyRequestsException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500
                    {
                        throw new InternalServerErrorException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway) //502
                    {
                        _cocApi.IsAvailable = false;

                        throw new BadGateWayException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) //503
                    {
                        _cocApi.IsAvailable = false;

                        throw new ServiceUnavailableException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)  //504
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

                _cocApi.Logger?.LogWarning("{source} {encodedUrl} {message}", _source, encodedUrl.Replace("https://api.clashofclans.com/v1", ""), e.Message);

                if (e is TaskCanceledException taskCanceledException)
                {
                    throw new ServerTookTooLongToRespondException(e.Message, e);
                }

                throw _cocApi.GetException(e);
            }
        }

        private static void SetRelationalProperties<T>(T result) where T : class, IDownloadable, new()
        {
            if (result is ClanAPIModel clan)
            {
                //if (clan.BadgeUrls != null) clan.BadgeUrls.ClanTag = clan.ClanTag;

                if (clan.Villages != null)
                {
                    foreach (var clanVillage in clan.Villages)
                    {
                        clanVillage.ClanTag = clan.ClanTag;
                    }
                }

                if (clan.BadgeUrls != null)
                {
                    clan.BadgeUrlsId = clan.BadgeUrls.Id;
                }

                if (clan.Location != null)
                {
                    clan.LocationId = clan.Location.Id;
                }
            }

            if (result is VillageAPIModel village)
            {
                if (village.LegendStatistics != null)
                {
                    village.LegendStatistics.VillageTag = village.VillageTag;

                    if (village.LegendStatistics.BestSeason != null) village.LegendStatistics.BestSeason.VillageTag = village.VillageTag;

                    if (village.LegendStatistics.CurrentSeason != null) village.LegendStatistics.CurrentSeason.VillageTag = village.VillageTag;

                    if (village.LegendStatistics.PreviousVersusSeason != null) village.LegendStatistics.PreviousVersusSeason.VillageTag = village.VillageTag;

                    if (village.LegendStatistics.PreviousVersusSeason != null) village.LegendStatistics.PreviousVersusSeason.VillageTag = village.VillageTag;
                }

                foreach (var spell in village.Spells.EmptyIfNull())
                {
                    spell.VillageTag = village.VillageTag;
                }

                foreach (var troop in village.Troops.EmptyIfNull())
                {
                    troop.VillageTag = village.VillageTag;
                }

                //if (village.Clan != null)
                //{
                //    if (village.Clan.BadgeUrls != null)
                //    {
                //        village.Clan.BadgeUrls.ClanTag = village.Clan.ClanTag;
                //    }
                //}
            }

            if (result is LeagueGroupAPIModel group)
            {
                group.GroupId = $"{group.Season.ToString()}{group.Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

                foreach (var leagueClan in group.Clans.EmptyIfNull())
                {
                    leagueClan.GroupId = group.GroupId;

                    foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())
                    {
                        leagueVillage.ClanTag = leagueClan.ClanTag;
                    }

                    //if (leagueClan.BadgeUrls != null) leagueClan.BadgeUrls.ClanTag = leagueClan.ClanTag;
                }

                foreach(var round in group.Rounds.EmptyIfNull())
                {
                    round.RoundId = $"{group.Season.ToShortDateString()};{group.Clans.OrderBy(c => c.ClanTag).First().ClanTag};{group.Rounds!.IndexOf(round)}";
                }
            }

            if (result is CurrentWarAPIModel war)
            {
                foreach (var attack in war.Attacks.EmptyIfNull())
                {
                    attack.WarId = war.WarId;

                    //attack.AttackId = $"{attack.WarID};{attack.Order}";
                    //attack.AttackId = attack.Order.ToString();
                }

                foreach (var warClan in war.Clans)
                {
                    warClan.WarId = war.WarId;

                    foreach(var warVillage in warClan.Villages.EmptyIfNull())
                    {
                        warVillage.WarClanId = warClan.WarClanId;
                    }

                    //if (warClan.BadgeUrls != null)
                    //{
                    //    warClan.BadgeUrls.ClanTag = warClan.ClanTag;
                    //}
                }
            }
        }

        private static void SetIDownloadableProperties<T>(T result, string encodedURL) where T : class, IDownloadable, new()
        {
            switch (result)
            {
                case LeagueWarAPIModel leagueWarAPIModel:
                    if (leagueWarAPIModel.State == WarState.WarEnded)
                    {
                        leagueWarAPIModel.Expires = DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        leagueWarAPIModel.Expires = DateTime.UtcNow.Add(_cfg.LeagueWarAPIModelTimeToLive);
                    }

                    leagueWarAPIModel.EncodedUrl = encodedURL;
                    break;

                case CurrentWarAPIModel currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.Expires = DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        currentWar.Expires = DateTime.UtcNow.Add(_cfg.CurrentWarAPIModelTimeToLive);
                    }

                    currentWar.EncodedUrl = encodedURL;
                    break;

                case LeagueGroupAPIModel leagueGroupAPIModel:
                    if (leagueGroupAPIModel.State == LeagueState.WarsEnded)
                    {
                        leagueGroupAPIModel.Expires = DateTime.UtcNow.AddHours(6);
                    }
                    else
                    {
                        leagueGroupAPIModel.Expires = DateTime.UtcNow.Add(_cfg.LeagueGroupAPIModelTimeToLive);
                    }

                    leagueGroupAPIModel.EncodedUrl = encodedURL;
                    break;

                case ClanAPIModel clanAPIModel:
                    clanAPIModel.Expires = DateTime.UtcNow.Add(_cfg.ClanAPIModelTimeToLive);
                    clanAPIModel.EncodedUrl = encodedURL;
                    break;

                case VillageAPIModel villageAPIModel:
                    villageAPIModel.Expires = DateTime.UtcNow.Add(_cfg.VillageAPIModelTimeToLive);
                    villageAPIModel.EncodedUrl = encodedURL;
                    break;

                case WarLogAPIModel warLogAPIModel:
                    warLogAPIModel.Expires = DateTime.UtcNow.Add(_cfg.WarLogAPIModelTimeToLive);
                    warLogAPIModel.EncodedUrl = encodedURL;
                    break;

                default:
                    throw new CocApiException($"Unhandled Type");
            }
        }
    }
}