# CocApi.Rest.BaseApis.GoldpassApi

All URIs are relative to *https://api.clashofclans.com/v1*

Method | HTTP request | Description
------------- | ------------- | -------------
[**GetCurrentGoldPassSeason**](GoldpassApi.md#getcurrentgoldpassseason) | **GET** /goldpass/seasons/current | Get information about the current gold pass season.


<a name="getcurrentgoldpassseason"></a>
# **GetCurrentGoldPassSeason**
> GoldPassSeason GetCurrentGoldPassSeason ()

Get information about the current gold pass season.

Get information about the current gold pass season.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetCurrentGoldPassSeasonExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new GoldpassApi(config);

            try
            {
                // Get information about the current gold pass season.
                GoldPassSeason result = apiInstance.GetCurrentGoldPassSeason();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling GoldpassApi.GetCurrentGoldPassSeason: " + e.Message );
                Debug.Print("Status Code: "+ e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**GoldPassSeason**](GoldPassSeason.md)

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

