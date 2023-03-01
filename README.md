
# OnceMi.AspNetCore.OSS
Asp.Net Core对象储存扩展包，支持Minio自建对象储存、阿里云OSS、腾讯云COS、七牛云Kodo、华为云OBS、百度云BOS。支持OSS常规操作，比如储存桶创建，删除、对象上传、下载、生成签名URL等。目前支持.NET Core3.1/.NET 5/.NET 6，推荐升级至.NET 6.

## 各厂家相关SDK文档  
- Minio: [点此查看](https://docs.min.io/docs/dotnet-client-api-reference.html "点此查看")  
- Aliyun: [点此查看](https://help.aliyun.com/document_detail/32085.html "点此查看")  
- QCloud: [点此查看](https://cloud.tencent.com/document/product/436/32819 "点此查看")  
- 七牛云: [点此查看](https://developer.qiniu.com/kodo/1237/csharp "点此查看")  
- HuaweiOBS：[点此查看](https://support.huaweicloud.com/sdk-dotnet-devg-obs/obs_25_0001.html "点此查看")  
- 百度云BOS：[点此查看](https://cloud.baidu.com/doc/BOS/s/ejwvys1ju "点此查看")  

## 已知问题  
1. Minio通过Nginx发反向代理后直接通过域名（不加端口）调用存在问题，应该是Minio本身问题，有兴趣的可以自行测试研究，具体信息我已经发布在Issue中。  
2. ~~腾讯云`PutObjectAsync`流式上传接口，有非常低的概率会抛“储存桶不存在的异常”，应该是腾讯云自身的原因，具体原因未知。~~ PS：最近没有复现了

## 如何使用  
1、安装`OnceMi.AspNetCore.OSS`依赖。  
Cmd install：  
```shell
dotnet add package OnceMi.AspNetCore.OSS
```
Nuget： [![](https://img.shields.io/nuget/v/OnceMi.AspNetCore.OSS.svg)](https://www.nuget.org/packages/OnceMi.AspNetCore.OSS)

2、在`Startup.cs`中配置  
You need to configure OSSService in your Startup.cs：

```csharp
//default minio
//添加默认对象储存配置信息
services.AddOSSService(option =>
{
    option.Provider = OSSProvider.Minio;
    option.Endpoint = "oss.oncemi.com:9000";
    option.AccessKey = "Q*************9";
    option.SecretKey = "A**************************Q";
    option.IsEnableHttps = true;
    option.IsEnableCache = true;
});

//aliyun oss
//添加名称为‘aliyunoss’的OSS对象储存配置信息
services.AddOSSService("aliyunoss", option =>
 {
     option.Provider = OSSProvider.Aliyun;
     option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
     option.AccessKey = "L*******************U";
     option.SecretKey = "5*******************************T";
     option.IsEnableCache = true;
 });

//qcloud oss
//也可以从配置文件中加载节点为‘OSSProvider’的配置信息
services.AddOSSService("QCloud", "OSSProvider");
```

可注入多个OSSService，不同的Service用名称来区分。需要注意的是，腾讯云COS中配置节点Endpoint表示AppId。  

appsettings.json配置文件实例： 
```csharp
{
  "OSSProvider": {
    "Provider": "QCloud", //枚举值支持：Minio/Aliyun/QCloud
    "Endpoint": "你的AppId", //腾讯云中表示AppId
    "Region": "ap-chengdu",  //地域
    "AccessKey": "A****************************z",
    "SecretKey": "g6I***************la",
    "IsEnableCache": true  //是否启用缓存，推荐开启
  }
}

```

3、使用Demo

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
/// 获取IOSSServiceFactory，根据名称创建对应的OSS服务
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

### 配置参数  

|  名称 |  类型  | 说明  | 案例  |  备注 |
| :------------ |:------------ | :------------ | :------------ | :------------ |
| Provider  | 枚举  | OSS提供者  |  Minio | 允许值：Minio/Aliyun/QCloud/Qiniu/HuaweiCloud |
| Endpoint  | string  | 节点  | oss-cn-hangzhou.aliyuncs.com  |  在腾讯云OSS中表示AppId  |
| AccessKey  | string  | AccessKey  | F...............s  |    |
| SecretKey  | string  | SecretKey  | v...............d  |    |
| Region  | string  | 地域  | ap-chengdu  |    |
| IsEnableHttps  | bool  | 是否启用HTTPS  |  true  |  建议启用  |
| IsEnableCache  | bool  | 是否启用缓存  |  true  |  启用后将缓存签名URL，以减少请求次数  |

#### Endpoint查询  
| Provider  | Endpoint  | Remark  |
| ------------ | ------------ | ------------ |
| Minio  | -  | 默认或自建Minio Endpoint  |
| Aliyun  | https://help.aliyun.com/document_detail/31837.html  | -  |
| QCloud  | -  | 腾讯云没有Endpoint，此配置项表示AppId  |
| Qiniu  | https://developer.qiniu.com/kodo/4088/s3-access-domainname  | -  |
| HuaweiCloud  | https://support.huaweicloud.com/productdesc-obs/obs_03_0152.html  | -  |


### API参考  

##### BucketExistsAsync  
`Task<bool> BucketExistsAsync(string bucketName);`

判断该储存桶是否存在。  

##### CreateBucketAsync  
`Task<bool> CreateBucketAsync(string bucketName);`

创建一个储存桶。如果当前储存桶存在，将抛出异常`BucketExistException`。  

##### ListBucketsAsync  
`Task<bool> ListBucketsAsync();`

列出当前账号下允许访问的所有储存桶。  

##### RemoveBucketAsync  
`Task<bool> RemoveBucketAsync(string bucketName);`

移除当前储存桶。移除储存桶之前，请先移除储存桶中所有的对象和对象碎片文件。  

##### SetBucketAclAsync  
`Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode);`

设置储存桶的外部访问权限，支持的权限有：私有、公共读、公共读写。返回设置结果（True or False）。  

##### GetBucketAclAsync  
`Task<AccessMode> GetBucketAclAsync(string bucketName);`

获取储存桶的外部访问权限。  

##### ObjectsExistsAsync  
`Task<bool> ObjectsExistsAsync(string bucketName, string objectName);`

获取指定储存桶中指定对象是否存在。  

##### ListObjectsAsync  
`Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null);`

列出当前储存桶所有文件。如果储存桶中文件较多，可以需要较长的执行时间，因此推荐填写prefix参数，prefix会根据文件名称进行前端匹配。比如输出abc，则列出全部abc开头的文件或目录。  

##### GetObjectAsync  
获取文件的数据流。  
Methos 1:  

`Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default);`

Example  
```csharp
try
{
    await _OSSService.GetObjectAsync(_bucketName, "1.jpg", (stream) =>
    {
        using (FileStream fs = new FileStream("1.jpg", FileMode.Create, FileAccess.Write))
        {
            stream.CopyTo(fs);
            fs.Close();
        }
    });
    return Json("OK");
}
catch (Exception ex)
{
    throw ex;
}
```

Methos 2:  
 
`Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default);`

Example  
```csharp
try
{
    await _OSSService.GetObjectAsync(_bucketName, "1.jpg", "C:\\Temp\\1.jpg");
    return Json("OK");
}
catch (Exception ex)
{
    throw ex;
}
```

##### PutObjectAsync  

上传文件。支持流式上传和上传本地文件。腾讯云不止流式上传，为了兼容接口，采用先将流加载到内存中再上传。  

Method 1(流式上传):

`Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default(CancellationToken));`

Example  
```csharp
try
{
    byte[] bs = System.IO.File.ReadAllBytes(@"C:\Users\sysru\Desktop\PHOTO-1.jpg");
    using (MemoryStream filestream = new MemoryStream(bs))
    {
        await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", filestream);
    }

    return Json("OK");
}
catch (Exception ex)
{
    throw;
}
```

Method 2(上传本地文件):
`Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default);`

Example  
```csharp
try
{
    await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", @"C:\Users\sysru\Desktop\PHOTO-1.jpg");
    return Json("OK");
}
catch (Exception ex)
{
    throw;
}
```

##### GetObjectMetadataAsync  
```csharp
Task<ItemMeta> GetObjectMetadataAsync(string bucketName
    , string objectName
    , string versionID = null
    , string matchEtag = null
    , DateTime? modifiedSince = null);
```

获取对象的元数据，或根据VersionId获取对象元数据。需要注意的是，在阿里云对象存储和腾讯云对象存储中不支持matchEtag和modifiedSincecan参数。  

##### CopyObjectAsync  
`Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName = null);`

在储存桶之间复制对象。  

##### RemoveObjectAsync  
`Task<bool> RemoveObjectAsync(string bucketName, string objectName);`

删除储存桶中指定对象。  

`Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames);`

删除储存桶中多个对象。  

##### RemovePresignedUrlCache  
`void RemovePresignedUrlCache(string bucketName, string objectName);`

清除对象生成的签名URL缓存。在未开启签名URL缓存的情况下，此功能无效。  

##### PresignedGetObjectAsync  
`Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt);`

生成一个给HTTP GET请求用的presigned URL。浏览器/移动端的客户端可以用这个URL进行下载，即使其所在的存储桶是私有的。这个presigned URL可以设置一个失效时间，且不能超过7天。  
如果该对象拥有公共读权限或该对象继承了储存桶的公共读权限，将生成永久下载链接。  
如果Option参数中设置为IsEnableCache为True，将会在有效时间中缓存生成的签名链接，同时也推荐开启此功能，将大大降低请求的频率。  

##### PresignedPutObjectAsync  
`Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt);`

生成一个给HTTP PUT请求用的presigned URL。浏览器/移动端的客户端可以用这个URL进行上传，即使其所在的存储桶是私有的。这个presigned URL可以设置一个失效时间，且不能超过7天。  
如果Option参数中设置为IsEnableCache为True，将会在有效时间中缓存生成的签名链接，同时也推荐开启此功能，将大大降低请求的频率。  
注意：七牛云对象储存不支持此操作！  

##### SetObjectAclAsync  
`Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode);`

设置对象的访问权限，默认文件的访问权限是继承储存桶的。但是可以单独通过此API为对象设置访问权限。  
注意：七牛云、百度云对象储存不支持此操作！  

##### GetObjectAclAsync  
`Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName);`

获取对象的储存桶权限，如果是该权限继承自储存桶，获取的可能是储存桶对当前对象的访问权限。  
注意：七牛云、百度云对象储存不支持此操作！  

##### RemoveObjectAclAsync  
`Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName);`

清除该对象的访问权限或将其恢复至继承权限。  
注意：七牛云对象储存不支持此操作！   

### 替换内部缓存提供器

如果启用了缓存来缓存签名URL，可以提高单个文件的签名URL请求效率。由于1.1.3之前版本使用的MemoryCache，有三个问题：  
1、不支持分布式，只能单机缓存  
2、大量占用应用服务器内存  
3、应用重启之后，之前的缓存丢失  

从1.1.3开始，提供了一个ICacheProvider接口。用户可以自己实现此接口，替换掉内部的MemoryCache，比如使用Redis。  
下面是代码：
```csharp
class RedisCacheProvider : ICacheProvider
{
    private readonly RedisClient _cache;

    public RedisCacheProvider(RedisClient cache)
    {
        this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public T Get<T>(string key) where T : class
    {
        string val = _cache.Get(key);
        if (string.IsNullOrEmpty(val))
        {
            return default(T);
        }
        return JsonUtil.DeserializeStringToObject<T>(val);
    }

    public void Remove(string key)
    {
        _cache.Del(key);
    }

    public void Set<T>(string key, T value, TimeSpan ts) where T : class
    {
        string stringVal = JsonUtil.SerializeToString(value);
        _cache.Set(key, stringVal, ts);
    }
}

//构建Redis Client
var client = new RedisClient("127.0.0.1:6379,password=,ConnectTimeout=3000,defaultdatabase=0");
services.TryAddSingleton<RedisClient>(client);
//注入ICacheProvider，一定要在AddOSSService之前注入
services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
```

## Dependencies

1. Aliyun.OSS.SDK.NetCore
2. Microsoft.Extensions.Caching.Memory
3. Newtonsoft.Json
4. Tencent.QCloud.Cos.Sdk
5. Minio
6. Qiniu
7. https://github.com/huaweicloud/huaweicloud-sdk-dotnet-obs

## To do list  
~~1. 修改签名URL过期策略为滑动过期策略~~  
2. 文件分页加载  
3. 文件分片上传  
