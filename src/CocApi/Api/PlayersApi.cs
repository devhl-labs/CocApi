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
using System.Collections.Immutable;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class PlayersApi
    {
        private readonly CocApi.TokenProvider _tokenProvider;
        private CocApi.Client.ExceptionFactory _exceptionFactory = (name, response) => null;
        public delegate System.Threading.Tasks.Task HttpRequestResultEventHandler(object sender, HttpRequestResultEventArgs log);        
        public event HttpRequestResultEventHandler HttpRequestResult;
        private readonly System.Collections.Concurrent.ConcurrentBag<IHttpRequestResult> _httpRequestResults = new System.Collections.Concurrent.ConcurrentBag<IHttpRequestResult>();
        internal void OnHttpRequestResult(HttpRequestResultEventArgs log) => HttpRequestResult?.Invoke(this, log);
        public ImmutableArray<IHttpRequestResult> HttpRequestResults => _httpRequestResults.ToImmutableArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class.
        /// </summary>
        /// <returns></returns>
        public PlayersApi(CocApi.TokenProvider tokenProvider, TimeSpan? httpRequestTimeOut = null, string basePath = "https://api.clashofclans.com/v1")
        {
            this.Configuration = CocApi.Client.Configuration.MergeConfigurations(
                CocApi.Client.GlobalConfiguration.Instance,
                new CocApi.Client.Configuration { BasePath = basePath, Timeout = ((int?)httpRequestTimeOut?.TotalMilliseconds) ?? 100000  }
            );
            this.Client = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new CocApi.Client.ApiClient(this.Configuration.BasePath);
            this.ExceptionFactory = CocApi.Client.Configuration.DefaultExceptionFactory;
            this._tokenProvider = tokenProvider;
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
        public async System.Threading.Tasks.Task<Player> GetPlayerAsync (string playerTag, System.Threading.CancellationToken? cancellationToken = default)
        {
             CocApi.Client.ApiResponse<Player> localVarResponse = await GetPlayerResponseAsync(playerTag,  cancellationToken.GetValueOrDefault());
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <returns>Task of ApiResponse (Player)</returns>
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<Player>> GetPlayerResponseAsync (string playerTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            // verify the required parameter 'playerTag' is set
            if (playerTag == null)
                throw new CocApi.Client.ApiException(400, "Missing required parameter 'playerTag' when calling PlayersApi->GetPlayer");

            string formattedTag = Clash.FormatTag(playerTag);

            CocApi.Client.RequestOptions localVarRequestOptions = new CocApi.Client.RequestOptions();

            String[] _contentTypes = new String[] {
            };

            // to determine the Accept header
            String[] _accepts = new String[] {
                "application/json"
            };

            var localVarContentType = CocApi.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = CocApi.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            
            localVarRequestOptions.PathParameters.Add("playerTag", CocApi.Client.ClientUtils.ParameterToString(formattedTag)); // path parameter  //playerTag

            // authentication (JWT) required
            localVarRequestOptions.HeaderParameters.Add("authorization", "Bearer " + await _tokenProvider.GetTokenAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false));
            

            // make the HTTP request
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            ApiResponse<Player>? localVarResponse = null;

            try
            {
                localVarResponse = await this.AsynchronousClient.GetAsync<Player>("/players/{playerTag}", localVarRequestOptions, this.Configuration, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                stopwatch.Stop();

                HttpRequestException requestException = new HttpRequestException("/players/{playerTag}", localVarRequestOptions, stopwatch.Elapsed, e);

                _httpRequestResults.Add(requestException);

                OnHttpRequestResult(new HttpRequestResultEventArgs(requestException));

                throw;
            }

            stopwatch.Stop();

            if (localVarResponse.ErrorText == "The request timed-out." || localVarResponse.ErrorText == "The operation has timed out.")
            {
                TimeoutException timeoutException = new TimeoutException(localVarResponse.ErrorText);

                HttpRequestException requestException = new HttpRequestException("/players/{playerTag}", localVarRequestOptions, stopwatch.Elapsed, timeoutException);

                _httpRequestResults.Add(requestException);

                OnHttpRequestResult(new HttpRequestResultEventArgs(requestException));

                throw timeoutException;
            }

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("GetPlayer", localVarResponse);
                if (_exception != null) 
                {
                    HttpRequestException requestException = new HttpRequestException("/players/{playerTag}", localVarRequestOptions, stopwatch.Elapsed, _exception);

                    _httpRequestResults.Add(requestException);

                    OnHttpRequestResult(new HttpRequestResultEventArgs(requestException));

                    throw _exception;
                }
            }

            HttpRequestSuccess requestSuccess = new HttpRequestSuccess("/players/{playerTag}", localVarRequestOptions, stopwatch.Elapsed, localVarResponse.StatusCode);

            _httpRequestResults.Add(requestSuccess);

            OnHttpRequestResult(new HttpRequestResultEventArgs(requestSuccess));

            return localVarResponse;
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <returns>Task of ApiResponse (Player)</returns>
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<Player>?> GetPlayerResponseOrDefaultAsync (string playerTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            try
            {
                return await GetPlayerResponseAsync (playerTag, cancellationToken.GetValueOrDefault());
            }
            catch(ApiException)
            {
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="CocApi.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <returns>Task of Player</returns>
        public async System.Threading.Tasks.Task<Player?> GetPlayerOrDefaultAsync (string playerTag, System.Threading.CancellationToken? cancellationToken = default)
        {
             CocApi.Client.ApiResponse<Player>? localVarResponse = await GetPlayerResponseOrDefaultAsync(playerTag, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
             if (localVarResponse == null)
                return null;

             return localVarResponse.Data;
        }

    }
}