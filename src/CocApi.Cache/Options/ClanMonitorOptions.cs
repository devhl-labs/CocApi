using System;

namespace CocApi.Cache
{
    public class ClanMonitorOptions : MonitorOptions
    {
        public bool DisableClan { get; set; }

        public bool DisableGroup { get; set; }

        public bool DisableCurrentWar { get; set; }

        public bool DisableWarLog { get; set; }
    }
}
