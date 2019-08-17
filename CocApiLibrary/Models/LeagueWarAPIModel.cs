using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public class LeagueWarAPIModel : CurrentWarAPIModel, IInitialize, ICurrentWarAPIModel
    {

        public string WarTag { get; set; } = string.Empty;

        public new void Initialize()
        {
            base.Initialize();
            WarType = Enums.WarType.SCCWL;
        }
    }
}
