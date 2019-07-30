using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public class LeagueWarAPIModel : CurrentWarAPIModel
    {

        public string WarTag { get; set; } = string.Empty;

        internal new void Process()
        {
            base.Process();
            WarType = Enums.WarType.SCCWL;
        }
    }
}
