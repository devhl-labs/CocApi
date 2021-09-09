using System;
using CocApi.Client;

namespace CocApi.Cache
{
    internal class SeenCwlWar
    {
        public DateTime Season { get; }

        public string ClanTag { get; }

        public string OpponentTag { get; }

        public string WarTag { get; }
        
        public ApiResponse<Model.ClanWar>? ApiResponse { get; set; }

        public SeenCwlWar(DateTime season, string clanTag, string opponentTag, string warTag, ApiResponse<Model.ClanWar>? apiResponse)
        {
            ApiResponse = apiResponse;
            Season = season;
            ClanTag = clanTag;
            OpponentTag = opponentTag;
            WarTag = warTag;
        }
    }
}