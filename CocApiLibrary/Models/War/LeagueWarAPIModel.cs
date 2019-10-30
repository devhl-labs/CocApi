using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public class LeagueWarApiModel : CurrentWarApiModel, IInitialize, ICurrentWarApiModel
    {

        public string WarTag { get; set; } = string.Empty;

        public new void Initialize()
        {
            base.Initialize();

            WarType = Enums.WarType.SCCWL;
        }
    }
}
