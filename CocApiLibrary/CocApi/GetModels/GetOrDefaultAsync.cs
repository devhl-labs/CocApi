//using System;
//using System.Threading.Tasks;
//using System.Threading;

//using devhl.CocApi.Models;
//using devhl.CocApi.Exceptions;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;

//namespace devhl.CocApi
//{
//    public sealed partial class CocApi : IDisposable
//    {
//        //public async Task<Downloadable?> GetClanOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
//        //{
//        //    Downloadable? result = null;

//        //    try
//        //    {
//        //        result = await GetClanAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
//        //    }
//        //    catch (ServerResponseException) { }
//        //    catch (Exception)
//        //    {
//        //        throw;
//        //    }

//        //    return result;
//        //}

//        //public async Task<Downloadable?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
//        //{
//        //    Downloadable? result = null;

//        //    try
//        //    {
//        //        result = await GetLeagueGroupAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
//        //    }
//        //    catch (ServerResponseException) { }
//        //    catch (Exception)
//        //    {
//        //        throw;
//        //    }

//        //    return result;
//        //}

//        //public async Task<Downloadable?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
//        //{
//        //    Downloadable? result = null;

//        //    try
//        //    {
//        //        result = await GetCurrentWarAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
//        //    }
//        //    catch (ServerResponseException) { }
//        //    catch (Exception)
//        //    {
//        //        throw;
//        //    }

//        //    return result;
//        //}

//        //public async Task<Downloadable?> GetLeagueWarOrDefaultAsync(string warTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
//        //{
//        //    Downloadable? result = null;

//        //    try
//        //    {
//        //        result = await GetLeagueWarAsync(warTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
//        //    }
//        //    catch (ServerResponseException) { }
//        //    catch (Exception)
//        //    {
//        //        throw;
//        //    }

//        //    return result;
//        //}

//        //public async Task<Downloadable?> GetVillageOrDefaultAsync(string villageTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
//        //{
//        //    Downloadable? result = null;

//        //    try
//        //    {
//        //        result = await GetVillageAsync(villageTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
//        //    }
//        //    catch (ServerResponseException) { }
//        //    catch (Exception)
//        //    {
//        //        throw;
//        //    }

//        //    return result;
//        //}


//    }
//}
