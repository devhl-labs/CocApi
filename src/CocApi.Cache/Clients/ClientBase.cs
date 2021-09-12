using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public class ClientBase
    {
        public IDesignTimeDbContextFactory<CacheDbContext> DbContextFactory { get; }
        public string[]? DbContextArgs { get; }
        internal Synchronizer Synchronizer { get; }
        public IPerpetualExecution<object>[] PerpetualServices { get; }


        public ClientBase(
            CacheDbContextFactoryProvider provider, 
            Synchronizer synchronizer, 
            IPerpetualExecution<object>[] perpetualServices,
            IOptions<CacheOptions> options)
        {
            Library.SetMaxConcurrentEvents(options.Value.MaxConcurrentEvents);
            DbContextFactory = provider.Factory;
            DbContextArgs = provider.DbContextArgs ?? Array.Empty<string>();
            EnsureMigrated();
            Synchronizer = synchronizer;
            PerpetualServices = perpetualServices;
        }


        public async Task ImportDataToVersion2(string connectionString)
        {
            await ImportWarsAsync(connectionString);
            await ImportClansAsync(connectionString);
            await ImportPlayersAsync(connectionString);
        }

        private async Task ImportWarsAsync(string connectionString)
        {
            int id = int.MinValue;

            List<Models.CachedWar> oldWars;

            using var dbContext2 = DbContextFactory.CreateDbContext(DbContextArgs);

            if ((await dbContext2.Wars.FirstOrDefaultAsync().ConfigureAwait(false)) != null)
                return;

            do
            {
                using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                dbContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                DbContextOptionsBuilder<CacheContext> optionsBuilder = new();

                optionsBuilder.UseSqlite(connectionString);

                using CacheContext oldContext = new(optionsBuilder.Options);

                oldContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                oldWars = await oldContext.Wars.AsNoTracking().Where(w => w.Id > id).OrderBy(w => w.Id).Take(100).ToListAsync();

                if (oldWars.Count == 0)
                    return;

                id = oldWars.Max(w => w.Id);

                foreach (Models.CachedWar oldWar in oldWars)
                {
                    CachedWar cachedWar = new()
                    {
                        Announcements = oldWar.Announcements,
                        ClanTag = oldWar.ClanTag,
                        Content = oldWar.Data,
                        Download = true, // this value is not used
                        DownloadedAt = oldWar.Downloaded,
                        EndTime = oldWar.EndTime,
                        ExpiresAt = oldWar.ServerExpiration,
                        IsFinal = oldWar.IsFinal,
                        KeepUntil = oldWar.LocalExpiration,
                        OpponentTag = oldWar.OpponentTag,
                        PreparationStartTime = oldWar.PreparationStartTime,
                        RawContent = oldWar.RawContent,
                        Season = oldWar.Season,
                        State = oldWar.State,
                        StatusCode = oldWar.StatusCode,
                        Type = !string.IsNullOrWhiteSpace(oldWar.WarTag) ? WarType.SCCWL : oldWar.Type,
                        WarTag = oldWar.WarTag
                    };

                    if (oldWar.RawContent == string.Empty)
                        cachedWar.RawContent = null;

                    if (oldWar.Season == DateTime.MinValue)
                        cachedWar.Season = null;

                    if (oldWar.StatusCode == 0)
                        cachedWar.StatusCode = null;

                    if (oldWar.WarTag == string.Empty)
                        cachedWar.WarTag = null;

                    dbContext.Wars.Add(cachedWar);
                }

                await dbContext.SaveChangesAsync();


            } while (oldWars.Count == 100);
        }

        private async Task ImportClansAsync(string connectionString)
        {
            int id = int.MinValue;

            List<Models.CachedClan> oldClans;

            using var dbContext2 = DbContextFactory.CreateDbContext(DbContextArgs);

            if ((await dbContext2.Clans.FirstOrDefaultAsync().ConfigureAwait(false)) != null)
                return;

            do
            {
                using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                dbContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                DbContextOptionsBuilder<CacheContext> optionsBuilder = new();

                optionsBuilder.UseSqlite(connectionString);

                using CacheContext oldContext = new(optionsBuilder.Options);

                oldContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                oldClans = await oldContext.Clans.AsNoTracking().Where(w => w.Id > id).OrderBy(w => w.Id).Take(100).ToListAsync();

                if (oldClans.Count == 0)
                    return;

                id = oldClans.Max(w => w.Id);

                var oldClanWars = await oldContext.ClanWars.AsNoTracking().Where(w => oldClans.Select(c => c.Tag).Contains(w.Tag)).ToListAsync();
                var oldGroups = await oldContext.Groups.AsNoTracking().Where(w => oldClans.Select(c => c.Tag).Contains(w.Tag)).ToListAsync();
                var oldWarLogs = await oldContext.WarLogs.AsNoTracking().Where(w => oldClans.Select(c => c.Tag).Contains(w.Tag)).ToListAsync();

                foreach (Models.CachedClan oldClan in oldClans)
                {
                    CachedClan cachedClan = new()
                    {
                        CurrentWar = new(),
                        Download = true,
                        DownloadedAt = DateOrNull(oldClan.Downloaded),
                        DownloadMembers = oldClan.DownloadMembers,
                        ExpiresAt = DateOrNull(oldClan.ServerExpiration),
                        Group = new(),
                        IsWarLogPublic = oldClan.IsWarLogPublic,
                        KeepUntil = DateOrNull(oldClan.LocalExpiration),
                        RawContent = string.IsNullOrWhiteSpace(oldClan.RawContent) ? null : oldClan.RawContent,
                        StatusCode = oldClan.StatusCode == 0 ? null : oldClan.StatusCode,
                        Tag = oldClan.Tag,
                        WarLog = new()
                    };

                    cachedClan.CurrentWar.Download = oldClan.DownloadCurrentWar;
                    cachedClan.Group.Download = oldClan.DownloadCwl;
                    cachedClan.WarLog.Download = false;

                    dbContext.Clans.Add(cachedClan);

                    var oldClanWar = oldClanWars.FirstOrDefault(w => w.Tag == oldClan.Tag);
                    if (oldClanWar != null)
                    {
                        cachedClan.CurrentWar.Added = false; // default it to false, new war monitor can update it
                        cachedClan.CurrentWar.DownloadedAt = DateOrNull(oldClanWar.Downloaded);
                        var dummy = oldClanWar.Data?.Clans.FirstOrDefault(c => c.Key != oldClan.Tag);
                        cachedClan.CurrentWar.EnemyTag = dummy?.Key;
                        cachedClan.CurrentWar.ExpiresAt = DateOrNull(oldClanWar.ServerExpiration);
                        cachedClan.CurrentWar.KeepUntil = DateOrNull(oldClanWar.LocalExpiration);
                        cachedClan.CurrentWar.PreparationStartTime = DateOrNull(oldClanWar.PreparationStartTime);
                        cachedClan.CurrentWar.RawContent = string.IsNullOrWhiteSpace(oldClanWar.RawContent) ? null : oldClanWar.RawContent;
                        cachedClan.CurrentWar.State = oldClanWar.State;
                        cachedClan.CurrentWar.StatusCode = oldClanWar.StatusCode == 0 ? null : oldClanWar.StatusCode;
                        cachedClan.CurrentWar.Type = oldClanWar.Type;    
                    }

                    var oldGroup = oldGroups.FirstOrDefault(w => w.Tag == oldClan.Tag);
                    if (oldGroup != null)
                    {
                        cachedClan.Group.Added = false;
                        cachedClan.Group.DownloadedAt = DateOrNull(oldGroup.Downloaded);
                        cachedClan.Group.ExpiresAt = DateOrNull(oldGroup.LocalExpiration);
                        cachedClan.Group.KeepUntil = DateOrNull(oldGroup.LocalExpiration);
                        cachedClan.Group.RawContent = string.IsNullOrWhiteSpace(oldGroup.RawContent) ? null : oldGroup.RawContent;
                        cachedClan.Group.Season = oldGroup.Season == DateTime.MinValue ? null : oldGroup.Season;
                        cachedClan.Group.State = oldGroup.State;
                        cachedClan.Group.StatusCode = oldGroup.StatusCode == 0 ? null : oldGroup.StatusCode;
                    }

                    var oldWarLog = oldWarLogs.FirstOrDefault(w => w.Tag == oldClan.Tag);
                    if (oldWarLog != null)
                    {
                        cachedClan.WarLog.DownloadedAt = DateOrNull(oldWarLog.Downloaded);
                        cachedClan.WarLog.ExpiresAt = DateOrNull(oldWarLog.ServerExpiration);
                        cachedClan.WarLog.KeepUntil = DateOrNull(oldWarLog.LocalExpiration);
                        cachedClan.WarLog.RawContent = string.IsNullOrWhiteSpace(oldWarLog.RawContent) ? null : oldWarLog.RawContent;
                        cachedClan.WarLog.StatusCode = oldWarLog.StatusCode == 0 ? null : oldWarLog.StatusCode;
                    }
                }

                await dbContext.SaveChangesAsync();


            } while (oldClans.Count == 100);
        }

        private async Task ImportPlayersAsync(string connectionString)
        {
            int id = int.MinValue;

            List<Models.CachedPlayer> oldPlayers;

            using var dbContext2 = DbContextFactory.CreateDbContext(DbContextArgs);

            if ((await dbContext2.Players.FirstOrDefaultAsync().ConfigureAwait(false)) != null)
                return;

            do
            {
                using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                dbContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                DbContextOptionsBuilder<CacheContext> optionsBuilder = new();

                optionsBuilder.UseSqlite(connectionString);

                using CacheContext oldContext = new(optionsBuilder.Options);

                oldContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                oldPlayers = await oldContext.Players.AsNoTracking().Where(w => w.Id > id).OrderBy(w => w.Id).Take(100).ToListAsync();

                if (oldPlayers.Count == 0)
                    return;

                id = oldPlayers.Max(w => w.Id);

                foreach (Models.CachedPlayer oldPlayer in oldPlayers)
                {
                    CachedPlayer cachedPlayer = new(oldPlayer.Tag)
                    {
                        ClanTag = oldPlayer.ClanTag,
                        Download = oldPlayer.Download,
                        DownloadedAt = DateOrNull(oldPlayer.Downloaded),
                        ExpiresAt = DateOrNull(oldPlayer.ServerExpiration),
                        KeepUntil = DateOrNull(oldPlayer.LocalExpiration),
                        RawContent = string.IsNullOrWhiteSpace(oldPlayer.RawContent) ? null : oldPlayer.RawContent,
                        StatusCode = oldPlayer.StatusCode == 0 ? null : oldPlayer.StatusCode,
                        Tag = oldPlayer.Tag
                    };

                    dbContext.Players.Add(cachedPlayer);
                }

                await dbContext.SaveChangesAsync();


            } while (oldPlayers.Count == 100);
        }

        private static DateTime? DateOrNull(DateTime dte) => dte == DateTime.MinValue ? null : dte;

        private void EnsureMigrated()
        {
            try
            {
                using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                dbContext.Clans.FirstOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to query the database. You may need to run a migration.", e);
            }
        }

        public void Stop()
        {
            foreach (var perptualService in PerpetualServices)
                perptualService.IsEnabled = false;
        }

        public void Start()
        {
            foreach (var perptualService in PerpetualServices)
                perptualService.IsEnabled = true;
        }
    }
}
