using System;
using CocApi.Rest.Client;

namespace CocApi.Cache;

internal class SeenCwlWar
{
    public DateTime Season { get; }

    public string ClanTag { get; }

    public string OpponentTag { get; }

    public string WarTag { get; }
    
    public IOk<Rest.Models.ClanWar>? ApiResponse { get; set; }

    public SeenCwlWar(DateTime season, string clanTag, string opponentTag, string warTag, IOk<Rest.Models.ClanWar>? apiResponse)
    {
        ApiResponse = apiResponse;
        Season = season;
        ClanTag = clanTag;
        OpponentTag = opponentTag;
        WarTag = warTag;
    }
}