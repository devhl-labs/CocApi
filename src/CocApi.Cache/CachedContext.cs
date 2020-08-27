﻿using CocApi.Cache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    public class CachedContext : DbContext
    {
        public DbSet<CachedClan> Clans { get; set; }

        public DbSet<CachedClanWar> ClanWars { get; set; }

        public DbSet<CachedPlayer> Players { get; set; }

        public DbSet<CachedWar> Wars { get; set; }

        public DbSet<CachedClanWarLeagueGroup> Groups { get; set; }

        public DbSet<CachedClanWarLog> WarLogs { get; set; }

        public CachedContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CachedPlayer>().HasIndex(p => p.Tag).IsUnique();

            builder.Entity<CachedClan>().HasIndex(p => p.Tag).IsUnique();

            builder.Entity<CachedClanWar>().HasIndex(p => p.Tag).IsUnique();

            builder.Entity<CachedClanWar>().HasIndex(p => new { p.Tag, p.PreparationStartTime }).IsUnique();

            builder.Entity<CachedWar>().HasIndex(p => new { p.PreparationStartTime, p.OpponentTag }).IsUnique();

            builder.Entity<CachedWar>().HasIndex(p => new { p.PreparationStartTime, p.ClanTag }).IsUnique();

            builder.Entity<CachedWar>().HasIndex(p => p.OpponentTag);

            builder.Entity<CachedWar>().HasIndex(p => p.ClanTag);

            builder.Entity<CachedWar>().HasIndex(p => p.WarTag);

            builder.Entity<CachedClanWarLeagueGroup>().HasIndex(p => p.Tag).IsUnique();

            builder.Entity<CachedClanWarLog>().HasIndex(p => p.Tag).IsUnique();
        }
    }

    public class CacheContextFactory : IDesignTimeDbContextFactory<CachedContext>
    {
        public CachedContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CachedContext>();
            optionsBuilder.UseSqlite("Data Source=cocapi.db");

            return new CachedContext(optionsBuilder.Options);
        }
    }
}