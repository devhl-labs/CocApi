using Dapper.SqlWriter;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

namespace devhl.CocApi.Updaters
{
    public class NewUpdater
    {
        public NewUpdater(CocApi cocApi, SqlWriter sqlWriter)
        {
            CocApi = cocApi;

            SqlWriter = sqlWriter;
        }

        public bool DownloadClanVillages { get; set; } = true;
        public bool DownloadWars { get; set; } = true;
        private CocApi CocApi { get; set; }

        private bool IsRunning { get; set; }
        private SqlWriter SqlWriter { get; set; }

        private bool StopRequested { get; set; }
        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        private ConcurrentDictionary<string, CachedVillage> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedVillage>();

        private ConcurrentDictionary<string, CurrentWar> UpdatingClanWar { get; set; } = new ConcurrentDictionary<string, CurrentWar>();

        private ConcurrentDictionary<string, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<string, CachedWar>();

        public async Task AddClanAsync(string clanTag, bool downloadVillages, bool downloadWars, bool downloadCwl, bool download = true)
        {
            CachedClan? cachedClan = await SqlWriter.Select<CachedClan>()
                                                   .Where(c => c.ClanTag == clanTag)
                                                   .QueryFirstOrDefaultAsync();

            if (cachedClan == null)
            {
                cachedClan = new CachedClan
                {
                    ClanTag = clanTag,
                    DownloadClan = download,
                    DownloadCwl = downloadCwl,
                    DownloadVillages = downloadVillages,
                    DownloadCurrentWar = downloadWars
                };

                await SqlWriter.Insert(cachedClan).ExecuteAsync();
            }
        }

        public async Task AddOrUpdateClanAsync(string clanTag, bool downloadVillages, bool downloadWars, bool downloadCwl, bool download = true)
        {
            CachedClan? cachedClan = await SqlWriter.Select<CachedClan>()
                                                   .Where(c => c.ClanTag == clanTag)
                                                   .QueryFirstOrDefaultAsync();
            if (cachedClan == null)
            {
                await AddClanAsync(clanTag, downloadVillages, downloadWars, downloadCwl, download);
            }
            else
            {
                cachedClan.DownloadVillages = downloadVillages;

                cachedClan.DownloadCurrentWar = downloadWars;

                cachedClan.DownloadCwl = downloadCwl;

                cachedClan.DownloadClan = download;

                await SqlWriter.Update(cachedClan).ExecuteAsync();
            }
        }

        public async Task AddVillage(string villageTag)
        {
            CachedVillage cachedVillage = new CachedVillage
            {
                VillageTag = villageTag
            };

            await SqlWriter.Insert(cachedVillage).ExecuteAsync();
        }

        public async Task UpdateAsync()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            StopRequested = false;

            while (!StopRequested)
            {
                List<Task> tasks = new List<Task>();

                List<CachedClan> cachedClans = await SqlWriter.Select<CachedClan>()
                                                                     .Where(c => (c.DownloadClan == true && c.ClanUpdatesAt < DateTime.UtcNow) ||
                                                                                 c.DownloadCurrentWar == true /*&& c.WarUpdatesAt < DateTime.UtcNow*/)
                                                                     .OrderBy(c => c.ClanUpdatesAt)
                                                                     .Limit(CocApi.CocApiConfiguration.NumberOfUpdaters)
                                                                     .QueryToListAsync();

                for (int i = 0; i < cachedClans.Count; i++)
                    tasks.Add(UpdateClan(cachedClans[i]));

                List<CachedWar> cachedWars = await SqlWriter.Select<CachedWar>()
                                                                   .Where(w => w.WarState < WarState.WarEnded &&
                                                                               w.WarTag == null)
                                                                   .OrderBy(w => w.ProcessedAt)
                                                                   .Limit(CocApi.CocApiConfiguration.NumberOfUpdaters)
                                                                   .QueryToListAsync();

                for (int i = 0; i < cachedWars.Count; i++)
                    tasks.Add(ProcessCurrentWar(cachedWars[i]));

                List<CachedVillage> cachedVillages = await SqlWriter.Select<CachedVillage>()
                                                                    .Where(v => v.Download == true &&
                                                                                v.UpdatesAt < DateTime.UtcNow)
                                                                    .Limit(CocApi.CocApiConfiguration.NumberOfUpdaters)
                                                                    .QueryToListAsync();


                for (int i = 0; i < cachedVillages.Count; i++)
                    tasks.Add(UpdateVillage(cachedVillages[i]));

                await Task.WhenAll(tasks);
            }

            IsRunning = false;
        }

