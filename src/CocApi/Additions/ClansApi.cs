using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Api
{
    public partial class ClansApi
    {
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetClanWarLeagueWarResponseAsync(string warTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await getClanWarLeagueWarResponseAsync(warTag, cancellationToken);

            //response.Data.ServerResponseExpires = response.ServerExpiration;

            response.Data.WarTag = warTag;

            response.Data.Initialize();

            return response;
        }

        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetCurrentWarResponseAsync(string clanTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await getCurrentWarResponseAsync(clanTag, cancellationToken);

            //response.Data.ServerResponseExpires = response.ServerExpiration;

            response.Data.Initialize();

            return response;
        }
    }
}
