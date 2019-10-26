using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public interface IClanAPIModel
    {
        string ClanTag { get; }

        string Name { get; }

        int ClanLevel { get; set; }

        BadgeUrlModel? BadgeUrls { get; set; }
    }
}
