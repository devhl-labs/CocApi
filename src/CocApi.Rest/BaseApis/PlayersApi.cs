// <auto-generated>
/*
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Rest.IApis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public interface IPlayersApi : IApi
    {
        /// <summary>
        /// Get player information
        /// </summary>
        /// <remarks>
        /// Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;Player?&gt;&gt;</returns>
        Task<ApiResponse<Player?>> FetchPlayerResponseAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null);

        /// <summary>
        /// Get player information
        /// </summary>
        /// <remarks>
        /// Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse&lt;Player&gt;</returns>
        Task<Player> FetchPlayerAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null);

        /// <summary>
        /// Get player information
        /// </summary>
        /// <remarks>
        /// Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </remarks>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse&lt;Player?&gt;</returns>
        Task<Player?> FetchPlayerOrDefaultAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null);

        /// <summary>
        /// Verify player API token that can be found from the game settings.
        /// </summary>
        /// <remarks>
        /// Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;VerifyTokenResponse?&gt;&gt;</returns>
        Task<ApiResponse<VerifyTokenResponse?>> VerifyTokenResponseAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null);

        /// <summary>
        /// Verify player API token that can be found from the game settings.
        /// </summary>
        /// <remarks>
        /// Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse&lt;VerifyTokenResponse&gt;</returns>
        Task<VerifyTokenResponse> VerifyTokenAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null);

        /// <summary>
        /// Verify player API token that can be found from the game settings.
        /// </summary>
        /// <remarks>
        /// Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </remarks>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse&lt;VerifyTokenResponse?&gt;</returns>
        Task<VerifyTokenResponse?> VerifyTokenOrDefaultAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null);
    }
}

namespace CocApi.Rest.BaseApis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class PlayersApi : IApis.IPlayersApi
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger<PlayersApi> Logger { get; }

        /// <summary>
        /// The HttpClient
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// A token provider of type <see cref="ApiKeyProvider"/>
        /// </summary>
        public TokenProvider<ApiKeyToken> ApiKeyProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class.
        /// </summary>
        /// <returns></returns>
        public PlayersApi(ILogger<PlayersApi> logger, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider, 
            TokenProvider<ApiKeyToken> apiKeyProvider)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            Logger = logger;
            HttpClient = httpClient;
            ApiKeyProvider = apiKeyProvider;
        }

        /// <summary>
        /// Logs the api response
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnApiResponded(ApiResponseEventArgs args)
        {
            Logger.LogInformation("{0,-9} | {1} | {3}", (args.ReceivedAt - args.RequestedAt).TotalSeconds, args.HttpStatus, args.Path);
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="Player"/>&gt;</returns>
        public async Task<Player> FetchPlayerAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            ApiResponse<Player?> result = await FetchPlayerResponseAsync(playerTag, cancellationToken).ConfigureAwait(false);

            if (result.Content == null)
                throw new ApiException(result.ReasonPhrase, result.StatusCode, result.RawContent);

            return result.Content;
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="Player"/>&gt;</returns>
        public async Task<Player?> FetchPlayerOrDefaultAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            ApiResponse<Player?>? result = null;
            try 
            {
                result = await FetchPlayerResponseAsync(playerTag, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

            return result != null && result.IsSuccessStatusCode
                ? result.Content
                : null;
        }

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="playerTag"></param>
        /// <returns></returns>
        protected virtual string OnFetchPlayer(string playerTag)
        {
            #pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            #pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (playerTag == null)
                throw new ArgumentNullException(nameof(playerTag));

            #pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            #pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return playerTag;
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponse"></param>
        /// <param name="playerTag"></param>
        protected virtual void AfterFetchPlayer(ApiResponse<Player?> apiResponse, string playerTag)
        {
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="playerTag"></param>
        protected virtual void OnErrorFetchPlayer(Exception exception, string pathFormat, string path, string playerTag)
        {
            Logger.LogError(exception, "An error occured while sending the request to the server.");
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="Player"/></returns>
        public async Task<ApiResponse<Player?>> FetchPlayerResponseAsync(string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            UriBuilder uriBuilder = new UriBuilder();

            try
            {
                playerTag = OnFetchPlayer(playerTag);

                using (HttpRequestMessage request = new HttpRequestMessage())
                {
                    uriBuilder.Host = HttpClient.BaseAddress!.Host;
                    uriBuilder.Scheme = ClientUtils.SCHEME;
                    uriBuilder.Path = ClientUtils.CONTEXT_PATH + "/players/{playerTag}";

                    uriBuilder.Path = uriBuilder.Path.Replace("%7BplayerTag%7D", Uri.EscapeDataString(playerTag.ToString()));                    List<TokenBase> tokens = new List<TokenBase>();

                    ApiKeyToken apiKey = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokens.Add(apiKey);

                    apiKey.UseInHeader(request, "authorization");

                    request.RequestUri = uriBuilder.Uri;

                    string[] accepts = new string[] { 
                        "application/json" 
                    };

                    string? accept = ClientUtils.SelectHeaderAccept(accepts);

                    if (accept != null)
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));

                    request.Method = HttpMethod.Get;

                    DateTime requestedAt = DateTime.UtcNow;

                    using (HttpResponseMessage responseMessage = await HttpClient.SendAsync(request, cancellationToken.GetValueOrDefault()).ConfigureAwait(false))
                    {
                        OnApiResponded(new ApiResponseEventArgs(requestedAt, DateTime.UtcNow, responseMessage.StatusCode, "/players/{playerTag}", uriBuilder.Path));

                        string responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                        ApiResponse<Player?> apiResponse = new ApiResponse<Player?>(responseMessage, responseContent);

                        if (apiResponse.IsSuccessStatusCode)
                        {
                            apiResponse.Content = JsonSerializer.Deserialize<Player>(apiResponse.RawContent, _jsonSerializerOptions);
                            AfterFetchPlayer(apiResponse, playerTag);
                        }
                        else if (apiResponse.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase token in tokens)
                                token.BeginRateLimit();

                        return apiResponse;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorFetchPlayer(e, "/players/{playerTag}", uriBuilder.Path, playerTag);
                throw;
            }
        }

        /// <summary>
        /// Verify player API token that can be found from the game settings. Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="VerifyTokenResponse"/>&gt;</returns>
        public async Task<VerifyTokenResponse> VerifyTokenAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            ApiResponse<VerifyTokenResponse?> result = await VerifyTokenResponseAsync(body, playerTag, cancellationToken).ConfigureAwait(false);

            if (result.Content == null)
                throw new ApiException(result.ReasonPhrase, result.StatusCode, result.RawContent);

            return result.Content;
        }

        /// <summary>
        /// Verify player API token that can be found from the game settings. Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="VerifyTokenResponse"/>&gt;</returns>
        public async Task<VerifyTokenResponse?> VerifyTokenOrDefaultAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            ApiResponse<VerifyTokenResponse?>? result = null;
            try 
            {
                result = await VerifyTokenResponseAsync(body, playerTag, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

            return result != null && result.IsSuccessStatusCode
                ? result.Content
                : null;
        }

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        /// <returns></returns>
        protected virtual (VerifyTokenRequest, string) OnVerifyToken(VerifyTokenRequest body, string playerTag)
        {
            #pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            #pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (playerTag == null)
                throw new ArgumentNullException(nameof(playerTag));

            #pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            #pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return (body, playerTag);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponse"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        protected virtual void AfterVerifyToken(ApiResponse<VerifyTokenResponse?> apiResponse, VerifyTokenRequest body, string playerTag)
        {
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        protected virtual void OnErrorVerifyToken(Exception exception, string pathFormat, string path, VerifyTokenRequest body, string playerTag)
        {
            Logger.LogError(exception, "An error occured while sending the request to the server.");
        }

        /// <summary>
        /// Verify player API token that can be found from the game settings. Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="VerifyTokenResponse"/></returns>
        public async Task<ApiResponse<VerifyTokenResponse?>> VerifyTokenResponseAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken? cancellationToken = null)
        {
            UriBuilder uriBuilder = new UriBuilder();

            try
            {
                var validatedParameters = OnVerifyToken(body, playerTag);
                body = validatedParameters.Item1;
                playerTag = validatedParameters.Item2;

                using (HttpRequestMessage request = new HttpRequestMessage())
                {
                    uriBuilder.Host = HttpClient.BaseAddress!.Host;
                    uriBuilder.Scheme = ClientUtils.SCHEME;
                    uriBuilder.Path = ClientUtils.CONTEXT_PATH + "/players/{playerTag}/verifytoken";

                    uriBuilder.Path = uriBuilder.Path.Replace("%7BplayerTag%7D", Uri.EscapeDataString(playerTag.ToString()));                    request.Content = (body as object) is System.IO.Stream stream
                        ? request.Content = new StreamContent(stream)
                        : request.Content = new StringContent(JsonSerializer.Serialize(body, _jsonSerializerOptions));

                    List<TokenBase> tokens = new List<TokenBase>();

                    ApiKeyToken apiKey = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokens.Add(apiKey);

                    apiKey.UseInHeader(request, "authorization");

                    request.RequestUri = uriBuilder.Uri;

                    string[] contentTypes = new string[] {
                        "application/json" 
                    };

                    string? contentType = ClientUtils.SelectHeaderContentType(contentTypes);

                    if (contentType != null)
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    string[] accepts = new string[] { 
                        "application/json" 
                    };

                    string? accept = ClientUtils.SelectHeaderAccept(accepts);

                    if (accept != null)
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));

                    request.Method = HttpMethod.Post;

                    DateTime requestedAt = DateTime.UtcNow;

                    using (HttpResponseMessage responseMessage = await HttpClient.SendAsync(request, cancellationToken.GetValueOrDefault()).ConfigureAwait(false))
                    {
                        OnApiResponded(new ApiResponseEventArgs(requestedAt, DateTime.UtcNow, responseMessage.StatusCode, "/players/{playerTag}/verifytoken", uriBuilder.Path));

                        string responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                        ApiResponse<VerifyTokenResponse?> apiResponse = new ApiResponse<VerifyTokenResponse?>(responseMessage, responseContent);

                        if (apiResponse.IsSuccessStatusCode)
                        {
                            apiResponse.Content = JsonSerializer.Deserialize<VerifyTokenResponse>(apiResponse.RawContent, _jsonSerializerOptions);
                            AfterVerifyToken(apiResponse, body, playerTag);
                        }
                        else if (apiResponse.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase token in tokens)
                                token.BeginRateLimit();

                        return apiResponse;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorVerifyToken(e, "/players/{playerTag}/verifytoken", uriBuilder.Path, body, playerTag);
                throw;
            }
        }
    }
}
