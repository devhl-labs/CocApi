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
    public interface IGoldpassApi : IApi
    {
        /// <summary>
        /// Get information about the current gold pass season.
        /// </summary>
        /// <remarks>
        /// Get information about the current gold pass season.
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;GoldPassSeason&gt;&gt;</returns>
        Task<ApiResponse<GoldPassSeason>> FetchCurrentGoldPassSeasonAsync(System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Get information about the current gold pass season.
        /// </summary>
        /// <remarks>
        /// Get information about the current gold pass season.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;GoldPassSeason&gt;?&gt;</returns>
        Task<ApiResponse<GoldPassSeason>?> FetchCurrentGoldPassSeasonOrDefaultAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}

namespace CocApi.Rest.Apis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class GoldpassApi : IApis.IGoldpassApi
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger<GoldpassApi> Logger { get; }

        /// <summary>
        /// The HttpClient
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// A token provider of type <see cref="ApiKeyProvider"/>
        /// </summary>
        public TokenProvider<ApiKeyToken> ApiKeyProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoldpassApi"/> class.
        /// </summary>
        /// <returns></returns>
        public GoldpassApi(ILogger<GoldpassApi> logger, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider,
            TokenProvider<ApiKeyToken> apiKeyProvider)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            Logger = logger;
            HttpClient = httpClient;
            ApiKeyProvider = apiKeyProvider;
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        private void AfterFetchCurrentGoldPassSeasonDefaultImplementation(ApiResponse<GoldPassSeason> apiResponseLocalVar)
        {
            bool suppressDefaultLog = false;
            AfterFetchCurrentGoldPassSeason(ref suppressDefaultLog, apiResponseLocalVar);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        partial void AfterFetchCurrentGoldPassSeason(ref bool suppressDefaultLog, ApiResponse<GoldPassSeason> apiResponseLocalVar);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        private void OnErrorFetchCurrentGoldPassSeasonDefaultImplementation(Exception exception, string pathFormat, string path)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorFetchCurrentGoldPassSeason(exception, pathFormat, path);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        partial void OnErrorFetchCurrentGoldPassSeason(Exception exception, string pathFormat, string path);

        /// <summary>
        /// Get information about the current gold pass season. Get information about the current gold pass season.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="GoldPassSeason"/></returns>
        public async Task<ApiResponse<GoldPassSeason>?> FetchCurrentGoldPassSeasonOrDefaultAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await FetchCurrentGoldPassSeasonAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get information about the current gold pass season. Get information about the current gold pass season.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="GoldPassSeason"/></returns>
        public async Task<ApiResponse<GoldPassSeason>> FetchCurrentGoldPassSeasonAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/goldpass/seasons/current";

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

                        ApiResponse<GoldPassSeason> apiResponseLocalVar = new ApiResponse<GoldPassSeason>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/goldpass/seasons/current", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterFetchCurrentGoldPassSeasonDefaultImplementation(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorFetchCurrentGoldPassSeasonDefaultImplementation(e, "/goldpass/seasons/current", uriBuilderLocalVar.Path);
                throw;
            }
        }
    }
}
