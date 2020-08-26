//using Dapper.SqlWriter;
////using CocApi.Cache.Models;
////using CocApi.Cache.Models.Cache;
////using CocApi.Cache.Models.Clans;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using CocApi.Model;
//using CocApi.Api;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.EntityFrameworkCore;
//using CocApi.Client;
//using CocApi.Cache.Models.Cache;
//using CocApi.Cache.CocApi;
//using System.Text.RegularExpressions;

//namespace CocApi.Cache
//{
//    public sealed class ClansCache
//    {
//        private readonly ClansApi _clansApi;
//        private readonly CachedContext _dbContext;

//        public ClansCache(ClansApi clansApi, CachedContext dbContext)
//        {
//            _clansApi = clansApi;
//            _dbContext = dbContext;
//        }

//        public async Task<Clan> GetClanAsync(string tag)
//        {
//            CachedClan result = await GetCacheAsync(tag).ConfigureAwait(false);

//            return result.Data;
//        }

//        public async Task<Clan?> GetClanOrDefaultAsync(string tag)
//        {
//            CachedClan? result = await GetCacheOrDefaultAsync(tag).ConfigureAwait(false);

//            return result?.Data;
//        }

//        public async Task<CachedClan> GetCacheAsync(string tag)
//        {
//            return await _dbContext.Clans.Where(i => i.Tag == tag).FirstAsync().ConfigureAwait(false);
//        }

//        public async Task<CachedClan?> GetCacheOrDefaultAsync(string tag)
//        {
//            return await _dbContext.Clans.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);
//        }

//        public async Task<Clan> GetOrFetchClanAsync(string tag)
//        {
//            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
//                ?? await _clansApi.GetClanAsync(tag).ConfigureAwait(false);
//        }

//        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag)
//        {
//            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
//                ?? await _clansApi.GetClanOrDefaultAsync(tag).ConfigureAwait(false);
//        }

//        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
//        {
//            if (downloadClan == false && downloadMembers == true)
//                throw new Exception("DownloadClan must be true to download members.");

//            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
//                throw new InvalidTagException(tag);

//            CachedClan cachedClan = await _dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

//            if (cachedClan != null)
//                return;

//            await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers, _dbContext);

//            return;
//        }

//        public async Task UpdateAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
//        {
//            if (downloadClan == false && downloadMembers == true)
//                throw new Exception("DownloadClan must be true to download members.");

//            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
//                throw new InvalidTagException(tag);

//            CachedClan cachedClan = await _dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

//            if (cachedClan == null)
//            {
//                await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers, _dbContext);

//                return;
//            }

//            cachedClan.Tag = formattedTag;
//            cachedClan.Download = downloadClan;
//            cachedClan.DownloadCurrentWar = downloadWars;
//            cachedClan.DownloadCwl = downloadCwl;
//            cachedClan.DownloadMembers = downloadMembers;

//            _dbContext.Clans.Update(cachedClan);

//            await _dbContext.SaveChangesAsync();
//        }

//        private async Task InsertCachedClanAsync(string formattedTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers, CachedContext dbContext)
//        {
//            CachedClan cachedClan = new CachedClan
//            {
//                Tag = formattedTag,
//                Download = downloadClan,
//                DownloadCurrentWar = downloadWars,
//                DownloadCwl = downloadCwl,
//                DownloadMembers = downloadMembers
//            };

//            dbContext.Clans.Update(cachedClan);

//            CachedClanWar cachedClanWar = new CachedClanWar
//            {
//                Tag = formattedTag
//            };

//            dbContext.ClanWars.Update(cachedClanWar);

//            CachedClanWarLeagueGroup group = new CachedClanWarLeagueGroup
//            {
//                Tag = formattedTag
//            };

//            dbContext.Groups.Update(group);

//            CachedClanWarLog log = new CachedClanWarLog
//            {
//                Tag = formattedTag
//            };

//            dbContext.WarLogs.Update(log);

//            await dbContext.SaveChangesAsync();

//            return;
//        }
//    }
//}