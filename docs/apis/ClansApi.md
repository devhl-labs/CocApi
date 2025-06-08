# CocApi.Rest.Apis.ClansApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetCapitalRaidSeasons**](ClansApi.md#getcapitalraidseasons) | **GET** /clans/{clanTag}/capitalraidseasons | Retrieve clan&#39;s capital raid seasons |
| [**GetClan**](ClansApi.md#getclan) | **GET** /clans/{clanTag} | Get clan information |
| [**GetClanMembers**](ClansApi.md#getclanmembers) | **GET** /clans/{clanTag}/members | List clan members |
| [**GetClanWarLeagueGroup**](ClansApi.md#getclanwarleaguegroup) | **GET** /clans/{clanTag}/currentwar/leaguegroup | Retrieve information about clan&#39;s current clan war league group |
| [**GetClanWarLeagueWar**](ClansApi.md#getclanwarleaguewar) | **GET** /clanwarleagues/wars/{warTag} | Retrieve information about individual clan war league war |
| [**GetClanWarLog**](ClansApi.md#getclanwarlog) | **GET** /clans/{clanTag}/warlog | Retrieve clan&#39;s clan war log |
| [**GetCurrentWar**](ClansApi.md#getcurrentwar) | **GET** /clans/{clanTag}/currentwar | Retrieve information about clan&#39;s current clan war |
| [**SearchClans**](ClansApi.md#searchclans) | **GET** /clans | Search clans |

<a id="getcapitalraidseasons"></a>
# **GetCapitalRaidSeasons**
> ClanCapitalRaidSeasons GetCapitalRaidSeasons (string clanTag, int limit = null, string after = null, string before = null)

Retrieve clan's capital raid seasons

Retrieve clan's capital raid seasons


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |
| **limit** | **int** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

### Return type

[**ClanCapitalRaidSeasons**](ClanCapitalRaidSeasons.md)

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

<a id="getclan"></a>
# **GetClan**
> Clan GetClan (string clanTag)

Get clan information

Get information about a single clan by clan tag. Clan tags can be found using clan search operation. Note that clan tags start with hash character '#' and that needs to be URL-encoded properly to work in URL, so for example clan tag '#2ABC' would become '%232ABC' in the URL. 


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getclanmembers"></a>
# **GetClanMembers**
> List&lt;ClanMember&gt; GetClanMembers (string clanTag, int limit = null, string after = null, string before = null)

List clan members

List clan members.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |
| **limit** | **int** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getclanwarleaguegroup"></a>
# **GetClanWarLeagueGroup**
> ClanWarLeagueGroup GetClanWarLeagueGroup (string clanTag, bool realtime = null)

Retrieve information about clan's current clan war league group

Retrieve information about clan's current clan war league group


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |
| **realtime** | **bool** | Used to bypass cache. Only SuperCell approved users may use this option. | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getclanwarleaguewar"></a>
# **GetClanWarLeagueWar**
> ClanWar GetClanWarLeagueWar (string warTag, bool realtime = null)

Retrieve information about individual clan war league war

Retrieve information about individual clan war league war


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **warTag** | **string** | Tag of the war. |  |
| **realtime** | **bool** | Used to bypass cache. Only SuperCell approved users may use this option. | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getclanwarlog"></a>
# **GetClanWarLog**
> ClanWarLog GetClanWarLog (string clanTag, int limit = null, string after = null, string before = null)

Retrieve clan's clan war log

Retrieve clan's clan war log


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |
| **limit** | **int** | Limit the number of items returned in the response. | [optional]  |
| **after** | **string** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcurrentwar"></a>
# **GetCurrentWar**
> ClanWar GetCurrentWar (string clanTag, bool realtime = null)

Retrieve information about clan's current clan war

Retrieve information about clan's current clan war


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clanTag** | **string** | Tag of the clan. |  |
| **realtime** | **bool** | Used to bypass cache. Only SuperCell approved users may use this option. | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="searchclans"></a>
# **SearchClans**
> ClanList SearchClans (int locationId = null, int minMembers = null, int maxMembers = null, int minClanPoints = null, int minClanLevel = null, int limit = null, string name = null, string warFrequency = null, string after = null, string before = null, string labelIds = null)

Search clans

Search all clans by name and/or filtering the results using various criteria. At least one filtering criteria must be defined and if name is used as part of search, it is required to be at least three characters long. It is not possible to specify ordering for results so clients should not rely on any specific ordering as that may change in the future releases of the API. 


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **locationId** | **int** | Filter by clan location identifier. For list of available locations, refer to getLocations operation.  | [optional]  |
| **minMembers** | **int** | Filter by minimum number of clan members | [optional]  |
| **maxMembers** | **int** | Filter by maximum number of clan members | [optional]  |
| **minClanPoints** | **int** | Filter by minimum amount of clan points. | [optional]  |
| **minClanLevel** | **int** | Filter by minimum clan level. | [optional]  |
| **limit** | **int** | Limit the number of items returned in the response. | [optional]  |
| **name** | **string** | Search clans by name. If name is used as part of search query, it needs to be at least three characters long. Name search parameter is interpreted as wild card search, so it may appear anywhere in the clan name.  | [optional]  |
| **warFrequency** | **string** | Filter by clan war frequency | [optional]  |
| **after** | **string** | Return only items that occur after this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **before** | **string** | Return only items that occur before this marker. Before marker can be found from the response, inside the &#39;paging&#39; property. Note that only after or before can be specified for a request, not both.  | [optional]  |
| **labelIds** | **string** | Comma separatered list of label IDs to use for filtering results. | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

