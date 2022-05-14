using System.Collections.Generic;
using System.Linq;

namespace CocApi.Rest.Models
{
    public partial class ClanWarLogEntry
    {
        private readonly SortedDictionary<string, WarClanLogEntry> _clans = new();
        private readonly object _clansLock = new();
        public SortedDictionary<string, WarClanLogEntry> Clans
        {
            get
            {
                lock (_clansLock)
                {
                    if (_clans.Count > 0)
                        return _clans;
                    if (Clan != null)
                        _clans.Add(Clan.Tag, Clan);
                    if (Opponent.Tag != null)
                        _clans.Add(Opponent.Tag, Opponent);
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
