# CocApi.Rest.BaseApis.LabelsApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetClanLabels**](LabelsApi.md#getclanlabels) | **GET** /labels/clans | List clan labels |
| [**GetPlayerLabels**](LabelsApi.md#getplayerlabels) | **GET** /labels/players | List player labels |

<a id="getclanlabels"></a>
# **GetClanLabels**
> LabelsObject GetClanLabels (int? limit = null, string? after = null, string? before = null)

List clan labels

List clan labels

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetClanLabelsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LabelsApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List clan labels
                LabelsObject result = apiInstance.GetClanLabels(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LabelsApi.GetClanLabels: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetClanLabelsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List clan labels
    ApiResponse<LabelsObject> response = apiInstance.GetClanLabelsWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LabelsApi.GetClanLabelsWithHttpInfo: " + e.Message);
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

[**LabelsObject**](LabelsObject.md)

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

<a id="getplayerlabels"></a>
# **GetPlayerLabels**
> LabelsObject GetPlayerLabels (int? limit = null, string? after = null, string? before = null)

List player labels

List player labels

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetPlayerLabelsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new LabelsApi(config);
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = "after_example";  // string? | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = "before_example";  // string? | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List player labels
                LabelsObject result = apiInstance.GetPlayerLabels(limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling LabelsApi.GetPlayerLabels: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetPlayerLabelsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // List player labels
    ApiResponse<LabelsObject> response = apiInstance.GetPlayerLabelsWithHttpInfo(limit, after, before);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling LabelsApi.GetPlayerLabelsWithHttpInfo: " + e.Message);
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

[**LabelsObject**](LabelsObject.md)

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

