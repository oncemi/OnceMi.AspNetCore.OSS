using Microsoft.Extensions.Caching.Memory;
using OBS;
using OBS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    /// <summary>
    /// 华为OSS服务
    /// </summary>
    /// <remarks>
    /// 由gudao提供，https://github.com/gudao
    /// </remarks>
    public class HaweiOSSService : BaseOSSService, IHaweiOSSService
    {
        private readonly ObsClient _client = null;

        public ObsClient Context
        {
            get
            {
                return this._client;
            }
        }

        public HaweiOSSService(IMemoryCache cache, OSSOptions options) : base(cache, options)
        {
            string endPoint = options.Endpoint;
            //如果是不带协议的endpoint，添加协议
            if (!endPoint.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                endPoint = options.IsEnableHttps ? "https://" + endPoint : "http://" + endPoint;
            }
            _client = new ObsClient(Options.AccessKey, Options.SecretKey, new ObsConfig()
            {
                Endpoint = endPoint
            });
        }

        #region bucket

        #region 华为云自由方法

        /// <summary>
        /// 获取桶存量信息
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public GetBucketStorageInfoResponse GetBucketStorageInfo(string bucketName)
        {
            GetBucketStorageInfoRequest request = new GetBucketStorageInfoRequest
            {
                BucketName = bucketName,
            };
            GetBucketStorageInfoResponse response = _client.GetBucketStorageInfo(request);
            return response;
        }

        /// <summary>
        /// 设置桶存储类型
        /// </summary>
        /// <param name="bucketName">储存桶名称</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        public bool SetBucketStoragePolicy(string bucketName, StorageClassEnum type)
        {
            SetBucketStoragePolicyRequest request = new SetBucketStoragePolicyRequest
            {
                BucketName = bucketName,
                StorageClass = type,
            };
            SetBucketStoragePolicyResponse response = _client.SetBucketStoragePolicy(request);
            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// 获取桶存储类型
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        public StorageClassEnum GetBucketStoragePolicy(string bucketName)
        {
            GetBucketStoragePolicyRequest request = new GetBucketStoragePolicyRequest()
            {
                BucketName = "bucketName",
            };
            GetBucketStoragePolicyResponse response = _client.GetBucketStoragePolicy(request);
            if (response.StatusCode != HttpStatusCode.OK || response.StorageClass == null)
            {
                throw new Exception($"Get bucket '{bucketName}' storage policy failed. response code is {response.StatusCode}, response data: {JsonUtil.SerializeObject(response)}");
            }
            return response.StorageClass.Value;
        }

        #endregion

        /// <summary>
        /// 创建桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (await BucketExistsAsync(bucketName))
            {
                throw new BucketExistException($"Bucket '{bucketName}' already exists.");
            }
            CreateBucketRequest request = new CreateBucketRequest
            {
                Location = Options.Region,
                BucketName = bucketName,
            };
            CreateBucketResponse response = _client.CreateBucket(request);
            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// 判断桶是否存在
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            HeadBucketRequest request = new HeadBucketRequest
            {
                BucketName = bucketName,
            };
            bool exists = _client.HeadBucket(request);
            return Task.FromResult(exists);
        }

        /// <summary>
        /// 列出桶
        /// </summary>
        /// <returns></returns>
        public Task<List<Bucket>> ListBucketsAsync()
        {
            ListBucketsRequest request = new ListBucketsRequest()
            {
                IsQueryLocation = true,
            };
            ListBucketsResponse response = _client.ListBuckets(request);
            var buckets = response.Buckets;
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
                    Name = item.BucketName,
                    CreationDate = item.CreationDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }
            return Task.FromResult(resultList);
        }

        public Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            GetBucketAclRequest request = new GetBucketAclRequest
            {
                BucketName = bucketName,
            };
            GetBucketAclResponse response = _client.GetBucketAcl(request);
            if (response.StatusCode != HttpStatusCode.OK || response.AccessControlList == null)
            {
                throw new Exception($"Get bucket '{bucketName}' ACL failed. response code is {response.StatusCode}, response data: {JsonUtil.SerializeObject(response)}");
            }
            bool hasAllUser = false;
            PermissionEnum? permission = null;
            foreach (var item in response.AccessControlList.Grants)
            {
                if (item.Grantee is GroupGrantee grantee)
                {
                    if (grantee.GroupGranteeType == GroupGranteeEnum.AllUsers)
                    {
                        hasAllUser = true;
                        permission = item.Permission;
                        break;
                    }
                }
            }
            if (hasAllUser)
            {
                switch (permission.Value)
                {
                    case PermissionEnum.Read:
                        return Task.FromResult(AccessMode.PublicRead);
                    case PermissionEnum.Write:
                    case PermissionEnum.FullControl:
                        return Task.FromResult(AccessMode.PublicReadWrite);
                    default:
                        return Task.FromResult(AccessMode.Private);
                }
            }
            return Task.FromResult(AccessMode.Private);
        }

        /// <summary>
        /// 设置桶权限
        /// </summary>
        /// <param name="bucketName"></param>
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
                AccessMode.Default => CannedAclEnum.Private,
                AccessMode.Private => CannedAclEnum.Private,
                AccessMode.PublicRead => CannedAclEnum.PublicRead,
                AccessMode.PublicReadWrite => CannedAclEnum.PublicReadWrite,
                _ => CannedAclEnum.Private,
            };
            SetBucketAclRequest request = new SetBucketAclRequest
            {
                BucketName = bucketName,
                CannedAcl = canned
            };
            SetBucketAclResponse response = _client.SetBucketAcl(request);
            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// 删除桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            DeleteBucketRequest request = new DeleteBucketRequest
            {
                BucketName = bucketName
            };
            DeleteBucketResponse response = _client.DeleteBucket(request);
            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        #endregion

        /// <summary>
        /// 获取对象信息
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="versionID"></param>
        /// <param name="matchEtag"></param>
        /// <param name="modifiedSince"></param>
        /// <returns></returns>
        public Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);

            GetObjectMetadataRequest request = new GetObjectMetadataRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName,
                VersionId = versionID,

            };
            GetObjectMetadataResponse response = _client.GetObjectMetadata(request);
            var newMeta = new ItemMeta()
            {
                ObjectName = objectName,
                ContentType = response.ContentType,
                Size = response.ContentLength,
                LastModified = response.LastModified.GetValueOrDefault(),
                ETag = response.ETag,
                IsEnableHttps = Options.IsEnableHttps,
                MetaData = new Dictionary<string, string>(),
            };
            if (response.Metadata != null && response.Metadata.Count > 0)
            {
                foreach (var item in response.Metadata.KeyValuePairs)
                {
                    newMeta.MetaData.Add(item.Key, item.Value);
                }
            }
            return Task.FromResult(newMeta);
        }
        /// <summary>
        /// 列表对象
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            List<Item> result = new List<Item>();
            ListObjectsResponse resultObj = null;
            string nextMarker = string.Empty;
            do
            {
                // 每页列举的文件个数通过maxKeys指定，超过指定数将进行分页显示。
                var listObjectsRequest = new ListObjectsRequest()
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    Marker = nextMarker,
                    MaxKeys = 100
                };
                resultObj = _client.ListObjects(listObjectsRequest);
                if (resultObj == null)
                {
                    continue;
                }
                foreach (var item in resultObj.ObsObjects)
                {
                    result.Add(new Item()
                    {
                        Key = item.ObjectKey,
                        LastModified = item.LastModified.ToString(),
                        ETag = item.ETag,
                        Size = (ulong)item.Size,
                        BucketName = bucketName,
                        IsDir = false,
                        LastModifiedDateTime = item.LastModified,

                    });
                }
                nextMarker = resultObj.NextMarker;
            } while (resultObj.IsTruncated);

            return Task.FromResult(result);
        }

        /// <summary>
        /// 流式下载文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="callback"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            GetObjectRequest request = new GetObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName
            };
            using (GetObjectResponse response = _client.GetObject(request))
            {
                callback(response.OutputStream);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 下载到指定文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="fileName">文件路径，非文件夹</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

            GetObjectRequest request = new GetObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName
            };
            using (GetObjectResponse response = _client.GetObject(request))
            {
                if (!File.Exists(fileName))
                {
                    response.WriteResponseStreamToFile(fileName);
                }
            }
            return Task.CompletedTask;

        }

        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            HeadObjectRequest request = new HeadObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName
            };
            bool response = _client.HeadObject(request);

            return Task.FromResult(response);
        }
        /// <summary>
        /// 简单复制
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="destBucketName"></param>
        /// <param name="destObjectName"></param>
        /// <returns></returns>
        public Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName = null)
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

            CopyObjectRequest request = new CopyObjectRequest()
            {
                BucketName = destBucketName,
                ObjectKey = destObjectName,
                SourceBucketName = bucketName,
                SourceObjectKey = objectName
            };
            var response = _client.CopyObject(request);
            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// 获取临时文件访问地址
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="expiresInt"></param>
        /// <returns></returns>
        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);

            CreateTemporarySignatureRequest request = new CreateTemporarySignatureRequest()
            {
                BucketName = bucketName,
                Expires = expiresInt,
                ObjectKey = objectName,
                Method = HttpVerb.GET,
            };

            CreateTemporarySignatureResponse response = _client.CreateTemporarySignature(request);

            return Task.FromResult(response.SignUrl);
        }

        /// <summary>
        /// 上传 获取临时文件上传地址
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="expiresInt"></param>
        /// <returns></returns>
        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);

            CreateTemporarySignatureRequest request = new CreateTemporarySignatureRequest()
            {
                BucketName = bucketName,
                Expires = expiresInt,
                ObjectKey = objectName,
                Method = HttpVerb.PUT,
            };

            CreateTemporarySignatureResponse response = _client.CreateTemporarySignature(request);

            return Task.FromResult(response.SignUrl);
        }

        /// <summary>
        /// 文件流 上传文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName,
                InputStream = data,
            };
            PutObjectResponse response = _client.PutObject(request);

            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName,
                FilePath = filePath
            };
            PutObjectResponse response = _client.PutObject(request);
            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            DeleteObjectRequest request = new DeleteObjectRequest()
            {
                BucketName = bucketName,
                ObjectKey = objectName
            };
            DeleteObjectResponse response = _client.DeleteObject(request);
            return Task.FromResult(response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectNames"></param>
        /// <returns></returns>
        public Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames)
        {
            DeleteObjectsRequest request = new DeleteObjectsRequest();
            request.BucketName = bucketName;
            request.Quiet = true;
            foreach (var item in objectNames)
            {
                request.AddKey(item);
            }



            DeleteObjectsResponse response = _client.DeleteObjects(request);
            if (response.DeletedObjects != null && response.DeletedObjects.Count == objectNames.Count)
            {
                return Task.FromResult(true);
            }
            else
            {
                throw new Exception("Some file delete failed.");
            }


        }

        public Task RemovePresignedUrlCache(string bucketName, string objectName)
        {
            return Task.CompletedTask;
        }

        public Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            throw new NotImplementedException();
        }
        public Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            throw new NotImplementedException();
        }

        public Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            throw new NotImplementedException();
        }
    }
}
