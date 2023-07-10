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
    public interface IDeveloperApi : IApi
    {
        /// <summary>
        /// The class containing the events
        /// </summary>
        DeveloperApiEvents Events { get; }

        /// <summary>
        /// Create an api token.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="createTokenRequest">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;KeyInstance&gt;&gt;</returns>
        Task<ApiResponse<KeyInstance>> CreateAsync(CreateTokenRequest createTokenRequest, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Create an api token.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="createTokenRequest">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;KeyInstance&gt;?&gt;</returns>
        Task<ApiResponse<KeyInstance>?> CreateOrDefaultAsync(CreateTokenRequest createTokenRequest, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// List all tokens.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;KeyList&gt;&gt;</returns>
        Task<ApiResponse<KeyList>> KeysAsync(System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// List all tokens.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;KeyList&gt;?&gt;</returns>
        Task<ApiResponse<KeyList>?> KeysOrDefaultAsync(System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Login to the developer portal.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCredentials">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;LoginResponse&gt;&gt;</returns>
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginCredentials loginCredentials, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Login to the developer portal.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="loginCredentials">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;LoginResponse&gt;?&gt;</returns>
        Task<ApiResponse<LoginResponse>?> LoginOrDefaultAsync(LoginCredentials loginCredentials, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoke an api token.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="key">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&lt;KeyInstance&gt;&gt;</returns>
        Task<ApiResponse<KeyInstance>> RevokeAsync(Key key, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoke an api token.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="key">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task&lt;ApiResponse&gt;KeyInstance&gt;?&gt;</returns>
        Task<ApiResponse<KeyInstance>?> RevokeOrDefaultAsync(Key key, System.Threading.CancellationToken cancellationToken = default);
    }
}

namespace CocApi.Rest.Apis
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public class DeveloperApiEvents
    {
        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<KeyInstance>>? OnCreate;

        internal void ExecuteOnCreate(ApiResponse<KeyInstance> apiResponse)
        {
            OnCreate?.Invoke(this, new ApiResponseEventArgs<KeyInstance>(apiResponse));
        }

        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<KeyList>>? OnKeys;

        internal void ExecuteOnKeys(ApiResponse<KeyList> apiResponse)
        {
            OnKeys?.Invoke(this, new ApiResponseEventArgs<KeyList>(apiResponse));
        }

        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<LoginResponse>>? OnLogin;

        internal void ExecuteOnLogin(ApiResponse<LoginResponse> apiResponse)
        {
            OnLogin?.Invoke(this, new ApiResponseEventArgs<LoginResponse>(apiResponse));
        }

        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs<KeyInstance>>? OnRevoke;

        internal void ExecuteOnRevoke(ApiResponse<KeyInstance> apiResponse)
        {
            OnRevoke?.Invoke(this, new ApiResponseEventArgs<KeyInstance>(apiResponse));
        }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class DeveloperApi : IApis.IDeveloperApi
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger<DeveloperApi> Logger { get; }

        /// <summary>
        /// The HttpClient
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The class containing the events
        /// </summary>
        public DeveloperApiEvents Events { get; }

        /// <summary>
        /// A token provider of type <see cref="ApiKeyProvider"/>
        /// </summary>
        public TokenProvider<ApiKeyToken> ApiKeyProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperApi"/> class.
        /// </summary>
        /// <returns></returns>
        public DeveloperApi(ILogger<DeveloperApi> logger, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider, DeveloperApiEvents developerApiEvents,
            TokenProvider<ApiKeyToken> apiKeyProvider)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            Logger = logger;
            HttpClient = httpClient;
            Events = developerApiEvents;
            ApiKeyProvider = apiKeyProvider;
        }

        partial void FormatCreate(CreateTokenRequest createTokenRequest);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="createTokenRequest"></param>
        /// <returns></returns>
        private void ValidateCreate(CreateTokenRequest createTokenRequest)
        {
            if (createTokenRequest == null)
                throw new ArgumentNullException(nameof(createTokenRequest));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="createTokenRequest"></param>
        private void AfterCreateDefaultImplementation(ApiResponse<KeyInstance> apiResponseLocalVar, CreateTokenRequest createTokenRequest)
        {
            bool suppressDefaultLog = false;
            AfterCreate(ref suppressDefaultLog, apiResponseLocalVar, createTokenRequest);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="createTokenRequest"></param>
        partial void AfterCreate(ref bool suppressDefaultLog, ApiResponse<KeyInstance> apiResponseLocalVar, CreateTokenRequest createTokenRequest);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="createTokenRequest"></param>
        private void OnErrorCreateDefaultImplementation(Exception exception, string pathFormat, string path, CreateTokenRequest createTokenRequest)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorCreate(exception, pathFormat, path, createTokenRequest);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="createTokenRequest"></param>
        partial void OnErrorCreate(Exception exception, string pathFormat, string path, CreateTokenRequest createTokenRequest);

        /// <summary>
        /// Create an api token. 
        /// </summary>
        /// <param name="createTokenRequest">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyInstance"/></returns>
        public async Task<ApiResponse<KeyInstance>?> CreateOrDefaultAsync(CreateTokenRequest createTokenRequest, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await CreateAsync(createTokenRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create an api token. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="createTokenRequest">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyInstance"/></returns>
        public async Task<ApiResponse<KeyInstance>> CreateAsync(CreateTokenRequest createTokenRequest, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateCreate(createTokenRequest);

                FormatCreate(createTokenRequest);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    Uri urlLocalVar = httpRequestMessageLocalVar.RequestUri = new Uri("https://developer.clashofclans.com/api/apikey/create");
                    uriBuilderLocalVar.Host = urlLocalVar.Authority;
                    uriBuilderLocalVar.Scheme = urlLocalVar.Scheme;
                    uriBuilderLocalVar.Path = urlLocalVar.AbsolutePath;

                    httpRequestMessageLocalVar.Content = (createTokenRequest as object) is System.IO.Stream stream
                        ? httpRequestMessageLocalVar.Content = new StreamContent(stream)
                        : httpRequestMessageLocalVar.Content = new StringContent(JsonSerializer.Serialize(createTokenRequest, _jsonSerializerOptions));

                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKeyTokenLocalVar);
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

                        ApiResponse<KeyInstance> apiResponseLocalVar = new ApiResponse<KeyInstance>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/apikey/create", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterCreateDefaultImplementation(apiResponseLocalVar, createTokenRequest);

                        Events.ExecuteOnCreate(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorCreateDefaultImplementation(e, "/apikey/create", uriBuilderLocalVar.Path, createTokenRequest);
                throw;
            }
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        private void AfterKeysDefaultImplementation(ApiResponse<KeyList> apiResponseLocalVar)
        {
            bool suppressDefaultLog = false;
            AfterKeys(ref suppressDefaultLog, apiResponseLocalVar);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        partial void AfterKeys(ref bool suppressDefaultLog, ApiResponse<KeyList> apiResponseLocalVar);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        private void OnErrorKeysDefaultImplementation(Exception exception, string pathFormat, string path)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorKeys(exception, pathFormat, path);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        partial void OnErrorKeys(Exception exception, string pathFormat, string path);

        /// <summary>
        /// List all tokens. 
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyList"/></returns>
        public async Task<ApiResponse<KeyList>?> KeysOrDefaultAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await KeysAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// List all tokens. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyList"/></returns>
        public async Task<ApiResponse<KeyList>> KeysAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    Uri urlLocalVar = httpRequestMessageLocalVar.RequestUri = new Uri("https://developer.clashofclans.com/api/apikey/list");
                    uriBuilderLocalVar.Host = urlLocalVar.Authority;
                    uriBuilderLocalVar.Scheme = urlLocalVar.Scheme;
                    uriBuilderLocalVar.Path = urlLocalVar.AbsolutePath;

                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKeyTokenLocalVar);
                    httpRequestMessageLocalVar.RequestUri = uriBuilderLocalVar.Uri;

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

                        ApiResponse<KeyList> apiResponseLocalVar = new ApiResponse<KeyList>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/apikey/list", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterKeysDefaultImplementation(apiResponseLocalVar);

                        Events.ExecuteOnKeys(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorKeysDefaultImplementation(e, "/apikey/list", uriBuilderLocalVar.Path);
                throw;
            }
        }

        partial void FormatLogin(LoginCredentials loginCredentials);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="loginCredentials"></param>
        /// <returns></returns>
        private void ValidateLogin(LoginCredentials loginCredentials)
        {
            if (loginCredentials == null)
                throw new ArgumentNullException(nameof(loginCredentials));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="loginCredentials"></param>
        private void AfterLoginDefaultImplementation(ApiResponse<LoginResponse> apiResponseLocalVar, LoginCredentials loginCredentials)
        {
            bool suppressDefaultLog = false;
            AfterLogin(ref suppressDefaultLog, apiResponseLocalVar, loginCredentials);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="loginCredentials"></param>
        partial void AfterLogin(ref bool suppressDefaultLog, ApiResponse<LoginResponse> apiResponseLocalVar, LoginCredentials loginCredentials);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="loginCredentials"></param>
        private void OnErrorLoginDefaultImplementation(Exception exception, string pathFormat, string path, LoginCredentials loginCredentials)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorLogin(exception, pathFormat, path, loginCredentials);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="loginCredentials"></param>
        partial void OnErrorLogin(Exception exception, string pathFormat, string path, LoginCredentials loginCredentials);

        /// <summary>
        /// Login to the developer portal. 
        /// </summary>
        /// <param name="loginCredentials">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LoginResponse"/></returns>
        public async Task<ApiResponse<LoginResponse>?> LoginOrDefaultAsync(LoginCredentials loginCredentials, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await LoginAsync(loginCredentials, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Login to the developer portal. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCredentials">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="LoginResponse"/></returns>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginCredentials loginCredentials, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateLogin(loginCredentials);

                FormatLogin(loginCredentials);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    Uri urlLocalVar = httpRequestMessageLocalVar.RequestUri = new Uri("https://developer.clashofclans.com/api/login");
                    uriBuilderLocalVar.Host = urlLocalVar.Authority;
                    uriBuilderLocalVar.Scheme = urlLocalVar.Scheme;
                    uriBuilderLocalVar.Path = urlLocalVar.AbsolutePath;

                    httpRequestMessageLocalVar.Content = (loginCredentials as object) is System.IO.Stream stream
                        ? httpRequestMessageLocalVar.Content = new StreamContent(stream)
                        : httpRequestMessageLocalVar.Content = new StringContent(JsonSerializer.Serialize(loginCredentials, _jsonSerializerOptions));

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

                        ApiResponse<LoginResponse> apiResponseLocalVar = new ApiResponse<LoginResponse>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/api/login", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterLoginDefaultImplementation(apiResponseLocalVar, loginCredentials);

                        Events.ExecuteOnLogin(apiResponseLocalVar);

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorLoginDefaultImplementation(e, "/api/login", uriBuilderLocalVar.Path, loginCredentials);
                throw;
            }
        }

        partial void FormatRevoke(Key key);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private void ValidateRevoke(Key key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="key"></param>
        private void AfterRevokeDefaultImplementation(ApiResponse<KeyInstance> apiResponseLocalVar, Key key)
        {
            bool suppressDefaultLog = false;
            AfterRevoke(ref suppressDefaultLog, apiResponseLocalVar, key);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="key"></param>
        partial void AfterRevoke(ref bool suppressDefaultLog, ApiResponse<KeyInstance> apiResponseLocalVar, Key key);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        private void OnErrorRevokeDefaultImplementation(Exception exception, string pathFormat, string path, Key key)
        {
            Logger.LogError(exception, "An error occurred while sending the request to the server.");
            OnErrorRevoke(exception, pathFormat, path, key);
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        partial void OnErrorRevoke(Exception exception, string pathFormat, string path, Key key);

        /// <summary>
        /// Revoke an api token. 
        /// </summary>
        /// <param name="key">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyInstance"/></returns>
        public async Task<ApiResponse<KeyInstance>?> RevokeOrDefaultAsync(Key key, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await RevokeAsync(key, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Revoke an api token. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="key">Request body</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="ApiResponse{T}"/>&gt; where T : <see cref="KeyInstance"/></returns>
        public async Task<ApiResponse<KeyInstance>> RevokeAsync(Key key, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateRevoke(key);

                FormatRevoke(key);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    Uri urlLocalVar = httpRequestMessageLocalVar.RequestUri = new Uri("https://developer.clashofclans.com/api/apikey/revoke");
                    uriBuilderLocalVar.Host = urlLocalVar.Authority;
                    uriBuilderLocalVar.Scheme = urlLocalVar.Scheme;
                    uriBuilderLocalVar.Path = urlLocalVar.AbsolutePath;

                    httpRequestMessageLocalVar.Content = (key as object) is System.IO.Stream stream
                        ? httpRequestMessageLocalVar.Content = new StreamContent(stream)
                        : httpRequestMessageLocalVar.Content = new StringContent(JsonSerializer.Serialize(key, _jsonSerializerOptions));

                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKeyTokenLocalVar);
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

                        ApiResponse<KeyInstance> apiResponseLocalVar = new ApiResponse<KeyInstance>(httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/apikey/revoke", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterRevokeDefaultImplementation(apiResponseLocalVar, key);

                        Events.ExecuteOnRevoke(apiResponseLocalVar);

                        if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorRevokeDefaultImplementation(e, "/apikey/revoke", uriBuilderLocalVar.Path, key);
                throw;
            }
        }
    }
}