        private async Task ProcessCurrentWar(CachedWar cachedWar)
        {
            if (UpdatingWar.TryAdd(cachedWar.WarKey(), cachedWar) == false)
                return;

            try
            {
                List<CachedClanWar> cachedClanWars = await SqlWriter.Select<CachedClanWar>()
                                                                    .Where(w => w.PrepStartTime == cachedWar.PrepStartTime &&
                                                                                (w.ClanTag == cachedWar.ClanTag || 
                                                                                 w.ClanTag == cachedWar.OpponentTag))
                                                                    .OrderBy(w => w.UpdatesAt)
                                                                    .QueryToListAsync();

                if (DateTime.UtcNow > cachedWar.EndTime && 
                    cachedClanWars.Count == 1 && 
                    cachedClanWars.Any(w => w.WarState == WarState.WarEnded) == false)
                {
                    string missingTag = cachedWar.ClanTags.First(t => t != cachedClanWars[0].ClanTag);

                    CachedClan enemy = await SqlWriter.Select<CachedClan>()
                                                      .Where(c => c.ClanTag == missingTag)
                                                      .QueryFirstAsync();

                    //ignore the DownloadCurrentWar false since this war is over and we need the ending
                    if (enemy.DownloadCurrentWar == false && DateTime.UtcNow > enemy.WarUpdatesAt)
                    {                        
                        IWar? war = await CocApi.Wars.FetchAsync<CurrentWar>(missingTag);

                        if (war != null)
                        {
                            enemy.WarUpdatesAt = war.ServerExpirationUtc;

                            await SqlWriter.Update(enemy).ExecuteAsync();
                        }

                        if (war is CurrentWar enemyCurrentWar)
                        {
                            CachedClanWar cachedClanWar = new CachedClanWar(missingTag, enemyCurrentWar);

                            await SqlWriter.Insert(cachedClanWar).ExecuteAsync();

                            cachedClanWars.Add(cachedClanWar);
                        }
                    }                        
                }

                CachedClanWar fetchedClanWar = cachedClanWars.OrderByDescending(w => w.UpdatesAt).First();

                cachedWar.ToCurrentWar().Update(CocApi, fetchedClanWar.ToCurrentWar());

                if (cachedClanWars.All(cw => cw.IsDownloadable == false) || 
                    fetchedClanWar.WarState == WarState.WarEnded ||
                    cachedWar.PrepStartTime < DateTime.UtcNow.AddDays(-10))
                    cachedWar.IsFinal = true;

                cachedWar.ProcessedAt = DateTime.UtcNow;

                cachedWar.WarState = fetchedClanWar.WarState;

                cachedWar.Json = fetchedClanWar.Json;

                await SqlWriter.Update(cachedWar).ExecuteAsync();
            }
            finally
            {
                UpdatingWar.TryRemove(cachedWar.WarKey(), out _);
            }
        }

        private async Task UpdateClan(CachedClan cachedClan, Clan? storedClan)
        {
            if (cachedClan.DownloadClan == false || DateTime.UtcNow < cachedClan.ClanUpdatesAt)
                return;

            Clan? fetchedClan = await CocApi.Clans.FetchAsync(cachedClan.ClanTag);

            if (storedClan != null && fetchedClan != null)
                storedClan.Update(CocApi, fetchedClan);

            if (fetchedClan != null)
            {
                cachedClan.ClanJson = JsonConvert.SerializeObject(fetchedClan);

                cachedClan.IsWarLogPublic = fetchedClan.IsWarLogPublic;

                cachedClan.ClanUpdatesAt = fetchedClan.EffectiveExpiration();
            }
        }

