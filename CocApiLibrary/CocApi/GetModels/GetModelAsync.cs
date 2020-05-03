//using System;
//using System.Threading.Tasks;
//using System.Threading;

//using devhl.CocApi.Models;
//using devhl.CocApi.Exceptions;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;
//using System.Linq;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics.Contracts;
//using System.ComponentModel;
//using System.Transactions;

//namespace devhl.CocApi
//{
//    public sealed partial class CocApi : IDisposable
//    {
//        ////todo prob need to delete these
//        //private void UpdateDictionary<T>(ConcurrentDictionary<string, T> dictionary, string key, T downloadable) where T : IDownloadable
//        //{
//        //    dictionary.AddOrUpdate(key, downloadable, (_, existingItem) =>
//        //    {
//        //        if (downloadable.ServerExpirationUtc > existingItem.ServerExpirationUtc)
//        //            return downloadable;

//        //        return existingItem;
//        //    });
//        //}

//        //private void UpdateCurrentWar(string key, CurrentWar currentWar)
//        //{
//        //    AllWarsByWarKey.AddOrUpdate(key, currentWar, (_, existingItem) =>
//        //    {
//        //        if (currentWar.ServerExpirationUtc > existingItem.ServerExpirationUtc)
//        //            return currentWar;

//        //        return existingItem;
//        //    });
//        //}







//    }

//}
