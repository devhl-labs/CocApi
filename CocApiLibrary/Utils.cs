using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace devhl.CocApi
{
    public static class Utils
    {
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

        public static string ToJson(this object? obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new KnownTypesBinder()                
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T? Deserialize<T>(this string str) where T : class
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = new KnownTypesBinder()
            };

            return JsonConvert.DeserializeObject<T>(str, settings);
        }
    }
}
