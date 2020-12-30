# OnceMi.AspNetCore.OSS
Asp.Net Core 5.0对象储存扩展包，支持Minio自建对象储存、阿里云OSS、腾讯云COS。支持OSS常规操作，比如储存桶创建，删除、对象上传、下载、生成签名URL等。目前仅支持.NET 5，也推荐升级至.NET 5.

# Documents
Minio:https://docs.min.io/docs/dotnet-client-api-reference.html  
Aliyun:https://help.aliyun.com/document_detail/32085.html  
QCloud:https://cloud.tencent.com/document/product/436/32819  

# How to use
1、Install OnceMi.AspNetCore.OSS。  
CLI中安装：  
```shell
dotnet add package OnceMi.AspNetCore.OSS
```
Nuget中安装：  
在Nuget包管理器中搜索`OnceMi.AspNetCore.OSS`并安装。  

2、Configuration  
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
//从配置文件中加载节点为‘OSSProvider’的配置信息
services.AddOSSService("QCloud", "OSSProvider");
```

可注入多个OSSService，不同的Service用名称来区分。需要注意的是，腾讯云COS中配置节点Endpoint表示AppId。  

3、Use  
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

### Option Params

|  名称 |  类型  | 说明  | 案例  |  备注 |
| :------------ |:------------ | :------------ | :------------ | :------------ |
| Provider  | 枚举  | OSS提供者  |  Minio | 允许值：Minio,Aliyun, QCloud |
| Endpoint  | string  | 节点  | oss-cn-hangzhou.aliyuncs.com  |  在腾讯云OSS中表示AppId  |
| AccessKey  | string  | AccessKey  | F...............s  |    |
| SecretKey  | string  | SecretKey  | v...............d  |    |
| Region  | string  | 地域  | ap-chengdu  |    |
| SessionToken  | string  | token  |   |  仅Minio中使用  |
| IsEnableHttps  | bool  | 是否启用HTTPS  |  true  |  建议启用  |
| IsEnableCache  | bool  | 是否启用缓存  |  true  |  启用后将缓存签名URL，以减少请求次数  |

### API Reference

##### BucketExistsAsync  
`Task<bool> BucketExistsAsync(string bucketName);`

判断该储存桶是否存在。  

##### CreateBucketAsync  
`Task<bool> CreateBucketAsync(string bucketName);`

创建一个储存桶。如果当前储存桶存在，则直接返回True。  

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

上传文件。支持流式上传和上传本地文件。  

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

获取对象的元数据，或更具VersionId获取对象元数据。需要注意的是，在阿里云对象存储和腾讯云对象存储中不支持matchEtag和modifiedSincecan参数。  

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

##### SetObjectAclAsync  
`Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode);`

设置对象的访问权限，默认文件的访问权限是集成储存桶的。但是可以单独通过此API为对象设置访问权限。  

##### GetObjectAclAsync  
`Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName);`

获取对象的储存桶权限，如果是该权限继承自储存桶，获取的可能是储存桶对当前对象的访问权限。  

##### RemoveObjectAclAsync  
`Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName);`

清除该对象的访问权限或将其恢复至继承权限。  
