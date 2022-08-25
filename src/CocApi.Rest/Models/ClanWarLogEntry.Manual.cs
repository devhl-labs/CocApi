using System.Collections.Generic;
using System.Linq;

namespace CocApi.Rest.Models
{
    public partial class ClanWarLogEntry
    {
        private volatile SortedDictionary<string, WarClanLogEntry>? _clans;
        private readonly object _clansLock = new();
        public SortedDictionary<string, WarClanLogEntry> Clans
        {
            get
            {
                if (_clans != null) // avoid the lock if we can
                    return _clans;

                lock (_clansLock)
                {
                    if (_clans != null)
                        return _clans;

                    _clans = (Clan?.Tag == null || Opponent?.Tag == null)
                        ? new SortedDictionary<string, WarClanLogEntry>()
                        : new SortedDictionary<string, WarClanLogEntry>
                        {
                            { Clan.Tag, Clan },
                            { Opponent.Tag, Opponent }
                        };

                    return _clans;
                }
            }
        }

        public WarType WarType
        {
            get
            {
                if (Opponent == null || Opponent.ClanLevel == 0)
                    return WarType.SCCWL;
                if (Clans.All(c => c.Value.ExpEarned == 0))
                    return WarType.Friendly;
                return WarType.Random;
            }
        }
    }
}
