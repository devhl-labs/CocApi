/* 
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class PlayersApi
    {
        private CocApi.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class.
        /// </summary>
        /// <returns></returns>
        public PlayersApi() : this((string) null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class.
        /// </summary>
        /// <returns></returns>
        public PlayersApi(String basePath)
        {
            this.Configuration = CocApi.Client.Configuration.MergeConfigurations(
                CocApi.Client.GlobalConfiguration.Instance,
                new CocApi.Client.Configuration { BasePath = basePath }
            );
            this.Client = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            this.ExceptionFactory = CocApi.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public PlayersApi(CocApi.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = CocApi.Client.Configuration.MergeConfigurations(
                CocApi.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.Client = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            ExceptionFactory = CocApi.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public PlayersApi(CocApi.Client.ISynchronousClient client,CocApi.Client.IAsynchronousClient asyncClient, CocApi.Client.IReadableConfiguration configuration)
        {
            if(client == null) throw new ArgumentNullException("client");
            if(asyncClient == null) throw new ArgumentNullException("asyncClient");
            if(configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = CocApi.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public CocApi.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public CocApi.Client.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public CocApi.Client.IReadableConfiguration Configuration {get; set;}

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public CocApi.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }



        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <returns>Task of Player</returns>
        public async System.Threading.Tasks.Task<Player> GetPlayerAsync (string playerTag)
        {
             CocApi.Client.ApiResponse<Player> localVarResponse = await GetPlayerWithHttpInfoAsync(playerTag);
             return localVarResponse.Data;

        }

        ///// <summary>
        ///// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        ///// </summary>
        ///// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        ///// <param name="playerTag">Tag of the player.</param>
        ///// <returns>Task of ApiResponse (Player)</returns>
        //public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<Player>> GetPlayerWithHttpInfoAsync (string playerTag)
        //{
        //    // verify the required parameter 'playerTag' is set
        //    if (playerTag == null)
        //        throw new CocApi.Client.ApiException(400, "Missing required parameter 'playerTag' when calling PlayersApi->GetPlayer");


        //    CocApi.Client.RequestOptions localVarRequestOptions = new CocApi.Client.RequestOptions();

        //    String[] _contentTypes = new String[] {
        //    };

        //    // to determine the Accept header
        //    String[] _accepts = new String[] {
        //        "application/json"
        //    };


        //    var localVarContentType = CocApi.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
        //    if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

        //    var localVarAccept = CocApi.Client.ClientUtils.SelectHeaderAccept(_accepts);
        //    if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            
        //    localVarRequestOptions.PathParameters.Add("playerTag", CocApi.Client.ClientUtils.ParameterToString(playerTag)); // path parameter

        //    // authentication (JWT) required
        //    localVarRequestOptions.HeaderParameters.Add("authorization", "Bearer " + await this.Configuration.GetTokenAsync());
            

        //    // make the HTTP request

        //    var localVarResponse = await this.AsynchronousClient.GetAsync<Player>("/players/{playerTag}", localVarRequestOptions, this.Configuration);

        //    if (this.ExceptionFactory != null)
        //    {
        //        Exception _exception = this.ExceptionFactory("GetPlayer", localVarResponse);
        //        if (_exception != null) throw _exception;
        //    }

        //    return localVarResponse;
        //}

    }
}