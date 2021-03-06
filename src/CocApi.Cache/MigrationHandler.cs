﻿using System;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Cache
{
    public static class MigrationHandler
    {
        public static void Migrate(string connectionString = "Data Source=CocApi.Cache.sqlite")
        {
            DbContextOptionsBuilder<CacheContext> optionsBuilder = new DbContextOptionsBuilder<CacheContext>();
            
            optionsBuilder.UseSqlite(connectionString);

            CacheContext cacheContext = new CacheContext(optionsBuilder.Options);

            cacheContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

            cacheContext.Database.Migrate();
        }
    }
}
