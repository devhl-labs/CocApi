using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    public static class Extensions
    {
        public const string PopDirecitonalFormatting = "\u202C";
        public const string LeftToRightEmbedding = "\u202A";
        public const string LeftToRightOverride = "\u202D";
        public const string LeftToRightIsolate = "\u2066";
        public const string PopDirectionalIsolate = "\u2069";

        //public static ILogger? Logger { get; internal set; } = null;

        /// <summary>
        /// Discord markup characters are stripped out
        /// Includes *_~~`|| also {space} and >{space}
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DiscordSafe(this String str)
        {
            return str.Replace("*", "").Replace("_", "").Replace("~~", "").Replace("`", "").Replace("||", "").Replace("> ", "");
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
            DateTime result = DateTime.ParseExact(str, "yyyyMMdd'T'HHmmss.fff'Z'", null);

            result = DateTime.SpecifyKind(result, DateTimeKind.Utc);

            return result;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();
    }
}
