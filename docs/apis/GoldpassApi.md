# CocApi.Rest.Apis.GoldpassApi

All URIs are relative to *https://api.clashofclans.com/v1*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetCurrentGoldPassSeason**](GoldpassApi.md#getcurrentgoldpassseason) | **GET** /goldpass/seasons/current | Get information about the current gold pass season. |

<a id="getcurrentgoldpassseason"></a>
# **GetCurrentGoldPassSeason**
> GoldPassSeason GetCurrentGoldPassSeason ()

Get information about the current gold pass season.

Get information about the current gold pass season.


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

