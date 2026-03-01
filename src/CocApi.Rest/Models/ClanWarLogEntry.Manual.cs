using System.Collections.Generic;
using System.Linq;

namespace CocApi.Rest.Models
{
    public partial class ClanWarLogEntry
    {
        public SortedDictionary<string, WarClanLogEntry> Clans { get; private set; } = new SortedDictionary<string, WarClanLogEntry>();

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

        partial void OnCreated()
        {
            if (Clan?.Tag != null && Opponent?.Tag != null)
                Clans = new SortedDictionary<string, WarClanLogEntry>
                {
                    { Clan.Tag, Clan },
                    { Opponent.Tag, Opponent }
                };
        }
    }
}
