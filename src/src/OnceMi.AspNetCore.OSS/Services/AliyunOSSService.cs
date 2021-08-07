using Aliyun.OSS;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class AliyunOSSService : IBaseOSSService, IAliyunOSSService
    {
        private readonly IMemoryCache _cache;
        private readonly OssClient _client = null;
        public OSSOptions Options { get; private set; }

        public OssClient Context
        {
            get
            {
                return this._client;
            }
        }

        public AliyunOSSService(OssClient client
            , IMemoryCache provider
            , OSSOptions options)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(OssClient));
            this._cache = provider ?? throw new ArgumentNullException(nameof(IMemoryCache));
            this.Options = options ?? throw new ArgumentNullException(nameof(OSSOptions));
        }

        #region Bucket

        public Task<List<Bucket>> ListBucketsAsync()
        {
            IEnumerable<Aliyun.OSS.Bucket> buckets = _client.ListBuckets();
            if (buckets == null)
            {
                return null;
            }
            if (buckets.Count() == 0)
            {
                return Task.FromResult(new List<Bucket>());
            }
            var resultList = new List<Bucket>();
            foreach (var item in buckets)
            {
                resultList.Add(new Bucket()
                {
                    Location = item.Location,
                    Name = item.Name,
                    Owner = new Owner()
                    {
                        Name = item.Owner.DisplayName,
                        Id = item.Owner.Id
                    },
                    CreationDate = item.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }
            return Task.FromResult(resultList);
        }

        public Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            return Task.FromResult(_client.DoesBucketExist(bucketName));
        }

        public Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            //检查桶是否存在
            Bucket bucket = ListBucketsAsync().Result?.Where(p => p.Name == bucketName)?.FirstOrDefault();
            if (bucket != null)
            {
                string localtion = Options.Endpoint?.Split('.')[0];
                if (bucket.Location.Equals(localtion, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BucketExistException($"Bucket '{bucketName}' already exists.");
                }
                else
                {
                    throw new BucketExistException($"There have a same name bucket '{bucketName}' in other localtion '{bucket.Location}'.");
                }
            }
            var request = new CreateBucketRequest(bucketName)
            {
                //设置存储空间访问权限ACL。
                ACL = CannedAccessControlList.Private,
                //设置数据容灾类型。
                DataRedundancyType = DataRedundancyType.LRS
            };
            // 创建存储空间。
            var result = _client.CreateBucket(request);
            return Task.FromResult(result != null);
        }

        public Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            _client.DeleteBucket(bucketName);
            return Task.FromResult(true);
        }

        public Task<string> GetBucketLocationAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var result = _client.GetBucketLocation(bucketName);
            if (result == null)
            {
                return null;
            }
            return Task.FromResult(result.Location);
        }

        public Task<bool> SetBucketCorsRequestAsync(string bucketName, List<BucketCorsRule> rules)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (rules == null || rules.Count == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }
            var request = new SetBucketCorsRequest(bucketName);
            foreach (var item in rules)
            {
                var rule = new CORSRule();
                // 指定允许跨域请求的来源。
                rule.AddAllowedOrigin(item.Origin);
                // 指定允许的跨域请求方法(GET/PUT/DELETE/POST/HEAD)。
                rule.AddAllowedMethod(item.Method.ToString());
                // AllowedHeaders和ExposeHeaders不支持通配符。
                rule.AddAllowedHeader(item.AllowedHeader);
                // 指定允许用户从应用程序中访问的响应头。
                rule.AddExposeHeader(item.ExposeHeader);

                request.AddCORSRule(rule);
            }
            // 设置跨域资源共享规则。
            _client.SetBucketCors(request);
            return Task.FromResult(true);
        }

        public Task<string> GetBucketEndpointAsync(string bucketName)
        {
            var result = _client.GetBucketInfo(bucketName);
            if (result != null
                && result.Bucket != null
                && !string.IsNullOrEmpty(result.Bucket.Name)
                && !string.IsNullOrEmpty(result.Bucket.ExtranetEndpoint))
            {
                string host = $"{(Options.IsEnableHttps ? "https://" : "http://")}{result.Bucket.Name}.{result.Bucket.ExtranetEndpoint}";
                return Task.FromResult(host);
            }
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// 设置储存桶的访问权限
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var canned = mode switch
            {
                AccessMode.Default => CannedAccessControlList.Default,
                AccessMode.Private => CannedAccessControlList.Private,
                AccessMode.PublicRead => CannedAccessControlList.PublicRead,
                AccessMode.PublicReadWrite => CannedAccessControlList.PublicReadWrite,
                _ => CannedAccessControlList.Default,
            };
            var request = new SetBucketAclRequest(bucketName, canned);
            _client.SetBucketAcl(request);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 获取储存桶的访问权限
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var result = _client.GetBucketAcl(bucketName);
            var mode = result.ACL switch
            {
                CannedAccessControlList.Default => AccessMode.Default,
                CannedAccessControlList.Private => AccessMode.Private,
                CannedAccessControlList.PublicRead => AccessMode.PublicRead,
                CannedAccessControlList.PublicReadWrite => AccessMode.PublicReadWrite,
                _ => AccessMode.Default,
            };
            return Task.FromResult(mode);
        }

        #endregion

        #region Object

        public Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            var obj = _client.GetObject(bucketName, objectName);
            callback(obj.Content);
            return Task.CompletedTask;
        }

        public Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            GetObjectAsync(bucketName, objectName, (st) =>
              {
                  byte[] buf = new byte[1024];
                  var fs = File.Open(fileName, FileMode.OpenOrCreate);
                  var len = 0;
                  // 通过输入流将文件的内容读取到文件或者内存中。
                  while ((len = st.Read(buf.AsSpan(0, 1024))) != 0)
                  {
                      fs.Write(buf.AsSpan(0, len));
                  }
                  fs.Close();
              }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            List<Item> result = new List<Item>();
            ObjectListing resultObj = null;
            string nextMarker = string.Empty;
            do
            {
                // 每页列举的文件个数通过maxKeys指定，超过指定数将进行分页显示。
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 100
                };
                resultObj = _client.ListObjects(listObjectsRequest);
                if (resultObj == null)
                {
                    continue;
                }
                foreach (var item in resultObj.ObjectSummaries)
                {
                    result.Add(new Item()
                    {
                        Key = item.Key,
                        LastModified = item.LastModified.ToString(),
                        ETag = item.ETag,
                        Size = (ulong)item.Size,
                        BucketName = bucketName,
                        IsDir = false,
                        LastModifiedDateTime = item.LastModified
                    });
                }
                nextMarker = resultObj.NextMarker;
            } while (resultObj.IsTruncated);
            return Task.FromResult(result);
        }

        public Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            return Task.FromResult(_client.DoesObjectExist(bucketName, objectName));
        }

        public Task RemovePresignedUrlCache(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (Options.IsEnableCache)
            {
                string key = BuildPresignedObjectCacheKey(bucketName, objectName, PresignedObjectType.Get);
                _cache.Remove(key);
                key = BuildPresignedObjectCacheKey(bucketName, objectName, PresignedObjectType.Put);
                _cache.Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Get);
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Put);
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            var result = _client.PutObject(bucketName, objectName, data);
            if (result != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!File.Exists(filePath))
            {
                throw new Exception("Upload file is not exist.");
            }
            var result = _client.PutObject(bucketName, objectName, filePath);
            if (result != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 文件拷贝，默认采用分片拷贝的方式
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="destBucketName"></param>
        /// <param name="destObjectName"></param>
        /// <returns></returns>
        public Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName = null, string destObjectName = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(destBucketName))
            {
                destBucketName = bucketName;
            }
            destObjectName = FormatObjectName(destObjectName);
            var partSize = 50 * 1024 * 1024;
            // 创建OssClient实例。
            // 初始化拷贝任务。可以通过InitiateMultipartUploadRequest指定目标文件元信息。
            var request = new InitiateMultipartUploadRequest(destBucketName, destObjectName);
            var result = _client.InitiateMultipartUpload(request);
            // 计算分片数。
            var metadata = _client.GetObjectMetadata(bucketName, objectName);
            var fileSize = metadata.ContentLength;
            var partCount = (int)fileSize / partSize;
            if (fileSize % partSize != 0)
            {
                partCount++;
            }
            // 开始分片拷贝。
            var partETags = new List<PartETag>();
            for (var i = 0; i < partCount; i++)
            {
                var skipBytes = (long)partSize * i;
                var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);
                // 创建UploadPartCopyRequest。可以通过UploadPartCopyRequest指定限定条件。
                var uploadPartCopyRequest = new UploadPartCopyRequest(destBucketName, destObjectName, bucketName, objectName, result.UploadId)
                {
                    PartSize = size,
                    PartNumber = i + 1,
                    // BeginIndex用来定位此次上传分片开始所对应的位置。
                    BeginIndex = skipBytes
                };
                // 调用uploadPartCopy方法来拷贝每一个分片。
                var uploadPartCopyResult = _client.UploadPartCopy(uploadPartCopyRequest);
                partETags.Add(uploadPartCopyResult.PartETag);
            }
            // 完成分片拷贝。
            var completeMultipartUploadRequest =
            new CompleteMultipartUploadRequest(destBucketName, destObjectName, result.UploadId);
            // partETags为分片上传中保存的partETag的列表，OSS收到用户提交的此列表后，会逐一验证每个数据分片的有效性。全部验证通过后，OSS会将这些分片合成一个完整的文件。
            foreach (var partETag in partETags)
            {
                completeMultipartUploadRequest.PartETags.Add(partETag);
            }
            _client.CompleteMultipartUpload(completeMultipartUploadRequest);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            var result = _client.DeleteObject(bucketName, objectName);
            if (result != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (objectNames == null || objectNames.Count == 0)
            {
                throw new ArgumentNullException(nameof(objectNames));
            }
            List<string> delObjects = new List<string>();
            foreach (var item in objectNames)
            {
                delObjects.Add(FormatObjectName(item));
            }
            var quietMode = false;
            // DeleteObjectsRequest的第三个参数指定返回模式。
            var request = new DeleteObjectsRequest(bucketName, delObjects, quietMode);
            // 删除多个文件。
            var result = _client.DeleteObjects(request);
            if ((!quietMode) && (result.Keys != null))
            {
                if (result.Keys.Count() == delObjects.Count)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    throw new Exception("Some file delete failed.");
                }
            }
            else
            {
                if (result != null)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(true);
                }
            }
        }

        public Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            GetObjectMetadataRequest request = new GetObjectMetadataRequest(bucketName, objectName)
            {
                VersionId = versionID
            };
            var oldMeta = _client.GetObjectMetadata(request);
            // 设置新的文件元信息。
            var newMeta = new ItemMeta()
            {
                ObjectName = objectName,
                ContentType = oldMeta.ContentType,
                Size = oldMeta.ContentLength,
                LastModified = oldMeta.LastModified,
                ETag = oldMeta.ETag,
                IsEnableHttps = Options.IsEnableHttps,
                MetaData = new Dictionary<string, string>(),
            };
            if (oldMeta.UserMetadata != null && oldMeta.UserMetadata.Count > 0)
            {
                foreach (var item in oldMeta.UserMetadata)
                {
                    newMeta.MetaData.Add(item.Key, item.Value);
                }
            }
            return Task.FromResult(newMeta);
        }

        public async Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!await this.ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
            }
            var canned = mode switch
            {
                AccessMode.Default => CannedAccessControlList.Default,
                AccessMode.Private => CannedAccessControlList.Private,
                AccessMode.PublicRead => CannedAccessControlList.PublicRead,
                AccessMode.PublicReadWrite => CannedAccessControlList.PublicReadWrite,
                _ => CannedAccessControlList.Default,
            };
            _client.SetObjectAcl(bucketName, objectName, canned);
            return await Task.FromResult(true);
        }

        public async Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!await this.ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
            }
            var result = _client.GetObjectAcl(bucketName, objectName);
            var mode = result.ACL switch
            {
                CannedAccessControlList.Default => AccessMode.Default,
                CannedAccessControlList.Private => AccessMode.Private,
                CannedAccessControlList.PublicRead => AccessMode.PublicRead,
                CannedAccessControlList.PublicReadWrite => AccessMode.PublicReadWrite,
                _ => AccessMode.Default,
            };
            if (mode == AccessMode.Default)
            {
                return await GetBucketAclAsync(bucketName);
            }
            return await Task.FromResult(mode);
        }

        public async Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!await SetObjectAclAsync(bucketName, objectName, AccessMode.Default))
            {
                throw new Exception("Save new policy info failed when remove object acl.");
            }
            return await GetObjectAclAsync(bucketName, objectName);
        }
        #endregion

        #region private

        private async Task<string> PresignedObjectAsync(string bucketName, string objectName, int expiresInt, PresignedObjectType type)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (expiresInt <= 0)
            {
                throw new Exception("ExpiresIn time can not less than 0.");
            }
            if (expiresInt > 7 * 24 * 3600)
            {
                throw new Exception("ExpiresIn time no more than 7 days.");
            }
            long nowTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            const int minExpiresInt = 600;
            string key = BuildPresignedObjectCacheKey(bucketName, objectName, type);
            string objectUrl = null;
            //查找缓存
            if (Options.IsEnableCache && (expiresInt > minExpiresInt))
            {
                var cacheResult = _cache.Get<PresignedUrlCache>(key);
                PresignedUrlCache cache = cacheResult != null ? cacheResult : null;
                //缓存中存在，且有效时间不低于10分钟
                if (cache != null
                    && cache.Type == type
                    && cache.CreateTime > 0
                    && (cache.CreateTime + expiresInt - nowTime) > minExpiresInt
                    && cache.Name == objectName
                    && cache.BucketName == bucketName)
                {
                    return cache.Url;
                }
            }
            if (type == PresignedObjectType.Get)
            {
                //生成URL
                AccessMode accessMode = await this.GetObjectAclAsync(bucketName, objectName);
                if (accessMode == AccessMode.PublicRead || accessMode == AccessMode.PublicReadWrite)
                {
                    string bucketUrl = await this.GetBucketEndpointAsync(bucketName);
                    return $"{bucketUrl}{(objectName.StartsWith("/") ? "" : "/")}{objectName}";
                }
                else
                {
                    var req = new GeneratePresignedUriRequest(bucketName, objectName, SignHttpMethod.Get)
                    {
                        Expiration = DateTime.Now.AddSeconds(expiresInt)
                    };
                    var uri = _client.GeneratePresignedUri(req);
                    if (uri != null)
                    {
                        objectUrl = uri.ToString();
                    }
                }
            }
            else
            {
                var req = new GeneratePresignedUriRequest(bucketName, objectName, SignHttpMethod.Put)
                {
                    Expiration = DateTime.Now.AddSeconds(expiresInt)
                };
                var uri = _client.GeneratePresignedUri(req);
                if (uri != null)
                {
                    objectUrl = uri.ToString();
                }
            }
            if (string.IsNullOrEmpty(objectUrl))
            {
                throw new Exception("Presigned get object url failed.");
            }
            //save cache
            if (Options.IsEnableCache && expiresInt > minExpiresInt)
            {
                PresignedUrlCache urlCache = new PresignedUrlCache()
                {
                    Url = objectUrl,
                    CreateTime = nowTime,
                    Name = objectName,
                    BucketName = bucketName,
                    Type = type
                };
                int randomSec = new Random().Next(5, 30);
                _cache.Set(key, urlCache, TimeSpan.FromSeconds(expiresInt + randomSec));
            }
            return objectUrl;
        }

        #endregion
    }
}
