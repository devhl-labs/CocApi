//using CocApiLibrary.Models;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Text.Json.Serialization;

//namespace CocApiLibrary.Models
//{
//    public class WarLogApiModel : IDownloadable
//    {
//        public IEnumerable<WarLogEntryModel>? Items { get; set; }

//        public PagingApiModel? Paging { get; set; }

//        public DateTime DateTimeUtc { get; internal set; } = DateTime.UtcNow;

//        public DateTime Expires { get; internal set; }

//        public string EncodedUrl { get; internal set; } = string.Empty;

//        public bool IsExpired()
//        {
//            if (DateTime.UtcNow > Expires)
//            {
//                return true;
//            }
//            return false;
//        }
//    }
//}
