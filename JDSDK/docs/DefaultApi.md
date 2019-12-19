# IO.Swagger.Api.DefaultApi

All URIs are relative to *https://way.jd.com/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**GetCity**](DefaultApi.md#getcity) | **GET** /JDCloud/getCity | 
[**GetCountry**](DefaultApi.md#getcountry) | **GET** /JDCloud/getCountry | 
[**GetProvince**](DefaultApi.md#getprovince) | **GET** /JDCloud/getProvince | 获取省级列表
[**GetTown**](DefaultApi.md#gettown) | **GET** /JDCloud/getTown | 


<a name="getcity"></a>
# **GetCity**
> string GetCity (string parentId, string appkey)





### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetCityExample
    {
        public void main()
        {
            
            var apiInstance = new DefaultApi();
            var parentId = parentId_example;  // string | 省份id (default to 1)
            var appkey = appkey_example;  // string | 万象平台提供的appkey

            try
            {
                // 
                string result = apiInstance.GetCity(parentId, appkey);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DefaultApi.GetCity: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **parentId** | **string**| 省份id | [default to 1]
 **appkey** | **string**| 万象平台提供的appkey | 

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: text/plain
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getcountry"></a>
# **GetCountry**
> string GetCountry (string parentId, string appkey)





### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetCountryExample
    {
        public void main()
        {
            
            var apiInstance = new DefaultApi();
            var parentId = parentId_example;  // string | 市级id (default to 78)
            var appkey = appkey_example;  // string | 万象平台提供的appkey

            try
            {
                // 
                string result = apiInstance.GetCountry(parentId, appkey);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DefaultApi.GetCountry: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **parentId** | **string**| 市级id | [default to 78]
 **appkey** | **string**| 万象平台提供的appkey | 

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: text/plain
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getprovince"></a>
# **GetProvince**
> string GetProvince (string appkey)

获取省级列表

获取省级列表

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetProvinceExample
    {
        public void main()
        {
            
            var apiInstance = new DefaultApi();
            var appkey = appkey_example;  // string | 万象平台提供的appkey

            try
            {
                // 获取省级列表
                string result = apiInstance.GetProvince(appkey);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DefaultApi.GetProvince: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **appkey** | **string**| 万象平台提供的appkey | 

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: text/plain
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="gettown"></a>
# **GetTown**
> string GetTown (string parentId, string appkey)





### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetTownExample
    {
        public void main()
        {
            
            var apiInstance = new DefaultApi();
            var parentId = parentId_example;  // string | 区县地址id (default to 72)
            var appkey = appkey_example;  // string | 万象平台提供的appkey

            try
            {
                // 
                string result = apiInstance.GetTown(parentId, appkey);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DefaultApi.GetTown: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **parentId** | **string**| 区县地址id | [default to 72]
 **appkey** | **string**| 万象平台提供的appkey | 

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: text/plain
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

