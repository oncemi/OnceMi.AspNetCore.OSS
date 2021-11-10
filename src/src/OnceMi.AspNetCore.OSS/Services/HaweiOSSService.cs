using OBS;
using OBS.Model;
using OnceMi.AspNetCore.OSS.Interface.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class HaweiOSSService : IBaseOSSService, IHaweiOSSService
    {
        public OSSOptions Options { get; private set; }
        private readonly ObsClient _client = null;
        public ObsClient Context
        {
            get
            {
                return this._client;
            }
        }
        public HaweiOSSService(OSSOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(OSSOptions));

            _client = new ObsClient(Options.AccessKey,Options.SecretKey,new ObsConfig() { Endpoint=Options.Endpoint  });

        }

        #region bucket
        /// <summary>
        /// 创建桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            CreateBucketRequest request = new CreateBucketRequest
            {
                BucketName = bucketName,
            };
            CreateBucketResponse response = _client.CreateBucket(request);
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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
        /// 列表桶
        /// </summary>
        /// <returns></returns>
        public Task<List<Bucket>> ListBucketsAsync()
        {
            ListBucketsRequest request = new ListBucketsRequest();
            ListBucketsResponse response = _client.ListBuckets(request);
            request.IsQueryLocation = true;
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
            
            
          //对应关系？
            return Task.FromResult(new AccessMode());
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
                AccessMode.Default => CannedAclEnum.PublicReadDelivered,
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
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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

            GetObjectMetadataRequest request = new GetObjectMetadataRequest() {
             BucketName=bucketName,
              ObjectKey=objectName,
               VersionId=versionID,
                
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
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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

            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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
            return Task.FromResult(response.StatusCode == System.Net.HttpStatusCode.OK);
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
