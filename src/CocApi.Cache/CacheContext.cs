using CocApi.Cache.Models.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    public class CacheContext : DbContext
    {
        public DbSet<CachedItem> Items { get; set; }

        public DbSet<CachedClan> Clans { get; set; }

        public DbSet<CachedVillage> Villages { get; set; }

        public DbSet<CachedWar> Wars { get; set; }

        public CacheContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CachedItem>().HasIndex(p => p.Path).IsUnique();

            builder.Entity<CachedClan>().HasIndex(p => p.ClanTag).IsUnique();

            builder.Entity<CachedVillage>();

            builder.Entity<CachedWar>().HasIndex(p => new { p.PrepStartTime, p.ClanTag }).IsUnique();

            builder.Entity<CachedWar>().HasIndex(p => new { p.PrepStartTime, p.OpponentTag }).IsUnique();

            builder.Entity<CachedWar>().HasIndex(p => p.ClanTag);

            builder.Entity<CachedWar>().HasIndex(p => p.OpponentTag);
        }
    }

    public class CacheContextFactory : IDesignTimeDbContextFactory<CacheContext>
    {
        public CacheContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CacheContext>();
            optionsBuilder.UseSqlite("Data Source=cocapi.db");

            return new CacheContext(optionsBuilder.Options);
        }
    }
}
