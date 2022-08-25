# CocApi.Rest.BaseApis.DeveloperApi

All URIs are relative to *https://api.clashofclans.com/v1*

Method | HTTP request | Description
------------- | ------------- | -------------
[**Create**](DeveloperApi.md#create) | **POST** /apikey/create | Create an api token.
[**Keys**](DeveloperApi.md#keys) | **POST** /apikey/list | List all tokens.
[**Login**](DeveloperApi.md#login) | **POST** /api/login | Login to the developer portal.
[**Revoke**](DeveloperApi.md#revoke) | **POST** /apikey/revoke | Revoke an api token.


<a name="create"></a>
# **Create**
> KeyInstance Create (CreateTokenRequest createTokenRequest)

Create an api token.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class CreateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: cookieAuth
            config.AddApiKey("session", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("session", "Bearer");

            var apiInstance = new DeveloperApi(config);
            var createTokenRequest = new CreateTokenRequest(); // CreateTokenRequest | Request body

            try
            {
                // Create an api token.
                KeyInstance result = apiInstance.Create(createTokenRequest);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DeveloperApi.Create: " + e.Message );
                Debug.Print("Status Code: "+ e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **createTokenRequest** | [**CreateTokenRequest**](CreateTokenRequest.md)| Request body | 

### Return type

[**KeyInstance**](KeyInstance.md)

### Authorization

[cookieAuth](../README.md#cookieAuth)

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

<a name="keys"></a>
# **Keys**
> KeyList Keys ()

List all tokens.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class KeysExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: cookieAuth
            config.AddApiKey("session", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("session", "Bearer");

            var apiInstance = new DeveloperApi(config);

            try
            {
                // List all tokens.
                KeyList result = apiInstance.Keys();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DeveloperApi.Keys: " + e.Message );
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

[**KeyList**](KeyList.md)

### Authorization

[cookieAuth](../README.md#cookieAuth)

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

<a name="login"></a>
# **Login**
> LoginResponse Login (LoginCredentials loginCredentials)

Login to the developer portal.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class LoginExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            var apiInstance = new DeveloperApi(config);
            var loginCredentials = new LoginCredentials(); // LoginCredentials | Request body

            try
            {
                // Login to the developer portal.
                LoginResponse result = apiInstance.Login(loginCredentials);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DeveloperApi.Login: " + e.Message );
                Debug.Print("Status Code: "+ e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **loginCredentials** | [**LoginCredentials**](LoginCredentials.md)| Request body | 

### Return type

[**LoginResponse**](LoginResponse.md)

### Authorization

No authorization required

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

<a name="revoke"></a>
# **Revoke**
> KeyInstance Revoke (Key key)

Revoke an api token.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Rest.BaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace Example
{
    public class RevokeExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: cookieAuth
            config.AddApiKey("session", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("session", "Bearer");

            var apiInstance = new DeveloperApi(config);
            var key = new Key(); // Key | Request body

            try
            {
                // Revoke an api token.
                KeyInstance result = apiInstance.Revoke(key);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DeveloperApi.Revoke: " + e.Message );
                Debug.Print("Status Code: "+ e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | [**Key**](Key.md)| Request body | 

### Return type

[**KeyInstance**](KeyInstance.md)

### Authorization

[cookieAuth](../README.md#cookieAuth)

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

