using CocApi.Cache.Models.Clans;
using System.Collections.Generic;

namespace CocApi.Cache.Models.Wars
{
    public interface IWarClan
    {
        string ClanTag { get; }

        decimal DestructionPercentage { get; }
        
        Result Result { get; }

        int Stars { get; }
    }
}