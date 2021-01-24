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
            var response = await InternalGetClanWarLeagueWarResponseAsync(warTag, cancellationToken);

            //response.Data.WarTag = warTag;

            //response.Data.ServerExpiration = response.ServerExpiration;

            if (response.Data != null)
                response.Data.Initialize(response.ServerExpiration, warTag);

            return response;
        }

        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetCurrentWarResponseAsync(string clanTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await InternalGetCurrentWarResponseAsync(clanTag, cancellationToken);

            //response.Data.ServerExpiration = response.ServerExpiration;

            if (response.Data != null)
                response.Data.Initialize(response.ServerExpiration, null);

            return response;
        }
    }
}
