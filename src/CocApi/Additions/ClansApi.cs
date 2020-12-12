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
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetClanWarLeagueWarResponseAsync(string token, string warTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await getClanWarLeagueWarResponseAsync(token, warTag, cancellationToken);

            //response.Data.WarTag = warTag;

            //response.Data.ServerExpiration = response.ServerExpiration;

            response.Data.Initialize(response.ServerExpiration, warTag);

            return response;
        }

        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetCurrentWarResponseAsync(string token, string clanTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await getCurrentWarResponseAsync(token, clanTag, cancellationToken);

            //response.Data.ServerExpiration = response.ServerExpiration;

            response.Data.Initialize(response.ServerExpiration, null);

            return response;
        }
    }
}
