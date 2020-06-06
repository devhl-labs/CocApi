using Dapper.SqlWriter;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace devhl.CocApi.Updaters
{
    internal sealed class Updater
    {
        public Updater(CocApi cocApi, SqlWriter sqlWriter)
        {
            CocApi = cocApi;

            SqlWriter = sqlWriter;
        }

        private CocApi CocApi { get; set; }

        private bool IsRunning { get; set; }

        private SqlWriter SqlWriter { get; set; }

        private bool StopRequested { get; set; }

        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        private ConcurrentDictionary<string, IVillage> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, IVillage>();

        private ConcurrentDictionary<string, CurrentWar> UpdatingWar { get; set; } = new ConcurrentDictionary<string, CurrentWar>();

        public async Task StartAsync()
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                StopRequested = false;

                CocApi.OnLog(new LogEventArgs(nameof(Updater), nameof(StartAsync), LogLevel.Information, $"Started with {CocApi.CocApiConfiguration.Tokens.Count} tokens and {CocApi.CocApiConfiguration.TokenTimeOut.TotalMilliseconds} ms token timeout."));

                int clanId = 0;

                int warId = 0;

                int villageId = 0;

                while (!StopRequested)
                {
                    List<Task> tasks = new List<Task>();

                    List<CachedClan> cachedClans = await SqlWriter.Select<CachedClan>()
                                                                  .OrderBy(c => c.Id)
                                                                  .Where(c => c.Id > clanId)
                                                                  .Limit(CocApi.CocApiConfiguration.ConcurrentUpdates)
                                                                  .QueryToListAsync()
                                                                  .ConfigureAwait(false);

                    for (int i = 0; i < cachedClans.Count; i++)
                        tasks.Add(UpdateClanAsync(cachedClans[i]));

                    List<CachedWar> cachedWars = await SqlWriter.Select<CachedWar>()
                                                                .Where(w => w.IsFinal == false &&
                                                                            w.Id > warId)
                                                                .OrderBy(w => w.Id)
                                                                .Limit(CocApi.CocApiConfiguration.ConcurrentUpdates)
                                                                .QueryToListAsync()
                                                                .ConfigureAwait(false);

                    for (int i = 0; i < cachedWars.Count; i++)
                        tasks.Add(UpdateWarAsync(cachedWars[i]));

                    List<CachedVillage> cachedVillages = await SqlWriter.Select<CachedVillage>()
                                                                        .Where(v => v.Id > villageId)
                                                                        .OrderBy(v => v.Id)
                                                                        .Limit(CocApi.CocApiConfiguration.ConcurrentUpdates)
                                                                        .QueryToListAsync()
                                                                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedVillages.Count; i++)
                        tasks.Add(UpdateVillage(cachedVillages[i]));

                    DateTime oldestWar = DateTime.UtcNow.AddDays(-7);

                    tasks.Add(SqlWriter.Update<CachedWar>()
                                       .Where(w => w.IsFinal == false &&
                                                   ((w.IsAvailableByClan == false && w.IsAvailableByOpponent == false) ||
                                                   w.PrepStartTime < oldestWar))
                                       .Set(w => w.IsFinal == true)
                                       .ExecuteAsync());

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    if (cachedClans.Count < CocApi.CocApiConfiguration.ConcurrentUpdates)
                        clanId = 0;
                    else
                        clanId = cachedClans.Max(c => c.Id);

                    if (cachedWars.Count < CocApi.CocApiConfiguration.ConcurrentUpdates)
                        warId = 0;
                    else
                        warId = cachedWars.Max(w => w.Id);

                    if (cachedVillages.Count < CocApi.CocApiConfiguration.ConcurrentUpdates)
                        villageId = 0;
                    else
                        villageId = cachedVillages.Max(v => v.Id);

                    await Task.Delay(CocApi.CocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                CocApi.OnLog(new ExceptionEventArgs(nameof(Updater), nameof(StartAsync), e));

                _ = StartAsync();
            }
        }

        public async Task StopAsync()
        {
            StopRequested = true;

            CocApi.OnLog(new LogEventArgs(nameof(Updater), nameof(StopAsync), LogLevel.Information));

            while (IsRunning)
                await Task.Delay(50).ConfigureAwait(false);
        }

        private async Task<Clan?> GetClanAsync(CachedClan cachedClan)
        {
            Clan? clan = await CocApi.Clans.GetOrFetchAsync(cachedClan.ClanTag).ConfigureAwait(false);

            if (clan == null || clan.IsLocallyExpired(CocApi.CocApiConfiguration.ClanTimeToLive) == false)
                return clan;

            Clan? fetched = await CocApi.Clans.FetchAsync(cachedClan.ClanTag).ConfigureAwait(false);

            if (fetched == null)
                return clan;

            clan.Update(CocApi, fetched);

            return fetched;
        }

        private async Task UpdateClanAsync(CachedClan cachedClan)
        {
            if (UpdatingClans.TryAdd(cachedClan.ClanTag, cachedClan) == false)
                return;

            try
            {
                List<Task> tasks = new List<Task>();

                Clan? clan = null;

                if (cachedClan.DownloadClan)
                    clan = await GetClanAsync(cachedClan).ConfigureAwait(false);

                if (clan == null)
                    return;

                if (CocApi.Clans.DownloadClanVillages && cachedClan.DownloadVillages)
                    tasks.Add(UpdateClanVillages(clan));

                if (CocApi.Wars.DownloadCurrentWars && cachedClan.DownloadCurrentWar)
                    tasks.Add(UpdateClanWar(clan));

                if (CocApi.Wars.DownloadCwl && cachedClan.DownloadCwl)
                    tasks.Add(UpdateCwl(clan));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.ClanTag, out CachedClan _);
            }
        }

        private async Task UpdateClanVillages(Clan clan)
        {
            List<Task> tasks = new List<Task>();

            foreach (ClanVillage clanVillage in clan.Villages.EmptyIfNull())
            {
                tasks.Add(UpdateVillage(clanVillage));
            }

            await Task.WhenAll().ConfigureAwait(false);
        }

        private async Task UpdateClanWar(Clan clan)
        {
            IWar? war = await CocApi.Wars.GetOrFetchAsync<CurrentWar>(clan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false);

            if (!(war is CurrentWar currentWar))
                return;

            if (UpdatingWar.TryAdd(currentWar.WarKey(), currentWar) == false)
                return;

            try
            {
                CachedWar? cachedWar = await CocApi.SqlWriter.Select<CachedWar>()
                                                            .Where(w => w.PrepStartTime == currentWar.PreparationStartTimeUtc &&
                                                                        w.ClanTag == currentWar.WarClans[0].ClanTag)
                                                            .QuerySingleOrDefaultAsync()
                                                            .ConfigureAwait(false);

                if (cachedWar != null)
                    return;

                await SqlWriter.Update<CachedWar>()
                                .Where(w => w.ClanTag == clan.ClanTag)
                                .Set(w => w.IsAvailableByClan == false)
                                .ExecuteAsync()
                                .ConfigureAwait(false);

                await SqlWriter.Update<CachedWar>()
                                .Where(w => w.OpponentTag == clan.ClanTag)
                                .Set(w => w.IsAvailableByOpponent == false)
                                .ExecuteAsync()
                                .ConfigureAwait(false);

                cachedWar = new CachedWar
                {
                    ClanTag = currentWar.WarClans[0].ClanTag,
                    EndTime = currentWar.EndTimeUtc,
                    IsFinal = false,
                    Json = currentWar.ToJson(),
                    OpponentTag = currentWar.WarClans[1].ClanTag,
                    PrepStartTime = currentWar.PreparationStartTimeUtc,
                    WarState = currentWar.State,
                    IsAvailableByClan = true,
                    IsAvailableByOpponent = true
                };

                await SqlWriter.Insert(cachedWar).ExecuteAsync().ConfigureAwait(false);

                CocApi.Wars.OnNewWar(currentWar);
            }
            finally
            {
                UpdatingWar.TryRemove(currentWar.WarKey(), out _);
            }
        }

        private async Task UpdateCurrentWar(CurrentWar storedWar, CachedWar cachedWar)
        {
            if (storedWar.EndTimeUtc < DateTime.UtcNow)
            {
                List<CachedClan> cachedClans = await SqlWriter.Select<CachedClan>()
                                                              .Where(c => c.ClanTag == storedWar.WarClans[0].ClanTag ||
                                                                          c.ClanTag == storedWar.WarClans[1].ClanTag)
                                                              .QueryToListAsync()
                                                              .ConfigureAwait(false);

                foreach (WarClan warClan in storedWar.WarClans)
                {
                    CachedClan? cachedClan = cachedClans.FirstOrDefault(c => c.ClanTag == warClan.ClanTag);

                    //ignore DownloadCurrentWar because the war is over and we need the ending
                    if (cachedClan == null || cachedClan.DownloadCurrentWar == false)
                    {
                        IWar? war = await CocApi.Wars.GetOrFetchAsync<CurrentWar>(warClan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false);

                        if (war is NotInWar || (war is CurrentWar currentWar && currentWar.WarKey() != storedWar.WarKey()))
                        {
                            if (warClan.ClanTag == storedWar.WarClans[0].ClanTag)
                                cachedWar.IsAvailableByClan = false;
                            else
                                cachedWar.IsAvailableByOpponent = false;
                        }
                    }
                }
            }

            List<string> paths = new List<string>();

            foreach (WarClan warClan in storedWar.WarClans)
                paths.Add(CurrentWar.Url(warClan.ClanTag));

            List<Cache> fetchedWarContainers = await SqlWriter.Select<Cache>()
                                              .Where(c => c.Path == paths[0] ||
                                                          c.Path == paths[1])
                                              .OrderByDesc(c => c.ServerExpiration)
                                              .QueryToListAsync()
                                              .ConfigureAwait(false);

            CurrentWar? fetchedWar = null;

            for (int i = 0; i < fetchedWarContainers.Count; i++)
            {
                fetchedWar = fetchedWarContainers[i].Json.Deserialize<IWar>() as CurrentWar;

                if (fetchedWar?.WarKey() == storedWar.WarKey())
                    break;

                fetchedWar = null;
            }

            WarLogEntry? warlogEntry = null;

            if (fetchedWar == null && DateTime.UtcNow > storedWar.EndTimeUtc)
            {
                foreach (WarClan warClan in storedWar.WarClans.EmptyIfNull())
                {
                    Paginated<WarLogEntry>? warLogs = await CocApi.Wars.GetOrFetchWarLogAsync(warClan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false);

                    warlogEntry = warLogs?.Items.FirstOrDefault(l => l.EndTimeUtc == storedWar.EndTimeUtc);

                    if (warlogEntry != null)
                        break;
                }

                storedWar.Update(CocApi, warlogEntry, cachedWar.Announcements);
            }
            else
            {
                storedWar.Update(CocApi, fetchedWar, cachedWar.Announcements);
            }

            if (fetchedWar == null)
            {
                if (cachedWar.Announcements != storedWar.Announcements)
                {
                    cachedWar.Announcements = storedWar.Announcements;

                    cachedWar.Json = storedWar.ToJson();

                    await SqlWriter.Update(cachedWar).ExecuteAsync().ConfigureAwait(false);
                }

                return;
            }

            if (cachedWar.Announcements != storedWar.Announcements || storedWar.ServerExpirationUtc != fetchedWar.ServerExpirationUtc)
            {
                cachedWar.Announcements = storedWar.Announcements;

                cachedWar.Json = fetchedWar.ToJson();

                cachedWar.EndTime = fetchedWar.EndTimeUtc;

                if (fetchedWar.State == WarState.WarEnded ||
                    warlogEntry != null ||
                    (cachedWar.IsAvailableByClan == false && cachedWar.IsAvailableByOpponent == false) ||
                    cachedWar.PrepStartTime < DateTime.UtcNow.AddDays(-7))

                    cachedWar.IsFinal = true;

                cachedWar.WarState = fetchedWar.State;

                await SqlWriter.Update(cachedWar).ExecuteAsync().ConfigureAwait(false);
            }
        }

        private async Task UpdateCwl(Clan clan)
        {
            if (!(await CocApi.Wars.GetOrFetchLeagueGroupAsync(clan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false) is LeagueGroup leagueGroup))
                return;

            foreach (Round round in leagueGroup.Rounds.EmptyIfNull())
            {
                foreach (string warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
                {
                    LeagueWar? leagueWar = await CocApi.Wars.GetAsync<LeagueWar>(warTag).ConfigureAwait(false) as LeagueWar;

                    if (leagueWar != null)
                        return;

                    leagueWar = await CocApi.Wars.GetAsync<LeagueWar>(warTag).ConfigureAwait(false) as LeagueWar;

                    if (leagueWar != null && leagueWar.Announcements.HasFlag(Announcements.WarAnnounced) == false)
                        continue;

                    leagueWar ??= await CocApi.Wars.FetchAsync<LeagueWar>(warTag).ConfigureAwait(false) as LeagueWar;

                    if (leagueWar == null || UpdatingWar.TryAdd(leagueWar.WarKey(), leagueWar) == false)
                        return;

                    try
                    {
                        CachedWar cachedWar = new CachedWar(leagueWar);

                        await SqlWriter.Insert(cachedWar).ExecuteAsync().ConfigureAwait(false);

                        CocApi.Wars.OnNewWar(leagueWar);

                        if (leagueWar.WarClans.Any(wc => wc.ClanTag == clan.ClanTag))
                            break;
                    }
                    catch (Exception)
                    {
                        UpdatingWar.TryRemove(leagueWar.WarKey(), out _);
                    }
                }
            }
        }

        private async Task UpdateLeagueWar(LeagueWar storedWar, CachedWar cachedWar)
        {
            if (cachedWar.WarTag == null || UpdatingWar.TryAdd(storedWar.WarKey(), storedWar) == false)
                return;

            try
            {
                if (!(await CocApi.Wars.GetOrFetchAsync<LeagueWar>(cachedWar.WarTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false) is LeagueWar fetchedWar))
                    return;

                storedWar.Update(CocApi, fetchedWar, cachedWar.Announcements);

                if (cachedWar.Announcements != storedWar.Announcements || storedWar.ServerExpirationUtc != fetchedWar.ServerExpirationUtc)
                {
                    cachedWar.Announcements = storedWar.Announcements;

                    cachedWar.EndTime = fetchedWar.EndTimeUtc;

                    if (fetchedWar.State == WarState.WarEnded)
                        cachedWar.IsFinal = true;

                    cachedWar.Json = fetchedWar.ToJson();

                    cachedWar.WarState = fetchedWar.State;

                    await SqlWriter.Update(cachedWar).ExecuteAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                UpdatingWar.TryRemove(storedWar.WarKey(), out _);
            }
        }

        private async Task UpdateVillage(ClanVillage clanVillage)
        {
            if (!UpdatingVillage.TryAdd(clanVillage.VillageTag, clanVillage))
                return;

            try
            {
                Village? stored = await CocApi.Villages.GetOrFetchAsync(clanVillage.VillageTag).ConfigureAwait(false);

                if (stored == null || stored.IsLocallyExpired(CocApi.CocApiConfiguration.VillageTimeToLive) == false)
                    return;

                Village? fetched = await CocApi.Villages.FetchAsync(clanVillage.VillageTag).ConfigureAwait(false);

                if (fetched != null)
                    stored.Update(CocApi, fetched);
            }
            finally
            {
                UpdatingVillage.TryRemove(clanVillage.VillageTag, out _);
            }
        }

        private async Task UpdateVillage(CachedVillage cachedVillage)
        {
            Village? storedVillage = await CocApi.Villages.GetOrFetchAsync(cachedVillage.VillageTag).ConfigureAwait(false);

            if (storedVillage == null || storedVillage.IsLocallyExpired(CocApi.CocApiConfiguration.VillageTimeToLive) == false)
                return;

            if (UpdatingVillage.TryAdd(cachedVillage.VillageTag, storedVillage) == false)
                return;

            try
            {
                Village? fetched = await CocApi.Villages.FetchAsync(cachedVillage.VillageTag).ConfigureAwait(false);

                if (fetched != null)
                    storedVillage.Update(CocApi, fetched);
            }
            finally
            {
                UpdatingVillage.TryRemove(cachedVillage.VillageTag, out _);
            }
        }

        private async Task UpdateWarAsync(CachedWar cachedWar)
        {
            if (!(cachedWar.Json.Deserialize<IWar>() is CurrentWar storedWar))
                return;

            if (storedWar is LeagueWar leagueWar)
                await UpdateLeagueWar(leagueWar, cachedWar).ConfigureAwait(false);
            else
                await UpdateCurrentWar(storedWar, cachedWar).ConfigureAwait(false);
        }
    }
}