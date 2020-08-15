using System;
using System.Collections.Generic;
using System.Text;
using CocApi.Model;

namespace CocApi.Api
{
    public partial class PlayersApi
    {
        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <returns>Task of ApiResponse (Player)</returns>
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<Player>> GetPlayerWithHttpInfoAsync(string playerTag)
        {
            // verify the required parameter 'playerTag' is set
            if (playerTag == null)
                throw new CocApi.Client.ApiException(400, "Missing required parameter 'playerTag' when calling PlayersApi->GetPlayer");

            if (Clash.TryGetValidTag(playerTag, out string formattedTag) == false)
                throw new CocApi.InvalidTagException(playerTag);

            CocApi.Client.RequestOptions localVarRequestOptions = new CocApi.Client.RequestOptions();

            String[] _contentTypes = new String[] {
            };

            // to determine the Accept header
            String[] _accepts = new String[] {
                "application/json"
            };


            var localVarContentType = CocApi.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = CocApi.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("playerTag", CocApi.Client.ClientUtils.ParameterToString(formattedTag)); // path parameter

            // authentication (JWT) required
            localVarRequestOptions.HeaderParameters.Add("authorization", "Bearer " + await this.Configuration.GetTokenAsync());


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.GetAsync<Player>("/players/{playerTag}", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("GetPlayer", localVarResponse);
                if (_exception != null)
                    throw _exception;
            }

            return localVarResponse;
        }
    }
}
