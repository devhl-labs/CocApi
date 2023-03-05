# CocApi.Rest.BaseApis.LocationsApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetClanRanking**](LocationsApi.md#getclanranking) | **GET** /locations/{locationId}/rankings/clans | Get clan rankings for a specific location |
| [**GetClanVersusRanking**](LocationsApi.md#getclanversusranking) | **GET** /locations/{locationId}/rankings/clans-versus | Get clan versus rankings for a specific location |
| [**GetLocation**](LocationsApi.md#getlocation) | **GET** /locations/{locationId} | Get location information |
| [**GetLocations**](LocationsApi.md#getlocations) | **GET** /locations | List locations |
| [**GetPlayerRanking**](LocationsApi.md#getplayerranking) | **GET** /locations/{locationId}/rankings/players | Get player rankings for a specific location |
| [**GetPlayerVersusRanking**](LocationsApi.md#getplayerversusranking) | **GET** /locations/{locationId}/rankings/players-versus | Get player versus rankings for a specific location |

<a name="getclanranking"></a>
# **GetClanRanking**
> ClanRankingList GetClanRanking (string locationId, int? limit = null, string? after = null, string? before = null)

Get clan rankings for a specific location

Get clan rankings for a specific location

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetClanRankingExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var locationId = "locationId_example";  // string | Identifier of the location to retrieve.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get clan rankings for a specific location
                ClanRankingList result = apiInstance.GetClanRanking(locationId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetClanRanking: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetClanRankingWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get clan rankings for a specific location
    ApiResponse<ClanRankingList> response = apiInstance.GetClanRankingWithHttpInfo(locationId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetClanRankingWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **string** | Identifier of the location to retrieve. |  |
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**ClanRankingList**](ClanRankingList.md)

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

<a name="getclanversusranking"></a>
# **GetClanVersusRanking**
> ClanVersusRankingList GetClanVersusRanking (string locationId, int? limit = null, string? after = null, string? before = null)

Get clan versus rankings for a specific location

Get clan versus rankings for a specific location

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetClanVersusRankingExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var locationId = "locationId_example";  // string | Identifier of the location to retrieve.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get clan versus rankings for a specific location
                ClanVersusRankingList result = apiInstance.GetClanVersusRanking(locationId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetClanVersusRanking: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetClanVersusRankingWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get clan versus rankings for a specific location
    ApiResponse<ClanVersusRankingList> response = apiInstance.GetClanVersusRankingWithHttpInfo(locationId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetClanVersusRankingWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **string** | Identifier of the location to retrieve. |  |
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**ClanVersusRankingList**](ClanVersusRankingList.md)

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

<a name="getlocation"></a>
# **GetLocation**
> Location GetLocation (string locationId)

Get location information

Get information about specific location

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLocationExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var locationId = "locationId_example";  // string | Identifier of the location to retrieve.

            try
            {
                // Get location information
                Location result = apiInstance.GetLocation(locationId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetLocation: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLocationWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get location information
    ApiResponse<Location> response = apiInstance.GetLocationWithHttpInfo(locationId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetLocationWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **string** | Identifier of the location to retrieve. |  |

### Return type

[**Location**](Location.md)

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

<a name="getlocations"></a>
# **GetLocations**
> LocationList GetLocations (int? limit = null, string? after = null, string? before = null)

List locations

List locations

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetLocationsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List locations
                LocationList result = apiInstance.GetLocations(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetLocations: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetLocationsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List locations
    ApiResponse<LocationList> response = apiInstance.GetLocationsWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetLocationsWithHttpInfo: " + e.Message);
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

[**LocationList**](LocationList.md)

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

<a name="getplayerranking"></a>
# **GetPlayerRanking**
> PlayerRankingList GetPlayerRanking (string locationId, int? limit = null, string? after = null, string? before = null)

Get player rankings for a specific location

Get player rankings for a specific location

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetPlayerRankingExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var locationId = "locationId_example";  // string | Identifier of the location to retrieve.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get player rankings for a specific location
                PlayerRankingList result = apiInstance.GetPlayerRanking(locationId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetPlayerRanking: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetPlayerRankingWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get player rankings for a specific location
    ApiResponse<PlayerRankingList> response = apiInstance.GetPlayerRankingWithHttpInfo(locationId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetPlayerRankingWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **string** | Identifier of the location to retrieve. |  |
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

<a name="getplayerversusranking"></a>
# **GetPlayerVersusRanking**
> PlayerVersusRankingList GetPlayerVersusRanking (string locationId, int? limit = null, string? after = null, string? before = null)

Get player versus rankings for a specific location

Get player versus rankings for a specific location

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetPlayerVersusRankingExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LocationsApi(config);
            var locationId = "locationId_example";  // string | Identifier of the location to retrieve.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Get player versus rankings for a specific location
                PlayerVersusRankingList result = apiInstance.GetPlayerVersusRanking(locationId, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LocationsApi.GetPlayerVersusRanking: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetPlayerVersusRankingWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get player versus rankings for a specific location
    ApiResponse<PlayerVersusRankingList> response = apiInstance.GetPlayerVersusRankingWithHttpInfo(locationId, limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LocationsApi.GetPlayerVersusRankingWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **string** | Identifier of the location to retrieve. |  |
| **limit** | **int?** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string?** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string?** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**PlayerVersusRankingList**](PlayerVersusRankingList.md)

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

