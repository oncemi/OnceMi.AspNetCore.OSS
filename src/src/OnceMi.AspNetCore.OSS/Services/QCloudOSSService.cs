using COSXML;
using COSXML.Common;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.Model.Service;
using COSXML.Model.Tag;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class QCloudOSSService : IQCloudOSSService
    {
        private readonly IMemoryCache _cache;
        private readonly CosXml _client = null;
        public OSSOptions Options { get; private set; }

        public CosXml Context
        {
            get
            {
                return this._client;
            }
        }

        public QCloudOSSService(CosXml client
            , IMemoryCache cache
            , OSSOptions options)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(CosXml));
            this._cache = cache ?? throw new ArgumentNullException(nameof(IMemoryCache));
            this.Options = options ?? throw new ArgumentNullException(nameof(OSSOptions));
        }

        #region bucekt

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            HeadBucketRequest request = new HeadBucketRequest(bucketName);
            try
            {
                HeadBucketResult result = _client.HeadBucket(request);
                return await Task.FromResult(true);
            }
            catch (COSXML.CosException.CosClientException ex)
            {
                throw new Exception($"Rquest client error, {ex.Message}", ex);
            }
            catch (COSXML.CosException.CosServerException ex)
            {
                if (ex.statusCode == 403)
                {
                    return await Task.FromResult(true);
                }
                else if (ex.statusCode == 404)
                {
                    return await Task.FromResult(false);
                }
                else
                {
                    throw new Exception($"Server error, {ex.Message}", ex);
                }
            }
        }

        public Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            PutBucketRequest request = new PutBucketRequest(bucketName);
            //执行请求
            _client.PutBucket(request);
            return Task.FromResult(true);
        }

        public Task<List<Bucket>> ListBucketsAsync()
        {
            GetServiceRequest request = new GetServiceRequest();
            GetServiceResult result = _client.GetService(request);
            if (result == null || result.listAllMyBuckets == null)
            {
                throw new Exception("List buckets result is null.");
            }
            //得到所有的 buckets
            List<ListAllMyBuckets.Bucket> allBuckets = result.listAllMyBuckets.buckets;
            List<Bucket> buckets = new List<Bucket>();
            foreach (var item in allBuckets)
            {
                buckets.Add(new Bucket()
                {
                    Location = item.location,
                    Name = item.name,
                    Owner = new Owner()
                    {
                        Id = result.listAllMyBuckets.owner.id,
                        Name = result.listAllMyBuckets.owner.disPlayName,
                    },
                    CreationDate = item.createDate
                });
            }
            return Task.FromResult(buckets);
        }

        public Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            DeleteBucketRequest request = new DeleteBucketRequest(bucketName);
            //执行请求
            DeleteBucketResult result = _client.DeleteBucket(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            var acl = mode switch
            {
                AccessMode.Default => CosACL.Private,
                AccessMode.Private => CosACL.Private,
                AccessMode.PublicRead => CosACL.PublicRead,
                AccessMode.PublicReadWrite => CosACL.PublicReadWrite,
                _ => CosACL.Private,
            };
            PutBucketACLRequest request = new PutBucketACLRequest(bucketName);
            //设置私有读写权限
            request.SetCosACL(acl);
            //执行请求
            PutBucketACLResult result = _client.PutBucketACL(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            GetBucketACLRequest request = new GetBucketACLRequest(bucketName);
            //执行请求
            GetBucketACLResult result = _client.GetBucketACL(request);
            //存储桶的 ACL 信息
            AccessControlPolicy acl = result.accessControlPolicy;

            bool isPublicRead = false;
            bool isPublicWrite = false;
            if (acl != null
                && acl.accessControlList != null
                && acl.accessControlList.grants != null
                && acl.accessControlList.grants.Count > 0)
            {
                foreach (var item in acl.accessControlList.grants)
                {
                    if (string.IsNullOrEmpty(item.grantee.uri))
                    {
                        continue;
                    }
                    if (item.grantee.uri.Contains("allusers", StringComparison.OrdinalIgnoreCase))
                    {
                        switch (item.permission.ToLower())
                        {
                            case "read":
                                isPublicRead = true;
                                break;
                            case "write":
                                isPublicWrite = true;
                                break;
                        }
                    }
                }
            }

            //结果
            if (isPublicRead && !isPublicWrite)
            {
                return Task.FromResult(AccessMode.PublicRead);
            }
            else if (isPublicRead && isPublicWrite)
            {
                return Task.FromResult(AccessMode.PublicReadWrite);
            }
            else if (!isPublicRead && isPublicWrite)
            {
                return Task.FromResult(AccessMode.Private);
            }
            else
            {
                return Task.FromResult(AccessMode.Private);
            }
        }

        #endregion

        #region Object

        public async Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            List<Item> items = await ListObjectsAsync(bucketName, objectName);
            if (items != null && items.Count > 0)
            {
                Item result = items.Where(p => p.Key == objectName).FirstOrDefault();
                if (result != null)
                {
                    return await Task.FromResult(true);
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bucketName = ConvertBucketName(bucketName);
            ListBucket info = null;
            string nextMarker = null;
            List<Item> items = new List<Item>();
            do
            {
                GetBucketRequest request = new GetBucketRequest(bucketName);
                if (!string.IsNullOrEmpty(nextMarker))
                {
                    request.SetMarker(nextMarker);
                }
                if (!string.IsNullOrEmpty(prefix))
                {
                    request.SetPrefix(prefix);
                }
                //执行请求
                GetBucketResult result = _client.GetBucket(request);
                //bucket的相关信息
                info = result.listBucket;
                if (info.isTruncated)
                {
                    // 数据被截断，记录下数据下标
                    nextMarker = info.nextMarker;
                }
                foreach (var item in info.contentsList)
                {
                    items.Add(new Item()
                    {
                        Key = item.key,
                        LastModified = item.lastModified,
                        ETag = item.eTag,
                        Size = (ulong)item.size,
                        IsDir = false,
                        BucketName = bucketName,
                        VersionId = null,
                    });
                }
            } while (info.isTruncated);
            return Task.FromResult(items);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (!await ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'");
            }
            bucketName = ConvertBucketName(bucketName);

            await Task.Run(() =>
            {
                GetObjectBytesRequest request = new GetObjectBytesRequest(bucketName, objectName);
                //执行请求
                GetObjectBytesResult result = _client.GetObject(request);
                //获取内容
                byte[] content = result.content;
                if (content != null && content.Length > 0)
                {
                    MemoryStream ms = new MemoryStream(content);
                    callback(ms);
                }
                else
                {
                    throw new Exception("Get object bytes is null.");
                }
            }, cancellationToken);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (!await ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'");
            }
            bucketName = ConvertBucketName(bucketName);
            await Task.Run(() =>
            {
                string path = Path.GetFullPath(fileName);
                path = Directory.GetParent(fileName).FullName;
                string name = Path.GetFileName(fileName);
                GetObjectRequest request = new GetObjectRequest(bucketName, objectName, path, name);
                _client.GetObject(request);
            }, cancellationToken);
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            byte[] StreamToBytes(Stream stream)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            byte[] upload = StreamToBytes(data);
            if (upload == null || upload.Length == 0)
            {
                throw new Exception("Upload file stram is null.");
            }
            string contentType = "application/octet-stream";
            if (data is FileStream fileStream)
            {
                string fileName = fileStream.Name;
                if (!string.IsNullOrEmpty(fileName))
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
                }
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            bucketName = ConvertBucketName(bucketName);
            PostObjectRequest request = new PostObjectRequest(bucketName, objectName, upload);
            request.SetContentType(contentType);
            PostObjectResult result = _client.PostObject(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new Exception("Upload file is not exist.");
            }
            bucketName = ConvertBucketName(bucketName);
            PutObjectRequest request = new PutObjectRequest(bucketName, objectName, filePath);
            PutObjectResult result = _client.PutObject(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            bucketName = ConvertBucketName(bucketName);
            HeadObjectRequest request = new HeadObjectRequest(bucketName, objectName);
            if (!string.IsNullOrEmpty(versionID))
            {
                request.SetVersionId(versionID);
            }
            //执行请求
            HeadObjectResult result = _client.HeadObject(request);
            if (!result.IsSuccessful())
            {
                throw new Exception("Query object meta data failed.");
            }
            ItemMeta metaData = new ItemMeta()
            {
                ObjectName = objectName,
                Size = result.size,
                ETag = result.eTag,
                IsEnableHttps = Options.IsEnableHttps,
            };
            return Task.FromResult(metaData);
        }

        public Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (string.IsNullOrEmpty(destBucketName))
            {
                destBucketName = bucketName;
            }
            if (string.IsNullOrEmpty(destObjectName))
            {
                destObjectName = objectName;
            }
            bucketName = ConvertBucketName(bucketName);
            CopySourceStruct copySource = new CopySourceStruct(Options.Endpoint, bucketName, Options.Region, objectName);
            string bucket = ConvertBucketName(destBucketName);
            CopyObjectRequest request = new CopyObjectRequest(bucket, destObjectName);
            //设置拷贝源
            request.SetCopySource(copySource);
            //设置是否拷贝还是更新,此处是拷贝
            request.SetCopyMetaDataDirective(COSXML.Common.CosMetaDataDirective.Copy);
            //执行请求
            CopyObjectResult result = _client.CopyObject(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            bucketName = ConvertBucketName(bucketName);
            DeleteObjectRequest request = new DeleteObjectRequest(bucketName, objectName);
            DeleteObjectResult result = _client.DeleteObject(request);
            return Task.FromResult(result.IsSuccessful());
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
            bucketName = ConvertBucketName(bucketName);
            DeleteMultiObjectRequest request = new DeleteMultiObjectRequest(bucketName);
            //设置返回结果形式
            request.SetDeleteQuiet(false);
            request.SetObjectKeys(objectNames);
            DeleteMultiObjectResult result = _client.DeleteMultiObjects(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public void RemovePresignedUrlCache(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (Options.IsEnableCache)
            {
                string key = Encrypt.MD5($"{bucketName}_{objectName}_{PresignedObjectType.Put.ToString().ToUpper()}");
                _cache.Remove(key);
                key = Encrypt.MD5($"{bucketName}_{objectName}_{PresignedObjectType.Get.ToString().ToUpper()}");
                _cache.Remove(key);
            }
        }

        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Get);
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Put);
        }

        public Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            bucketName = ConvertBucketName(bucketName);
            var acl = mode switch
            {
                AccessMode.Default => CosACL.Private,
                AccessMode.Private => CosACL.Private,
                AccessMode.PublicRead => CosACL.PublicRead,
                AccessMode.PublicReadWrite => CosACL.PublicReadWrite,
                _ => CosACL.Private,
            };
            if (acl == CosACL.PublicReadWrite)
            {
                throw new Exception("QCloud object not support public read and write.");
            }

            PutObjectACLRequest request = new PutObjectACLRequest(bucketName, objectName);
            //设置私有读写权限 
            request.SetCosACL(acl);
            PutObjectACLResult result = _client.PutObjectACL(request);
            return Task.FromResult(result.IsSuccessful());
        }

        public Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            bool isPublicRead = false;
            bool isPublicWrite = false;

            bucketName = ConvertBucketName(bucketName);
            GetObjectACLRequest request = new GetObjectACLRequest(bucketName, objectName);
            GetObjectACLResult result = _client.GetObjectACL(request);
            AccessControlPolicy acl = result.accessControlPolicy;

            if (acl != null
                && acl.accessControlList != null
                && acl.accessControlList.grants != null
                && acl.accessControlList.grants.Count > 0)
            {
                foreach (var item in acl.accessControlList.grants)
                {
                    if (string.IsNullOrEmpty(item.grantee.uri))
                    {
                        continue;
                    }
                    if (item.grantee.uri.Contains("allusers", StringComparison.OrdinalIgnoreCase))
                    {
                        switch (item.permission.ToLower())
                        {
                            case "read":
                                isPublicRead = true;
                                break;
                            case "write":
                                isPublicWrite = true;
                                break;
                        }
                    }
                }
            }

            //结果
            if (isPublicRead && !isPublicWrite)
            {
                return Task.FromResult(AccessMode.PublicRead);
            }
            else if (isPublicRead && isPublicWrite)
            {
                return Task.FromResult(AccessMode.PublicReadWrite);
            }
            else if (!isPublicRead && isPublicWrite)
            {
                return Task.FromResult(AccessMode.Private);
            }
            else
            {
                return Task.FromResult(AccessMode.Private);
            }
        }

        public async Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            if (await SetObjectAclAsync(bucketName, objectName, AccessMode.Private))
            {
                return await GetObjectAclAsync(bucketName, objectName);
            }
            throw new Exception("Remove object acl failed.");
        }

        #endregion

        #region private

        private async Task<string> PresignedObjectAsync(string bucketName, string objectName, int expiresInt, PresignedObjectType type)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
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
            string key = Encrypt.MD5($"{bucketName}_{objectName}_{type.ToString().ToUpper()}");
            string objectUrl = null;
            string newBucketName = ConvertBucketName(bucketName);
            PreSignatureStruct preSignatureStruct = new PreSignatureStruct()
            {
                appid = Options.Endpoint,
                region = Options.Region,
                bucket = newBucketName,
                key = objectName,
                httpMethod = type.ToString().ToUpper(),
                isHttps = Options.IsEnableHttps,
                signDurationSecond = expiresInt,
                headers = null,
                queryParameters = null,
            };

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
                    objectUrl = $"{(Options.IsEnableHttps ? "https" : "http")}://{newBucketName}.cos.{Options.Region}.myqcloud.com{(objectName.StartsWith("/") ? "" : "/")}{objectName}";
                }
                else
                {
                    string uri = _client.GenerateSignURL(preSignatureStruct);
                    if (uri != null)
                    {
                        objectUrl = uri.ToString();
                    }
                }
            }
            else
            {
                string uri = _client.GenerateSignURL(preSignatureStruct);
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

        private string ConvertBucketName(string input)
        {
            return $"{input}-{Options.Endpoint}";
        }

        #endregion
    }
}
