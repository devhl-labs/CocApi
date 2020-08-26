using CocApi.Cache.Models;
using CocApi.Client;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace CocApi.Cache
{
    public static class Utils
    {
        //public static CachedItem_newnew<T> ToCachedItem<T>(this ApiResponse<T> response, TimeSpan localExpiration) where T : class
        //{
        //    string downloadDateString = response.Headers.First(h => h.Key == "Date").Value.First();
        //    DateTime downloadDate = DateTime.ParseExact(downloadDateString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
        //    string cacheControlString = response.Headers.First(h => h.Key == "Cache-Control").Value.First().Replace("max-age=", "");
        //    double cacheControl = double.Parse(cacheControlString);
        //    DateTime serverExpiration = downloadDate.AddSeconds(cacheControl);

        //    CachedItem_newnew<T> result = new CachedItem_newnew<T>
        //    {
        //        DownloadDate = downloadDate,
        //        RawContent = response.RawContent,
        //        ServerExpirationDate = serverExpiration,
        //        Data = response.Data
        //    };

        //    result.LocalExpirationDate = result.DownloadDate.Add(localExpiration);

        //    return result;
        //}

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        public static string ToEnumMemberAttrValue(this Enum @enum)
        {
            var attr =
                @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?.
                    GetCustomAttributes(false).OfType<EnumMemberAttribute>().
                    FirstOrDefault();
            if (attr == null)
                return @enum.ToString();
            return attr.Value;
        }

        //public static string ToJson(this object? obj)
        //{
        //    JsonSerializerSettings settings = new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.Objects,
        //        SerializationBinder = new KnownTypesBinder()                
        //    };

        //    return JsonConvert.SerializeObject(obj, settings);
        //}

        //public static T? Deserialize<T>(this string str) where T : class
        //{
        //    JsonSerializerSettings settings = new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.All,
        //        SerializationBinder = new KnownTypesBinder()
        //    };

        //    return JsonConvert.DeserializeObject<T>(str, settings);
        //}
    }
}
