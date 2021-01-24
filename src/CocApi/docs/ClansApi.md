# CocApi.Api.ClansApi

All URIs are relative to *https://api.clashofclans.com/v1*

Method | HTTP request | Description
------------- | ------------- | -------------
[**GetClan**](ClansApi.md#getclan) | **GET** /clans/{clanTag} | Get clan information
[**GetClanMembers**](ClansApi.md#getclanmembers) | **GET** /clans/{clanTag}/members | List clan members
[**GetClanWarLeagueGroup**](ClansApi.md#getclanwarleaguegroup) | **GET** /clans/{clanTag}/currentwar/leaguegroup | Retrieve information about clan&#39;s current clan war league group
[**GetClanWarLeagueWar**](ClansApi.md#getclanwarleaguewar) | **GET** /clanwarleagues/wars/{warTag} | Retrieve information about individual clan war league war
[**GetClanWarLog**](ClansApi.md#getclanwarlog) | **GET** /clans/{clanTag}/warlog | Retrieve clan&#39;s clan war log
[**GetCurrentWar**](ClansApi.md#getcurrentwar) | **GET** /clans/{clanTag}/currentwar | Retrieve information about clan&#39;s current clan war
[**SearchClans**](ClansApi.md#searchclans) | **GET** /clans | Search clans


<a name="getclan"></a>
# **GetClan**
> Clan GetClan (string clanTag)

Get clan information

Get information about a single clan by clan tag. Clan tags can be found using clan search operation. Note that clan tags start with hash character '#' and that needs to be URL-encoded properly to work in URL, so for example clan tag '#2ABC' would become '%232ABC' in the URL. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetClanExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var clanTag = clanTag_example;  // string | Tag of the clan.

            try
            {
                // Get clan information
                Clan result = apiInstance.GetClan(clanTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetClan: " + e.Message );
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
 **clanTag** | **string**| Tag of the clan. | 

### Return type

[**Clan**](Clan.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getclanmembers"></a>
# **GetClanMembers**
> List&lt;ClanMember&gt; GetClanMembers (string clanTag, int? limit = null, string after = null, string before = null)

List clan members

List clan members.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetClanMembersExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var clanTag = clanTag_example;  // string | Tag of the clan.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = after_example;  // string | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = before_example;  // string | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // List clan members
                List<ClanMember> result = apiInstance.GetClanMembers(clanTag, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetClanMembers: " + e.Message );
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
 **clanTag** | **string**| Tag of the clan. | 
 **limit** | **int?**| Limit the number of items returned in the response. | [optional] 
 **after** | **string**| Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 
 **before** | **string**| Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 

### Return type

[**List&lt;ClanMember&gt;**](ClanMember.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getclanwarleaguegroup"></a>
# **GetClanWarLeagueGroup**
> ClanWarLeagueGroup GetClanWarLeagueGroup (string clanTag)

Retrieve information about clan's current clan war league group

Retrieve information about clan's current clan war league group

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetClanWarLeagueGroupExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var clanTag = clanTag_example;  // string | Tag of the clan.

            try
            {
                // Retrieve information about clan's current clan war league group
                ClanWarLeagueGroup result = apiInstance.GetClanWarLeagueGroup(clanTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetClanWarLeagueGroup: " + e.Message );
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
 **clanTag** | **string**| Tag of the clan. | 

### Return type

[**ClanWarLeagueGroup**](ClanWarLeagueGroup.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getclanwarleaguewar"></a>
# **GetClanWarLeagueWar**
> ClanWar GetClanWarLeagueWar (string warTag)

Retrieve information about individual clan war league war

Retrieve information about individual clan war league war

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetClanWarLeagueWarExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var warTag = warTag_example;  // string | Tag of the war.

            try
            {
                // Retrieve information about individual clan war league war
                ClanWar result = apiInstance.GetClanWarLeagueWar(warTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetClanWarLeagueWar: " + e.Message );
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
 **warTag** | **string**| Tag of the war. | 

### Return type

[**ClanWar**](ClanWar.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getclanwarlog"></a>
# **GetClanWarLog**
> ClanWarLog GetClanWarLog (string clanTag, int? limit = null, string after = null, string before = null)

Retrieve clan's clan war log

Retrieve clan's clan war log

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetClanWarLogExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var clanTag = clanTag_example;  // string | Tag of the clan.
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = after_example;  // string | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = before_example;  // string | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 

            try
            {
                // Retrieve clan's clan war log
                ClanWarLog result = apiInstance.GetClanWarLog(clanTag, limit, after, before);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetClanWarLog: " + e.Message );
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
 **clanTag** | **string**| Tag of the clan. | 
 **limit** | **int?**| Limit the number of items returned in the response. | [optional] 
 **after** | **string**| Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 
 **before** | **string**| Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 

### Return type

[**ClanWarLog**](ClanWarLog.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getcurrentwar"></a>
# **GetCurrentWar**
> ClanWar GetCurrentWar (string clanTag)

Retrieve information about clan's current clan war

Retrieve information about clan's current clan war

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class GetCurrentWarExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var clanTag = clanTag_example;  // string | Tag of the clan.

            try
            {
                // Retrieve information about clan's current clan war
                ClanWar result = apiInstance.GetCurrentWar(clanTag);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.GetCurrentWar: " + e.Message );
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
 **clanTag** | **string**| Tag of the clan. | 

### Return type

[**ClanWar**](ClanWar.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="searchclans"></a>
# **SearchClans**
> ClanList SearchClans (string name = null, string warFrequency = null, int? locationId = null, int? minMembers = null, int? maxMembers = null, int? minClanPoints = null, int? minClanLevel = null, int? limit = null, string after = null, string before = null, string labelIds = null)

Search clans

Search all clans by name and/or filtering the results using various criteria. At least one filtering criteria must be defined and if name is used as part of search, it is required to be at least three characters long. It is not possible to specify ordering for results so clients should not rely on any specific ordering as that may change in the future releases of the API. 

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace Example
{
    public class SearchClansExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.clashofclans.com/v1";
            // Configure API key authorization: JWT
            config.AddApiKey("authorization", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("authorization", "Bearer");

            var apiInstance = new ClansApi(config);
            var name = name_example;  // string | Search clans by name. If name is used as part of search query, it needs to be at least three characters long. Name search parameter is interpreted as wild card search, so it may appear anywhere in the clan name.  (optional) 
            var warFrequency = warFrequency_example;  // string | Filter by clan war frequency (optional) 
            var locationId = 56;  // int? | Filter by clan location identifier. For list of available locations, refer to getLocations operation.  (optional) 
            var minMembers = 56;  // int? | Filter by minimum number of clan members (optional) 
            var maxMembers = 56;  // int? | Filter by maximum number of clan members (optional) 
            var minClanPoints = 56;  // int? | Filter by minimum amount of clan points. (optional) 
            var minClanLevel = 56;  // int? | Filter by minimum clan level. (optional) 
            var limit = 56;  // int? | Limit the number of items returned in the response. (optional) 
            var after = after_example;  // string | Return only items that occur after this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var before = before_example;  // string | Return only items that occur before this marker. Before marker can be found from the response, inside the 'paging' property. Note that only after or before can be specified for a request, not both.  (optional) 
            var labelIds = labelIds_example;  // string | Comma separatered list of label IDs to use for filtering results. (optional) 

            try
            {
                // Search clans
                ClanList result = apiInstance.SearchClans(name, warFrequency, locationId, minMembers, maxMembers, minClanPoints, minClanLevel, limit, after, before, labelIds);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ClansApi.SearchClans: " + e.Message );
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
 **name** | **string**| Search clans by name. If name is used as part of search query, it needs to be at least three characters long. Name search parameter is interpreted as wild card search, so it may appear anywhere in the clan name.  | [optional] 
 **warFrequency** | **string**| Filter by clan war frequency | [optional] 
 **locationId** | **int?**| Filter by clan location identifier. For list of available locations, refer to getLocations operation.  | [optional] 
 **minMembers** | **int?**| Filter by minimum number of clan members | [optional] 
 **maxMembers** | **int?**| Filter by maximum number of clan members | [optional] 
 **minClanPoints** | **int?**| Filter by minimum amount of clan points. | [optional] 
 **minClanLevel** | **int?**| Filter by minimum clan level. | [optional] 
 **limit** | **int?**| Limit the number of items returned in the response. | [optional] 
 **after** | **string**| Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 
 **before** | **string**| Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional] 
 **labelIds** | **string**| Comma separatered list of label IDs to use for filtering results. | [optional] 

### Return type

[**ClanList**](ClanList.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


