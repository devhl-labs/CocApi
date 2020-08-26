using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanWarEventArgs : EventArgs
    {
        public ClanWar ClanWar { get; }

        public ClanWarEventArgs(ClanWar clanWar)
        {
            ClanWar = clanWar;
        }
    }
}