        private async Task UpdateClan(CachedClan cachedClan)
        {
            if (StopRequested)
                return;

            if (UpdatingClans.TryAdd(cachedClan.ClanTag, cachedClan) == false)
                return;
                
            try
            {
                Clan? clan = null;

                if (cachedClan.ClanJson != null)
                    clan = JsonConvert.DeserializeObject<Clan>(cachedClan.ClanJson);

                await UpdateClan(cachedClan, clan);

                await UpdateClanWar(cachedClan);

                await SqlWriter.Update(cachedClan).ExecuteAsync();

                await UpdateClanVillages(cachedClan, clan);

                await Task.Delay(50);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.ClanTag, out CachedClan _);
            }
        }

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            if (DownloadWars && 
                cachedClan.DownloadCurrentWar && 
                cachedClan.IsWarLogPublic && 
                DateTime.UtcNow > cachedClan.WarUpdatesAt)
            {
                IWar? war = await CocApi.Wars.FetchAsync<CurrentWar>(cachedClan.ClanTag);

                if (war == null)
                    return;

                if (war is NotInWar)
                    await SqlWriter.Update<CachedClanWar>()
                                   .Where(cw => cw.ClanTag == cachedClan.ClanTag)
                                   .Set(cw => cw.IsDownloadable == false)
                                   .ExecuteAsync();


                cachedClan.WarUpdatesAt = war.EffectiveExpiration();

                if (!(war is CurrentWar currentWar))
                    return;

                if (!UpdatingClanWar.TryAdd(currentWar.WarKey(), currentWar))
                    return;

                try
                {
                    await AddClanAsync(currentWar.WarClans.First(wc => wc.ClanTag != cachedClan.ClanTag).ClanTag, false, false, false, false);

                    CachedClanWar? cachedClanWar = await SqlWriter.Select<CachedClanWar>()
                                                                 .Where(cw => cw.ClanTag == cachedClan.ClanTag &&
                                                                              cw.PrepStartTime == currentWar.PreparationStartTimeUtc)
                                                                 .QueryFirstOrDefaultAsync();

                    if (cachedClanWar == null)
                    {
                        await SqlWriter.Update<CachedClanWar>()
                                       .Where(cw => cw.PrepStartTime != currentWar.PreparationStartTimeUtc &&
                                                    cw.ClanTag == cachedClan.ClanTag &&
                                                    cw.IsDownloadable == true)
                                       .Set(cw => cw.IsDownloadable == false)
                                       .ExecuteAsync();

                        cachedClanWar = new CachedClanWar(cachedClan.ClanTag, currentWar);

                        await SqlWriter.Insert(cachedClanWar).ExecuteAsync();

                        CachedWar? cachedWar = await SqlWriter.Select<CachedWar>()
                                                              .Where(w => (w.ClanTag == cachedClan.ClanTag ||
                                                                           w.OpponentTag == cachedClan.ClanTag) &&
                                                                           w.PrepStartTime == currentWar.PreparationStartTimeUtc)
                                                              .QuerySingleOrDefaultAsync();

                        if (cachedWar == null)
                        {
                            cachedWar = new CachedWar(currentWar);

                            await SqlWriter.Insert(cachedWar).ExecuteAsync();

                            CocApi.Wars.OnNewWar(currentWar);
                        }
                    }
                    else
                    {
                        cachedClan.WarUpdatesAt = currentWar.EffectiveExpiration();

                        currentWar.Announcements = cachedClanWar.ToCurrentWar().Announcements;

                        cachedClanWar.Json = currentWar.ToJson();

                        cachedClanWar.UpdatesAt = currentWar.EffectiveExpiration();

                        cachedClanWar.WarState = currentWar.State;

                        cachedClanWar.IsDownloadable = true;

                        await SqlWriter.Update(cachedClanWar).ExecuteAsync();
                    }
                }
                finally
                {
                    UpdatingClanWar.TryRemove(currentWar.WarKey(), out _);
                }
            }
        }

        private async Task UpdateClanVillages(CachedClan cachedClan, Clan? clan)
        {
            if (StopRequested || clan == null || DownloadClanVillages == false || cachedClan.DownloadVillages == false)
                return;

            var query = SqlWriter.Select<CachedVillage>();

            foreach (ClanVillage clanVillage in clan.Villages.EmptyIfNull())
                query.OrWhere(cv => cv.VillageTag == clanVillage.VillageTag);

            List<CachedVillage> cachedVillages = await query.QueryToListAsync();

            foreach (ClanVillage clanVillage in clan.Villages.EmptyIfNull())
            {
                if (cachedVillages.Any(cv => cv.VillageTag == clanVillage.VillageTag))
                    continue;

                CachedVillage cachedVillage = new CachedVillage { VillageTag = clanVillage.VillageTag, Download = false };

                await SqlWriter.Insert(cachedVillage).ExecuteAsync();

                cachedVillages.Add(cachedVillage);
            }

            List<Task> tasks = new List<Task>();

            foreach (CachedVillage cachedVillage in cachedVillages)
                tasks.Add(UpdateVillage(cachedVillage));

            await Task.WhenAll(tasks);
        }

        private async Task UpdateVillage(CachedVillage cachedVillage)
        {
            if (StopRequested)
                return;

            if (DateTime.UtcNow < cachedVillage.UpdatesAt)
                return;

            if (!(UpdatingVillage.TryAdd(cachedVillage.VillageTag, cachedVillage)))
                return;

            try
            {
                Village? fetchedVillage = await CocApi.Villages.FetchAsync(cachedVillage.VillageTag);

                Village? storedVillage = null;

                if (cachedVillage.Json != null)
                    storedVillage = JsonConvert.DeserializeObject<Village>(cachedVillage.Json);

                if (storedVillage != null && fetchedVillage != null)
                    storedVillage.Update(CocApi, fetchedVillage);

                if (fetchedVillage != null)
                {
                    cachedVillage.Json = JsonConvert.SerializeObject(fetchedVillage);

                    cachedVillage.UpdatesAt = fetchedVillage.EffectiveExpiration();
                }

                await SqlWriter.Update(cachedVillage).ExecuteAsync();
            }
            finally
            {
                UpdatingVillage.TryRemove(cachedVillage.VillageTag, out CachedVillage _);
            }
        }
    }
}