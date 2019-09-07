//using AutoMapper;
using CocApiLibrary;
using CocApiLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace CocApiLibrary
{
    public static class Extensions
    {
        public const string PopDirecitonalFormatting = "\u202C";
        public const string LeftToRightEmbedding = "\u202A";
        public const string LeftToRightOverride = "\u202D";
        public const string LeftToRightIsolate = "\u2066";
        public const string PopDirectionalIsolate = "\u2069";

        /// <summary>
        /// Discord markup characters are stripped out
        /// Includes * _ ~ and `
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DiscordSafe(this String str)
        {
            return str.Replace("*", "").Replace("_", "").Replace("~", "").Replace("`", "");
        }

        /// <summary>
        /// Left aligns provided text.  Helpful for making tables that may have right aligned text buried within.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string LeftToRight(this String str)
        {
            return $"{LeftToRightOverride}{LeftToRightIsolate}{str}{PopDirectionalIsolate}{PopDirecitonalFormatting}";
        }

        /// <summary>
        /// Convert the date time string that SuperCell provides into a DateTime object.  It should look like this "20190710T224931.000Z"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str)
        {
            DateTime result = DateTime.ParseExact(str.Replace(".000Z", "").Replace("T", " "), "yyyyMMdd HHmmss", null);

            result = DateTime.SpecifyKind(result, DateTimeKind.Utc);

            return result;
        }

        /// <summary>
        /// Checks if the collection is null then iterates the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            if (self != null)
            {
                foreach (var element in self)
                {
                    action(element);
                }
            }
        }


        //public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(this IMappingExpression<TSource, TDestination> map, Expression<Func<TDestination, object>> selector)
        //{
        //    map.ForMember(selector, config => config.Ignore());
        //    return map;
        //}





        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.TryGetValue(key, out TValue val))
            {
                val = new TValue();

                dict.TryAdd(key, val);
            }

            return val;
        }

        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.TryAdd(key, value);
            }

            return dictionary;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

    }
}
