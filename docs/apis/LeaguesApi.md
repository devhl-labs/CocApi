# CocApi.Rest.BaseApis.LeaguesApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetCapitalLeague**](LeaguesApi.md#getcapitalleague) | **GET** /capitalleagues/{leagueId} | Get capital league information |
| [**GetCapitalLeagues**](LeaguesApi.md#getcapitalleagues) | **GET** /capitalleagues | List capital leagues |
| [**GetLeague**](LeaguesApi.md#getleague) | **GET** /leagues/{leagueId} | Get league information |
| [**GetLeagueSeasonRankings**](LeaguesApi.md#getleagueseasonrankings) | **GET** /leagues/{leagueId}/seasons/{seasonId} | Get league season rankings |
| [**GetLeagueSeasons**](LeaguesApi.md#getleagueseasons) | **GET** /leagues/{leagueId}/seasons | Get league seasons |
| [**GetLeagues**](LeaguesApi.md#getleagues) | **GET** /leagues | List leagues |
| [**GetWarLeague**](LeaguesApi.md#getwarleague) | **GET** /warleagues/{leagueId} | Get war league information |
| [**GetWarLeagues**](LeaguesApi.md#getwarleagues) | **GET** /warleagues | List war leagues |

<a id="getcapitalleague"></a>
# **GetCapitalLeague**
> CapitalLeague GetCapitalLeague (string leagueId)

Get capital league information

Get capital league information

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetCapitalLeagueExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var leagueId = "leagueId_example";  // string | Identifier of the league.

            try
            {
                // Get capital league information
                CapitalLeague result = apiInstance.GetCapitalLeague(leagueId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetCapitalLeague: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetCapitalLeagueWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get capital league information
    ApiResponse<CapitalLeague> response = apiInstance.GetCapitalLeagueWithHttpInfo(leagueId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetCapitalLeagueWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **leagueId** | **string** | Identifier of the league. |  |

### Return type

[**CapitalLeague**](CapitalLeague.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |
| **200** | Successful response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcapitalleagues"></a>
# **GetCapitalLeagues**
> CapitalLeagueObject GetCapitalLeagues (int? limit = null, string? after = null, string? before = null)

List capital leagues

List capital leagues

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetCapitalLeaguesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List capital leagues
                CapitalLeagueObject result = apiInstance.GetCapitalLeagues(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetCapitalLeagues: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetCapitalLeaguesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List capital leagues
    ApiResponse<CapitalLeagueObject> response = apiInstance.GetCapitalLeaguesWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetCapitalLeaguesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**CapitalLeagueObject**](CapitalLeagueObject.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |
| **200** | Successful response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getleague"></a>
# **GetLeague**
> League GetLeague (string leagueId)

Get league information

Get league information

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLeagueExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var leagueId = "leagueId_example";  // string | Identifier of the league.

            try
            {
                // Get league information
                League result = apiInstance.GetLeague(leagueId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetLeague: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLeagueWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get league information
    ApiResponse<League> response = apiInstance.GetLeagueWithHttpInfo(leagueId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetLeagueWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **leagueId** | **string** | Identifier of the league. |  |

### Return type

[**League**](League.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getleagueseasonrankings"></a>
# **GetLeagueSeasonRankings**
> PlayerRankingList GetLeagueSeasonRankings (string leagueId, string seasonId, int? limit = null, string? after = null, string? before = null)

Get league season rankings

Get league season rankings. Note that league season information is available only for Legend League. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLeagueSeasonRankingsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var leagueId = "leagueId_example";  // string | Identifier of the league.
            var seasonId = "seasonId_example";  // string | Identifier of the season.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get league season rankings
                PlayerRankingList result = apiInstance.GetLeagueSeasonRankings(leagueId, seasonId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetLeagueSeasonRankings: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLeagueSeasonRankingsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get league season rankings
    ApiResponse<PlayerRankingList> response = apiInstance.GetLeagueSeasonRankingsWithHttpInfo(leagueId, seasonId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetLeagueSeasonRankingsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **leagueId** | **string** | Identifier of the league. |  |
| **seasonId** | **string** | Identifier of the season. |  |
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**PlayerRankingList**](PlayerRankingList.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getleagueseasons"></a>
# **GetLeagueSeasons**
> LeagueSeasonList GetLeagueSeasons (string leagueId, int? limit = null, string? after = null, string? before = null)

Get league seasons

Get league seasons. Note that league season information is available only for Legend League. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLeagueSeasonsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var leagueId = "leagueId_example";  // string | Identifier of the league.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get league seasons
                LeagueSeasonList result = apiInstance.GetLeagueSeasons(leagueId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetLeagueSeasons: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLeagueSeasonsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get league seasons
    ApiResponse<LeagueSeasonList> response = apiInstance.GetLeagueSeasonsWithHttpInfo(leagueId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetLeagueSeasonsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **leagueId** | **string** | Identifier of the league. |  |
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**LeagueSeasonList**](LeagueSeasonList.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getleagues"></a>
# **GetLeagues**
> LeagueList GetLeagues (int? limit = null, string? after = null, string? before = null)

List leagues

List leagues

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLeaguesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List leagues
                LeagueList result = apiInstance.GetLeagues(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetLeagues: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLeaguesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List leagues
    ApiResponse<LeagueList> response = apiInstance.GetLeaguesWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetLeaguesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**LeagueList**](LeagueList.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getwarleague"></a>
# **GetWarLeague**
> WarLeague GetWarLeague (string leagueId)

Get war league information

Get war league information

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetWarLeagueExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var leagueId = "leagueId_example";  // string | Identifier of the league.

            try
            {
                // Get war league information
                WarLeague result = apiInstance.GetWarLeague(leagueId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetWarLeague: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetWarLeagueWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get war league information
    ApiResponse<WarLeague> response = apiInstance.GetWarLeagueWithHttpInfo(leagueId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetWarLeagueWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **leagueId** | **string** | Identifier of the league. |  |

### Return type

[**WarLeague**](WarLeague.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getwarleagues"></a>
# **GetWarLeagues**
> WarLeagueList GetWarLeagues (int? limit = null, string? after = null, string? before = null)

List war leagues

List war leagues

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetWarLeaguesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LeaguesApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List war leagues
                WarLeagueList result = apiInstance.GetWarLeagues(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LeaguesApi.GetWarLeagues: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetWarLeaguesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List war leagues
    ApiResponse<WarLeagueList> response = apiInstance.GetWarLeaguesWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LeaguesApi.GetWarLeaguesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**WarLeagueList**](WarLeagueList.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful response |  -  |
| **400** | Client provided incorrect parameters for the request. |  -  |
| **403** | Access denied, either because of missing/incorrect credentials or used API token does not grant access to the requested resource.  |  -  |
| **404** | Resource was not found. |  -  |
| **429** | Request was throttled, because amount of requests was above the threshold defined for the used API token.  |  -  |
| **500** | Unknown error happened when handling the request. |  -  |
| **503** | Service is temprorarily unavailable because of maintenance. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

