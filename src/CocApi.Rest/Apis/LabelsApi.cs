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

namespace CocApi.Rest.Apis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public interface ILabelsApi : IApi
    {
        /// <summary>
        /// The class containing the events
        /// </summary>
        LabelsApiEvents Events { get; }

        /// <summary>
        /// List clan labels
        /// </summary>
        /// <remarks>
        /// List clan labels
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;LabelsObject&gt;&gt;</returns>
        Task<ApiResponse<LabelsObject>> FetchClanLabelsAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// List clan labels
        /// </summary>
        /// <remarks>
        /// List clan labels
        /// </remarks>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;LabelsObject&gt;?&gt;</returns>
        Task<ApiResponse<LabelsObject>?> FetchClanLabelsOrDefaultAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// List player labels
        /// </summary>
        /// <remarks>
        /// List player labels
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;LabelsObject&gt;&gt;</returns>
        Task<ApiResponse<LabelsObject>> FetchPlayerLabelsAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// List player labels
        /// </summary>
        /// <remarks>
        /// List player labels
        /// </remarks>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;LabelsObject&gt;?&gt;</returns>
        Task<ApiResponse<LabelsObject>?> FetchPlayerLabelsOrDefaultAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public class LabelsApiEvents
    {
        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<LabelsObject>>? OnFetchClanLabels;

        /// <summary>
        /// The event raised after an error querying the server
        /// </summary>
        public event EventHandler<ExceptionEventArgs>? OnErrorFetchClanLabels;

        internal void ExecuteOnGetClanLabels(ApiResponse<LabelsObject> apiResponse)
        {
            OnFetchClanLabels?.Invoke(this, new ApiResponseEventArgs<LabelsObject>(apiResponse));
        }

        internal void ExecuteOnErrorGetClanLabels(Exception exception)
        {
            OnErrorFetchClanLabels?.Invoke(this, new ExceptionEventArgs(exception));
        }

        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<LabelsObject>>? OnFetchPlayerLabels;

        /// <summary>
        /// The event raised after an error querying the server
        /// </summary>
        public event EventHandler<ExceptionEventArgs>? OnErrorFetchPlayerLabels;

        internal void ExecuteOnGetPlayerLabels(ApiResponse<LabelsObject> apiResponse)
        {
            OnFetchPlayerLabels?.Invoke(this, new ApiResponseEventArgs<LabelsObject>(apiResponse));
        }

        internal void ExecuteOnErrorGetPlayerLabels(Exception exception)
        {
            OnErrorFetchPlayerLabels?.Invoke(this, new ExceptionEventArgs(exception));
        }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class LabelsApi : ILabelsApi
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger<LabelsApi> Logger { get; }

        /// <summary>
        /// The HttpClient
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The class containing the events
        /// </summary>
        public LabelsApiEvents Events { get; }

        /// <summary>
        /// A token provider of type <see cref="ApiKeyProvider"/>
        /// </summary>
        public TokenProvider<ApiKeyToken> ApiKeyProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelsApi"/> class.
        /// </summary>
        /// <returns></returns>
        public LabelsApi(ILogger<LabelsApi> logger, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider, LabelsApiEvents labelsApiEvents,
            TokenProvider<ApiKeyToken> apiKeyProvider)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            Logger = logger;
            HttpClient = httpClient;
            Events = labelsApiEvents;
            ApiKeyProvider = apiKeyProvider;
        }

        partial void FormatGetClanLabels(ref Option<int> limit, ref Option<string> after, ref Option<string> before);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <returns></returns>
        private void ValidateGetClanLabels(Option<string> after, Option<string> before)
        {
            if (after.IsSet && after.Value == null)
                throw new ArgumentNullException(nameof(after));

            if (before.IsSet && before.Value == null)
                throw new ArgumentNullException(nameof(before));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        private void AfterFetchClanLabelsDefaultImplementation(ApiResponse<LabelsObject> apiResponseLocalVar, Option<int> limit, Option<string> after, Option<string> before)
        {
            bool suppressDefaultLog = false;
            AfterFetchClanLabels(ref suppressDefaultLog, apiResponseLocalVar, limit, after, before);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        partial void AfterFetchClanLabels(ref bool suppressDefaultLog, ApiResponse<LabelsObject> apiResponseLocalVar, Option<int> limit, Option<string> after, Option<string> before);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        private void OnErrorFetchClanLabelsDefaultImplementation(Exception exception, string pathFormat, string path, Option<int> limit, Option<string> after, Option<string> before)
        {
            bool suppressDefaultLog = false;
            OnErrorFetchClanLabels(ref suppressDefaultLog, exception, pathFormat, path, limit, after, before);
            if (!suppressDefaultLog)
                Logger.LogError(exception, "An error occurred while sending the request to the server.");
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        partial void OnErrorFetchClanLabels(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, Option<int> limit, Option<string> after, Option<string> before);

        /// <summary>
        /// List clan labels List clan labels
        /// </summary>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LabelsObject"/></returns>
        public async Task<ApiResponse<LabelsObject>?> FetchClanLabelsOrDefaultAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await FetchClanLabelsAsync(limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// List clan labels List clan labels
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LabelsObject"/></returns>
        public async Task<ApiResponse<LabelsObject>> FetchClanLabelsAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateGetClanLabels(after, before);

                FormatGetClanLabels(ref limit, ref after, ref before);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/labels/clans";

                    System.Collections.Specialized.NameValueCollection parseQueryStringLocalVar = System.Web.HttpUtility.ParseQueryString(string.Empty);

                    if (limit.IsSet)
                        parseQueryStringLocalVar["limit"] = limit.Value.ToString();

                    if (after.IsSet)
                        parseQueryStringLocalVar["after"] = after.Value.ToString();

                    if (before.IsSet)
                        parseQueryStringLocalVar["before"] = before.Value.ToString();

                    uriBuilderLocalVar.Query = parseQueryStringLocalVar.ToString();

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

                        ApiResponse<LabelsObject> apiResponseLocalVar = new ApiResponse<LabelsObject>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/labels/clans", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterFetchClanLabelsDefaultImplementation(apiResponseLocalVar, limit, after, before);

                        Events.ExecuteOnGetClanLabels(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorFetchClanLabelsDefaultImplementation(e, "/labels/clans", uriBuilderLocalVar.Path, limit, after, before);
                Events.ExecuteOnErrorGetClanLabels(e);
                throw;
            }
        }

        partial void FormatGetPlayerLabels(ref Option<int> limit, ref Option<string> after, ref Option<string> before);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <returns></returns>
        private void ValidateGetPlayerLabels(Option<string> after, Option<string> before)
        {
            if (after.IsSet && after.Value == null)
                throw new ArgumentNullException(nameof(after));

            if (before.IsSet && before.Value == null)
                throw new ArgumentNullException(nameof(before));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        private void AfterFetchPlayerLabelsDefaultImplementation(ApiResponse<LabelsObject> apiResponseLocalVar, Option<int> limit, Option<string> after, Option<string> before)
        {
            bool suppressDefaultLog = false;
            AfterFetchPlayerLabels(ref suppressDefaultLog, apiResponseLocalVar, limit, after, before);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        partial void AfterFetchPlayerLabels(ref bool suppressDefaultLog, ApiResponse<LabelsObject> apiResponseLocalVar, Option<int> limit, Option<string> after, Option<string> before);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        private void OnErrorFetchPlayerLabelsDefaultImplementation(Exception exception, string pathFormat, string path, Option<int> limit, Option<string> after, Option<string> before)
        {
            bool suppressDefaultLog = false;
            OnErrorFetchPlayerLabels(ref suppressDefaultLog, exception, pathFormat, path, limit, after, before);
            if (!suppressDefaultLog)
                Logger.LogError(exception, "An error occurred while sending the request to the server.");
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        partial void OnErrorFetchPlayerLabels(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, Option<int> limit, Option<string> after, Option<string> before);

        /// <summary>
        /// List player labels List player labels
        /// </summary>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LabelsObject"/></returns>
        public async Task<ApiResponse<LabelsObject>?> FetchPlayerLabelsOrDefaultAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await FetchPlayerLabelsAsync(limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// List player labels List player labels
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Limit the number of items returned in the response. (optional)</param>
        /// <param name="after">Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="before">Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LabelsObject"/></returns>
        public async Task<ApiResponse<LabelsObject>> FetchPlayerLabelsAsync(Option<int> limit = default, Option<string> after = default, Option<string> before = default, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateGetPlayerLabels(after, before);

                FormatGetPlayerLabels(ref limit, ref after, ref before);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/labels/players";

                    System.Collections.Specialized.NameValueCollection parseQueryStringLocalVar = System.Web.HttpUtility.ParseQueryString(string.Empty);

                    if (limit.IsSet)
                        parseQueryStringLocalVar["limit"] = limit.Value.ToString();

                    if (after.IsSet)
                        parseQueryStringLocalVar["after"] = after.Value.ToString();

                    if (before.IsSet)
                        parseQueryStringLocalVar["before"] = before.Value.ToString();

                    uriBuilderLocalVar.Query = parseQueryStringLocalVar.ToString();

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

                        ApiResponse<LabelsObject> apiResponseLocalVar = new ApiResponse<LabelsObject>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/labels/players", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterFetchPlayerLabelsDefaultImplementation(apiResponseLocalVar, limit, after, before);

                        Events.ExecuteOnGetPlayerLabels(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorFetchPlayerLabelsDefaultImplementation(e, "/labels/players", uriBuilderLocalVar.Path, limit, after, before);
                Events.ExecuteOnErrorGetPlayerLabels(e);
                throw;
            }
        }
    }
}
