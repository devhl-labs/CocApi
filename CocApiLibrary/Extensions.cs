using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace devhl.CocApi
{
    public static class Statics
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();
    }
}
