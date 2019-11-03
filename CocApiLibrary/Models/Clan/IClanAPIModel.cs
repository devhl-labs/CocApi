using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public interface IClanApiModel
    {
        string ClanTag { get; }

        string Name { get; }

        int ClanLevel { get; set; }

        ClanBadgeUrlApiModel? BadgeUrls { get; set; }
    }
}
