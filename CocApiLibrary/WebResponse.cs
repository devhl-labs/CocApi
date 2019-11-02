﻿using CocApiLibrary.Exceptions;
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

                DateTime? cacheExpires = GetCacheExpirationDate(response);

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



                        //WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

                        //if (result is IInitialize process)
                        //{
                        //    process.Initialize();
                        //}

                        //SetIDownloadableProperties(result, encodedUrl);

                        //SetRelationalProperties(result);

                        //if (cacheExpires.HasValue)
                        //{
                        //    result.CacheExpiresAtUtc = cacheExpires;
                        //}

                        //return result;
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

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) //400
                    {
                        throw new BadRequestException(ex, response.StatusCode);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) //403
                    {
                        throw new ForbiddenException(ex, response.StatusCode);
                    }

                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && endPoint == EndPoint.LeagueGroup)
                    {
                        var leagueGroupNotFound = new LeagueGroupNotFound
                        {
                            CacheExpiresAtUtc = GetCacheExpirationDate(response)
                        };

                        SetIDownloadableProperties(leagueGroupNotFound, encodedUrl);

                        return leagueGroupNotFound;
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

                if (e is TaskCanceledException taskCanceled && endPoint == EndPoint.LeagueGroup)
                {
                    //there is a bug while the clan is searching where the api returns nothing
                    var leagueGroupNotFound = new LeagueGroupNotFound();

                    SetIDownloadableProperties(leagueGroupNotFound, encodedUrl);

                    return leagueGroupNotFound;
                }

                if (e is TaskCanceledException taskCanceledException)
                {
                    throw new ServerTookTooLongToRespondException(e.Message, e);
                }

                throw _cocApi.GetException(e);
            }
        }

        private static void PrepareResult<T>(T result, EndPoint endPoint, Stopwatch stopwatch, HttpResponseMessage response, string encodedUrl) where T : class, IDownloadable, new()
        {
            WebResponseTimers.Add(new WebResponseTimer(endPoint, stopwatch.Elapsed, response.StatusCode));

            if (result is IInitialize process)
            {
                process.Initialize();
            }

            SetIDownloadableProperties(result, encodedUrl);

            SetRelationalProperties(result);

            DateTime? cacheExpires = GetCacheExpirationDate(response);

            if (cacheExpires.HasValue)
            {
                result.CacheExpiresAtUtc = cacheExpires;
            }
        }

        private static DateTime? GetCacheExpirationDate(HttpResponseMessage response)
        {
            DateTime? cacheExpires = null;

            if (response.Headers.Date.HasValue && response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge.HasValue)
            {
                cacheExpires = response.Headers.Date.Value.DateTime.Add(response.Headers.CacheControl.MaxAge.Value);
            }

            return cacheExpires;
        }

        private static void SetRelationalProperties<T>(T result) where T : class, IDownloadable, new()
        {
            if (result is ClanApiModel clan)
            {
                SetRelationalProperties(clan);
            }

            if (result is PaginatedApiModel<ClanApiModel> clanSearch)
            {
                foreach(var clanItem in clanSearch.Items.EmptyIfNull())
                {
                    SetRelationalProperties(clanItem);
                }
            }

            if (result is VillageApiModel village)
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

                foreach(var hero in village.Heroes.EmptyIfNull())
                {
                    village.AllTroops.Add(hero);

                    hero.VillageTag = village.VillageTag;

                    hero.IsHero = true;
                }

                foreach(var troop in village.Troops.EmptyIfNull())
                {
                    village.AllTroops.Add(troop);

                    troop.VillageTag = village.VillageTag;

                    troop.IsHero = false;
                }

                foreach(var achievement in village.Achievements.EmptyIfNull())
                {
                    achievement.VillageTag = village.VillageTag;
                }

                foreach(var label in village.Labels.EmptyIfNull())
                {
                    label.VillageTag = village.VillageTag;
                }
            }

            if (result is LeagueGroupApiModel group)
            {
                group.GroupId = $"{group.Season.ToString()}{group.Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

                foreach (var leagueClan in group.Clans.EmptyIfNull())
                {
                    leagueClan.GroupId = group.GroupId;

                    foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())
                    {
                        leagueVillage.ClanTag = leagueClan.ClanTag;
                    }
                }

                foreach(var round in group.Rounds.EmptyIfNull())
                {
                    round.RoundId = $"{group.Season.ToShortDateString()};{group.Clans.OrderBy(c => c.ClanTag).First().ClanTag};{group.Rounds!.IndexOf(round)}";
                }
            }

            if (result is CurrentWarApiModel war)
            {
                foreach (var attack in war.Attacks.EmptyIfNull())
                {
                    attack.WarId = war.WarId;
                }

                foreach (var warClan in war.Clans)
                {
                    warClan.WarId = war.WarId;

                    foreach(var warVillage in warClan.Villages.EmptyIfNull())
                    {
                        warVillage.WarClanId = warClan.WarClanId;

                        warVillage.ClanTag = warClan.ClanTag;

                        warVillage.WarId = war.WarId;
                    }
                }

                if (war.PreparationStartTimeUtc != DateTime.MinValue)
                {
                    war.Flags.WarIsAccessible = true;
                }

                if (war.State > WarState.Preparation)
                {
                    war.Flags.WarAnnounced = true;

                    war.Flags.WarStarted = true;

                    war.Flags.WarStartingSoon = true;
                }

                if (war.State > WarState.InWar)
                {
                    war.Flags.AttacksMissed = true;

                    war.Flags.AttacksNotSeen = true;

                    war.Flags.WarEnded = true;

                    war.Flags.WarEndingSoon = true;

                    war.Flags.WarEndNotSeen = true;

                    war.Flags.WarEndSeen = true;
                }

                if (war.State == WarState.Preparation && war.WarStartingSoonUtc < DateTime.UtcNow)
                {
                    war.Flags.WarStartingSoon = true;
                }

                if (war.State == WarState.InWar && war.WarEndingSoonUtc < DateTime.UtcNow)
                {
                    war.Flags.WarEndingSoon = true;
                }
            }
        }

        private static void SetRelationalProperties(ClanApiModel clan)
        {
            foreach (var clanVillage in clan.Villages.EmptyIfNull())
            {
                clanVillage.ClanTag = clan.ClanTag;

                // make all occurances of the same league be the same instance for the benefit of ef
                if (clanVillage.League != null)
                {
                    clanVillage.League = clan.Villages.First(v => v.LeagueId == clanVillage.League.Id).League;
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

        private static void SetIDownloadableProperties<T>(T result, string encodedURL) where T : class, IDownloadable, new()
        {
            switch (result)
            {
                case LeagueWarApiModel leagueWarApiModel:
                    if (leagueWarApiModel.State == WarState.WarEnded)
                    {
                        leagueWarApiModel.Expires = DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        leagueWarApiModel.Expires = DateTime.UtcNow.Add(_cfg.LeagueWarApiModelTimeToLive);
                    }

                    leagueWarApiModel.EncodedUrl = encodedURL;
                    break;

                case CurrentWarApiModel currentWar:
                    if (currentWar.State == WarState.WarEnded)
                    {
                        currentWar.Expires = DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        currentWar.Expires = DateTime.UtcNow.Add(_cfg.CurrentWarApiModelTimeToLive);
                    }

                    currentWar.EncodedUrl = encodedURL;
                    break;


                case LeagueGroupApiModel leagueGroupApiModel:
                    if (leagueGroupApiModel.State == LeagueState.WarsEnded)
                    {
                        leagueGroupApiModel.Expires = DateTime.UtcNow.AddHours(6);
                    }
                    else
                    {
                        leagueGroupApiModel.Expires = DateTime.UtcNow.Add(_cfg.LeagueGroupApiModelTimeToLive);
                    }

                    leagueGroupApiModel.EncodedUrl = encodedURL;
                    break;


                case LeagueGroupNotFound leagueGroupNotFound:
                    leagueGroupNotFound.Expires = DateTime.UtcNow.Add(_cfg.LeagueGroupNotFoundTimeToLive);
                    leagueGroupNotFound.EncodedUrl = encodedURL;
                    break;


                case ClanApiModel clanApiModel:
                    clanApiModel.Expires = DateTime.UtcNow.Add(_cfg.ClanApiModelTimeToLive);
                    clanApiModel.EncodedUrl = encodedURL;
                    break;


                case VillageApiModel villageApiModel:
                    villageApiModel.Expires = DateTime.UtcNow.Add(_cfg.VillageApiModelTimeToLive);
                    villageApiModel.EncodedUrl = encodedURL;
                    break;


                case PaginatedApiModel<WarLogEntryModel> warLogApiModel:
                    warLogApiModel.Expires = DateTime.UtcNow.Add(_cfg.WarLogApiModelTimeToLive);
                    warLogApiModel.EncodedUrl = encodedURL;
                    break;


                case PaginatedApiModel<VillageLeagueApiModel> villageLeagueSearchModel:
                    villageLeagueSearchModel.Expires = DateTime.UtcNow.Add(_cfg.VillageLeagueSearchApiModelTimeToLive);
                    villageLeagueSearchModel.EncodedUrl = encodedURL;
                    break;


                case PaginatedApiModel<LocationApiModel> searchApiModel:
                    searchApiModel.Expires = DateTime.UtcNow.Add(_cfg.LocationSearchApiModelTimeToLive);
                    searchApiModel.EncodedUrl = encodedURL;
                    break;


                case PaginatedApiModel<ClanApiModel> clanSearchModel:
                    clanSearchModel.Expires = DateTime.UtcNow.Add(_cfg.ClanSearchApiModelTimeToLive);
                    clanSearchModel.EncodedUrl = encodedURL;
                    break;

                case NotInWar notInWar:
                    notInWar.Expires = DateTime.UtcNow.Add(_cfg.CurrentWarApiModelTimeToLive);
                    notInWar.EncodedUrl = encodedURL;
                    break;




                default:
                    throw new CocApiException($"Unhandled Type");
            }
        }
    }
}