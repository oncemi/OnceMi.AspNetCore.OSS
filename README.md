# OnceMi.AspNetCore.OSS
Asp.Net Core 5.0对象储存扩展包，支持Minio自建对象储存、阿里云OSS、腾讯云COS。

# How to use
1、安装OnceMi.AspNetCore.OSS。
CLI中安装：
```shell
dotnet add package OnceMi.AspNetCore.OSS
```
Nuget中安装：
在Nuget包管理器中搜索`OnceMi.AspNetCore.OSS`并安装。

2、Startup中注入服务

```csharp
//default minio
//添加默认对象储存配置信息
services.AddOSSService(option =>
{
    option.Provider = OSSProvider.Minio;
    option.Endpoint = "oss.oncemi.com:9000";
    option.AccessKey = "Q38Gh1ewNjs6UCA9";
    option.SecretKey = "AaTWMEB+tOlP3MuwNwWhhl1d6+qL4r8Q";
    option.IsEnableHttps = true;
    option.IsEnableCache = true;
});

//aliyun oss
//添加名称为‘aliyunoss’的OSS对象储存配置信息
services.AddOSSService("aliyunoss", option =>
 {
     option.Provider = OSSProvider.Aliyun;
     option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
     option.AccessKey = "LTAI4GDTV8J26jSXG12qftDU";
     option.SecretKey = "55zUTPZxPtrvo9WCryrSRfyRdPXhOT";
     option.IsEnableCache = true;
 });

//qcloud oss
//从配置文件中加载节点为‘OSSProvider’的配置信息
services.AddOSSService("QCloud", "OSSProvider");
```

可注入多个OSSService，不同的Service用名称来区分。需要注意的是，腾讯云COS中配置节点Endpoint表示AppId。

3、使用
```csharp
/// <summary>
/// 使用默认的配置文件
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IOSSService _OSSService;
    private readonly string _bucketName = "default-dev";

    public HomeController(ILogger<HomeController> logger
        , IOSSService OSSService)
    {
        _logger = logger;
        _OSSService = OSSService;
    }
}
```

```csharp
/// <summary>
/// 获取IOSSServiceFactory，更具名称创建对应的OSS服务
/// </summary>
public class QCloudController : Controller
{
    private readonly ILogger<QCloudController> _logger;
    private readonly IOSSService _OSSService;
    private readonly string _bucketName = "default-dev";

    public QCloudController(ILogger<QCloudController> logger
        , IOSSServiceFactory ossServiceFactory)
    {
        _logger = logger;
        _OSSService = ossServiceFactory.Create("QCloud");
    }
}
```
列出bucket中的全部文件
```csharp
public async Task<IActionResult> ListBuckets()
{
    try
    {
        var result = await _OSSService.ListBucketsAsync();
        return Json(result);
    }
    catch (Exception ex)
    {
        return Content(ex.Message);
    }
}
```

4、配置参数

|  名称 |  类型  | 说明  | 案例  |  备注 |
| :------------ |:------------ | :------------ | :------------ | :------------ |
| Provider  | 枚举  | OSS提供者  |  Minio |   |
| Endpoint  | string  | 节点  | oss-cn-hangzhou.aliyuncs.com  |  在腾讯云OSS中表示AppId  |
| AccessKey  | string  | AccessKey  | FJDGS24dsgDFSks  |    |
| SecretKey  | string  | SecretKey  | FJDGS24dsgDFSks  |    |
| Region  | string  | SecretKey  |   |  地域  |
| SessionToken  | string  | token  |   |  仅Minio中使用  |
| IsEnableHttps  | string  | 是否启用HTTPS  |   |    |
| IsEnableCache  | string  | 是否启用缓存  |   |  启用后将缓存签名URL，以减少请求次数  |

5、API说明

