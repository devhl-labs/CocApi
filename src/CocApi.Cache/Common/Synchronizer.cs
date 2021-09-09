using System.Collections.Concurrent;
using CocApi.Cache.Context.CachedItems;
using CocApi.Model;

namespace CocApi.Cache.Services
{
    public sealed class Synchronizer
    {
        public static bool Instantiated { get; private set; }


        internal ConcurrentDictionary<string, CachedClan?> UpdatingClan { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingWar { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingCwlWar { get; } = new();
        internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new();


        public Synchronizer()
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
        }
    }
}
