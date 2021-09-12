using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Cache
{
    public class CacheDbContext : DbContext
    {
        public DbSet<CachedClan> Clans { get; set; }

        public DbSet<CachedPlayer> Players { get; set; }

        public DbSet<CachedWar> Wars { get; set; }

        public CacheDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Clans
            var clan = builder.Entity<CachedClan>().ToTable(Library.TableNames.Clans);
            clan.HasIndex(p => p.Id).IsUnique();
            clan.HasIndex(p => p.KeepUntil);
            clan.HasIndex(p => p.ExpiresAt);
            clan.HasIndex(p => p.Tag).IsUnique();

            // Current War
            builder.Entity<CachedClan>().OwnsOne(p => p.CurrentWar, war =>
            {
                war.ToTable(Library.TableNames.CurrentWar);
                war.HasIndex(w => w.KeepUntil);
                war.HasIndex(w => w.ExpiresAt);
                war.HasIndex(w => w.EnemyTag);
                war.HasIndex(w => new { w.CachedClanId, w.Download });
                war.HasIndex(w => new { w.Added, w.CachedClanId, w.State });
            });

            // WarLog
            builder.Entity<CachedClan>().OwnsOne(p => p.WarLog, log =>
            {
                log.ToTable(Library.TableNames.WarLog);
                log.HasIndex(w => w.KeepUntil);
                log.HasIndex(w => w.ExpiresAt);
            });

            // Group
            builder.Entity<CachedClan>().OwnsOne(p => p.Group, group => {
                group.ToTable(Library.TableNames.Group);
                group.HasIndex(w => w.KeepUntil);
                group.HasIndex(w => w.ExpiresAt);
            });

            // Player
            var player = builder.Entity<CachedPlayer>().ToTable(Library.TableNames.Player);
            player.HasIndex(p => p.Id).IsUnique();
            player.HasIndex(p => p.KeepUntil);
            player.HasIndex(p => p.ExpiresAt);
            player.HasIndex(p => p.Tag).IsUnique();
            player.HasIndex(p => p.ClanTag);

            // War
            var war = builder.Entity<CachedWar>().ToTable(Library.TableNames.War);
            war.HasIndex(p => p.Id).IsUnique();
            war.HasIndex(p => new { p.PreparationStartTime, p.ClanTag, p.OpponentTag }).IsUnique();
            war.HasIndex(p => p.KeepUntil);
            war.HasIndex(p => p.ExpiresAt);
            war.HasIndex(p => p.ClanTag);
            war.HasIndex(p => p.OpponentTag);
            war.HasIndex(p => p.IsFinal);
            war.HasIndex(p => p.WarTag);
            war.HasIndex(p => p.Season);
        }
    }
}
