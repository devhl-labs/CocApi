using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocApi.Model;

namespace CocApi.Model
{
    public partial class ClanWarLogEntry
    {
        private SortedDictionary<string, WarClanLogEntry> _clans;

        private readonly object _clansLock = new object();

        public SortedDictionary<string, WarClanLogEntry> Clans
        {
            get
            {
                lock (_clansLock)
                {
                    if (_clans != null)
                        return _clans;

                    _clans = new SortedDictionary<string, WarClanLogEntry>();

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
