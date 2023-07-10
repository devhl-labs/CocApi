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
using CocApi.Rest.Apis;
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
        /// The class containing the events
        /// </summary>
        PlayersApiEvents Events { get; }

        /// <summary>
        /// Get player information
        /// </summary>
        /// <remarks>
        /// Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;Player&gt;&gt;</returns>
        Task<ApiResponse<Player>> FetchPlayerAsync(string playerTag, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Get player information
        /// </summary>
        /// <remarks>
        /// Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </remarks>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;Player&gt;?&gt;</returns>
        Task<ApiResponse<Player>?> FetchPlayerOrDefaultAsync(string playerTag, System.Threading.CancellationToken cancellationToken = default);

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
        /// <returns>Task&lt;ApiResponse&lt;VerifyTokenResponse&gt;&gt;</returns>
        Task<ApiResponse<VerifyTokenResponse>> VerifyTokenAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify player API token that can be found from the game settings.
        /// </summary>
        /// <remarks>
        /// Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </remarks>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;VerifyTokenResponse&gt;?&gt;</returns>
        Task<ApiResponse<VerifyTokenResponse>?> VerifyTokenOrDefaultAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken cancellationToken = default);
    }
}

namespace CocApi.Rest.Apis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public class PlayersApiEvents
    {
        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<Player>>? OnFetchPlayer;

        internal void ExecuteOnGetPlayer(ApiResponse<Player> apiResponse)
        {
            OnFetchPlayer?.Invoke(this, new ApiResponseEventArgs<Player>(apiResponse));
        }

        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<VerifyTokenResponse>>? OnVerifyToken;

        internal void ExecuteOnVerifyToken(ApiResponse<VerifyTokenResponse> apiResponse)
        {
            OnVerifyToken?.Invoke(this, new ApiResponseEventArgs<VerifyTokenResponse>(apiResponse));
        }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class PlayersApi : IApis.IPlayersApi
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
        /// The class containing the events
        /// </summary>
        public PlayersApiEvents Events { get; }

        /// <summary>
        /// A token provider of type <see cref="ApiKeyProvider"/>
        /// </summary>
        public TokenProvider<ApiKeyToken> ApiKeyProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersApi"/> class.
        /// </summary>
        /// <returns></returns>
        public PlayersApi(ILogger<PlayersApi> logger, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider, PlayersApiEvents playersApiEvents,
            TokenProvider<ApiKeyToken> apiKeyProvider)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            Logger = logger;
            HttpClient = httpClient;
            Events = playersApiEvents;
            ApiKeyProvider = apiKeyProvider;
        }

        partial void FormatGetPlayer(ref string playerTag);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="playerTag"></param>
        /// <returns></returns>
        private void ValidateGetPlayer(string playerTag)
        {
            if (playerTag == null)
                throw new ArgumentNullException(nameof(playerTag));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="playerTag"></param>
        private void AfterFetchPlayerDefaultImplementation(ApiResponse<Player> apiResponseLocalVar, string playerTag)
        {
            bool suppressDefaultLog = false;
            AfterFetchPlayer(ref suppressDefaultLog, apiResponseLocalVar, playerTag);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="playerTag"></param>
        partial void AfterFetchPlayer(ref bool suppressDefaultLog, ApiResponse<Player> apiResponseLocalVar, string playerTag);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="playerTag"></param>
        private void OnErrorFetchPlayerDefaultImplementation(Exception exception, string pathFormat, string path, string playerTag)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorFetchPlayer(exception, pathFormat, path, playerTag);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="playerTag"></param>
        partial void OnErrorFetchPlayer(Exception exception, string pathFormat, string path, string playerTag);

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="Player"/></returns>
        public async Task<ApiResponse<Player>?> FetchPlayerOrDefaultAsync(string playerTag, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await FetchPlayerAsync(playerTag, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get player information Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character &#39;#&#39; and that needs to be URL-encoded properly to work in URL, so for example player tag &#39;#2ABC&#39; would become &#39;%232ABC&#39; in the URL. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="Player"/></returns>
        public async Task<ApiResponse<Player>> FetchPlayerAsync(string playerTag, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateGetPlayer(playerTag);

                FormatGetPlayer(ref playerTag);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/players/{playerTag}";
                    uriBuilderLocalVar.Path = uriBuilderLocalVar.Path.Replace("%7BplayerTag%7D", Uri.EscapeDataString(playerTag.ToString()));

                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKeyTokenLocalVar);

                    apiKeyTokenLocalVar.UseInHeader(httpRequestMessageLocalVar, "authorization");

                    httpRequestMessageLocalVar.RequestUri = uriBuilderLocalVar.Uri;

                    string[] acceptLocalVars = new string[] {
                        "application/json"
                    };

                    string? acceptLocalVar = ClientUtils.SelectHeaderAccept(acceptLocalVars);

                    if (acceptLocalVar != null)
                        httpRequestMessageLocalVar.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptLocalVar));

                    httpRequestMessageLocalVar.Method = HttpMethod.Get;

                    DateTime requestedAtLocalVar = DateTime.UtcNow;

                    using (HttpResponseMessage httpResponseMessageLocalVar = await HttpClient.SendAsync(httpRequestMessageLocalVar, cancellationToken).ConfigureAwait(false))
                    {
                        string responseContentLocalVar = await httpResponseMessageLocalVar.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        ApiResponse<Player> apiResponseLocalVar = new ApiResponse<Player>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/players/{playerTag}", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterFetchPlayerDefaultImplementation(apiResponseLocalVar, playerTag);

                        Events.ExecuteOnGetPlayer(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorFetchPlayerDefaultImplementation(e, "/players/{playerTag}", uriBuilderLocalVar.Path, playerTag);
                throw;
            }
        }

        partial void FormatVerifyToken(VerifyTokenRequest body, ref string playerTag);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        /// <returns></returns>
        private void ValidateVerifyToken(VerifyTokenRequest body, string playerTag)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (playerTag == null)
                throw new ArgumentNullException(nameof(playerTag));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        private void AfterVerifyTokenDefaultImplementation(ApiResponse<VerifyTokenResponse> apiResponseLocalVar, VerifyTokenRequest body, string playerTag)
        {
            bool suppressDefaultLog = false;
            AfterVerifyToken(ref suppressDefaultLog, apiResponseLocalVar, body, playerTag);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        partial void AfterVerifyToken(ref bool suppressDefaultLog, ApiResponse<VerifyTokenResponse> apiResponseLocalVar, VerifyTokenRequest body, string playerTag);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        private void OnErrorVerifyTokenDefaultImplementation(Exception exception, string pathFormat, string path, VerifyTokenRequest body, string playerTag)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorVerifyToken(exception, pathFormat, path, body, playerTag);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="playerTag"></param>
        partial void OnErrorVerifyToken(Exception exception, string pathFormat, string path, VerifyTokenRequest body, string playerTag);

        /// <summary>
        /// Verify player API token that can be found from the game settings. Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </summary>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="VerifyTokenResponse"/></returns>
        public async Task<ApiResponse<VerifyTokenResponse>?> VerifyTokenOrDefaultAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await VerifyTokenAsync(body, playerTag, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Verify player API token that can be found from the game settings. Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">Request body</param>
        /// <param name="playerTag">Tag of the player.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="VerifyTokenResponse"/></returns>
        public async Task<ApiResponse<VerifyTokenResponse>> VerifyTokenAsync(VerifyTokenRequest body, string playerTag, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateVerifyToken(body, playerTag);

                FormatVerifyToken(body, ref playerTag);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/players/{playerTag}/verifytoken";
                    uriBuilderLocalVar.Path = uriBuilderLocalVar.Path.Replace("%7BplayerTag%7D", Uri.EscapeDataString(playerTag.ToString()));

                    httpRequestMessageLocalVar.Content = (body as object) is System.IO.Stream stream
                        ? httpRequestMessageLocalVar.Content = new StreamContent(stream)
                        : httpRequestMessageLocalVar.Content = new StringContent(JsonSerializer.Serialize(body, _jsonSerializerOptions));

                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKeyTokenLocalVar);

                    apiKeyTokenLocalVar.UseInHeader(httpRequestMessageLocalVar, "authorization");

                    httpRequestMessageLocalVar.RequestUri = uriBuilderLocalVar.Uri;

                    string[] contentTypes = new string[] {
                        "application/json"
                    };

                    string? contentTypeLocalVar = ClientUtils.SelectHeaderContentType(contentTypes);

                    if (contentTypeLocalVar != null && httpRequestMessageLocalVar.Content != null)
                        httpRequestMessageLocalVar.Content.Headers.ContentType = new MediaTypeHeaderValue(contentTypeLocalVar);

                    string[] acceptLocalVars = new string[] {
                        "application/json"
                    };

                    string? acceptLocalVar = ClientUtils.SelectHeaderAccept(acceptLocalVars);

                    if (acceptLocalVar != null)
                        httpRequestMessageLocalVar.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptLocalVar));

                    httpRequestMessageLocalVar.Method = HttpMethod.Post;

                    DateTime requestedAtLocalVar = DateTime.UtcNow;

                    using (HttpResponseMessage httpResponseMessageLocalVar = await HttpClient.SendAsync(httpRequestMessageLocalVar, cancellationToken).ConfigureAwait(false))
                    {
                        string responseContentLocalVar = await httpResponseMessageLocalVar.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        ApiResponse<VerifyTokenResponse> apiResponseLocalVar = new ApiResponse<VerifyTokenResponse>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/players/{playerTag}/verifytoken", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterVerifyTokenDefaultImplementation(apiResponseLocalVar, body, playerTag);

                        Events.ExecuteOnVerifyToken(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorVerifyTokenDefaultImplementation(e, "/players/{playerTag}/verifytoken", uriBuilderLocalVar.Path, body, playerTag);
                throw;
            }
        }
    }
}
