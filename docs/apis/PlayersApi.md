# CocApi.Rest.BaseApis.PlayersApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetPlayer**](PlayersApi.md#getplayer) | **GET** /players/{playerTag} | Get player information |
| [**VerifyToken**](PlayersApi.md#verifytoken) | **POST** /players/{playerTag}/verifytoken | Verify player API token that can be found from the game settings. |

<a id="getplayer"></a>
# **GetPlayer**
> Player GetPlayer (string playerTag)

Get player information

Get information about a single player by player tag. Player tags can be found either in game or by from clan member lists. Note that player tags start with hash character '#' and that needs to be URL-encoded properly to work in URL, so for example player tag '#2ABC' would become '%232ABC' in the URL. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class GetPlayerExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new PlayersApi(config);
            var playerTag = "playerTag_example";  // string | Tag of the player.

            try
            {
                // Get player information
                Player result = apiInstance.GetPlayer(playerTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PlayersApi.GetPlayer: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the GetPlayerWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Get player information
    ApiResponse<Player> response = apiInstance.GetPlayerWithHttpInfo(playerTag);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PlayersApi.GetPlayerWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **playerTag** | **string** | Tag of the player. |  |

### Return type

[**Player**](Player.md)

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

<a id="verifytoken"></a>
# **VerifyToken**
> VerifyTokenResponse VerifyToken (VerifyTokenRequest body, string playerTag)

Verify player API token that can be found from the game settings.

Verify player API token that can be found from the game settings. This API call can be used to check that players own the game accounts they claim to own as they need to provide the one-time use API token that exists inside the game. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class VerifyTokenExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new PlayersApi(config);
            var body = new VerifyTokenRequest(); // VerifyTokenRequest | Request body
            var playerTag = "playerTag_example";  // string | Tag of the player.

            try
            {
                // Verify player API token that can be found from the game settings.
                VerifyTokenResponse result = apiInstance.VerifyToken(body, playerTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PlayersApi.VerifyToken: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the VerifyTokenWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Verify player API token that can be found from the game settings.
    ApiResponse<VerifyTokenResponse> response = apiInstance.VerifyTokenWithHttpInfo(body, playerTag);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PlayersApi.VerifyTokenWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | [**VerifyTokenRequest**](VerifyTokenRequest.md) | Request body |  |
| **playerTag** | **string** | Tag of the player. |  |

### Return type

[**VerifyTokenResponse**](VerifyTokenResponse.md)

### Authorization

[JWT](../README.md#JWT)

### HTTP request headers

 - **Content-Type**: application/json
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

