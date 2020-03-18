using devhl.CocApi.Models.Clan;
using System.Collections.Generic;

namespace devhl.CocApi.Models.War
{
    public interface IWarClan
    {
        string ClanTag { get; }

        decimal DestructionPercentage { get; }
        
        Result Result { get; }

        int Stars { get; }
    }
}