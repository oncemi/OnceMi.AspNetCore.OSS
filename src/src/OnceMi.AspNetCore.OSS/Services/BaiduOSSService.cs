using BaiduBce;
using BaiduBce.Auth;
using BaiduBce.Services.Bos;
using BaiduBce.Services.Bos.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class BaiduOSSService : BaseOSSService, IBaiduOSSService
    {
        private readonly BosClient _client = null;

        public BosClient Context
        {
            get
            {
                return this._client;
            }
        }

        public BaiduOSSService(ICacheProvider cache
            , OSSOptions options) : base(cache, options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "The OSSOptions can not null");
            var config = new BceClientConfiguration()
            {
                Credentials = new DefaultBceCredentials(options.AccessKey, options.SecretKey),
                Endpoint = options.Endpoint
            };
            _client = new BosClient(config);
        }

        #region Bucket

        public Task<List<Bucket>> ListBucketsAsync()
        {
            var buckets = _client.ListBuckets();
            if (buckets == null)
            {
                return null;
            }
            if (buckets.Buckets.Count == 0)
            {
                return Task.FromResult(new List<Bucket>());
            }
            var resultList = new List<Bucket>();
            foreach (var item in buckets.Buckets)
            {
                resultList.Add(new Bucket()
                {
                    Location = item.Location,
                    Name = item.Name,
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
            var request = new CreateBucketRequest()
            {
                BucketName = bucketName
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
            return Task.FromResult(result.LocationConstraint);
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
                AccessMode.Default => BosConstants.CannedAcl.PublicReadWrite,
                AccessMode.Private => BosConstants.CannedAcl.Private,
                AccessMode.PublicRead => BosConstants.CannedAcl.PublicRead,
                AccessMode.PublicReadWrite => BosConstants.CannedAcl.PublicReadWrite,
                _ => BosConstants.CannedAcl.PublicReadWrite,
            };
            var request = new SetBucketAclRequest()
            {
                BucketName = bucketName,
                CannedAcl = canned
            };
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
            var result = _client.GetBucketAcl(bucketName).AccessControlList.FirstOrDefault();
            var mode = result.Permission.FirstOrDefault() switch
            {
                BosConstants.CannedAcl.Private => AccessMode.Private,
                BosConstants.CannedAcl.PublicRead => AccessMode.PublicRead,
                BosConstants.CannedAcl.PublicReadWrite => AccessMode.PublicReadWrite,
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
            callback(obj.ObjectContent);
            return Task.CompletedTask;
        }

        public Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            string fullPath = Path.GetFullPath(fileName);
            string parentPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            objectName = FormatObjectName(objectName);
            return GetObjectAsync(bucketName, objectName, (stream) =>
            {
                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                    stream.Dispose();
                    fs.Close();
                }
            }, cancellationToken);
        }

        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            List<Item> result = new List<Item>();
            ListObjectsResponse listObjectsResponse = _client.ListObjects(bucketName, prefix);
            foreach (var item in listObjectsResponse.Contents)
            {
                result.Add(new Item()
                {
                    Key = item.Key,
                    LastModified = item.LastModified.ToString(),
                    ETag = item.ETag,
                    Size = (ulong)item.Size,
                    BucketName = bucketName,
                    IsDir = !string.IsNullOrWhiteSpace(item.Key) && item.Key[^1] == '/',
                    LastModifiedDateTime = item.LastModified
                });
            }
            return Task.FromResult(result);
        }

        public Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            var obj = Task.FromResult(_client.GetObject(bucketName, objectName));
            if (obj != null)
                return Task.FromResult(true);
            else
                return Task.FromResult(false);
        }

        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Get
                , (bucketName, objectName, expiresInt) =>
                {
                    var res = _client.GeneratePresignedUrl(bucketName, objectName, expiresInt);
                    return Task.FromResult(res.AbsoluteUri);
                });
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Put
                , (bucketName, objectName, expiresInt) =>
                {
                    var res = _client.GeneratePresignedUrl(bucketName, objectName, expiresInt);
                    return Task.FromResult(res.AbsoluteUri);
                });
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
            CopyObjectResponse copyObjectResponse = _client.CopyObject(bucketName, objectName, bucketName,
                   destObjectName);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            _client.DeleteObject(bucketName, objectName);
            return Task.FromResult(true);
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
            foreach (var item in objectNames)
            {
                _client.DeleteObject(bucketName, item);
            }
            return Task.FromResult(true);
        }

        public Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            var oldMeta = _client.GetObjectMetadata(bucketName, objectName);
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
                AccessMode.Default => BosConstants.CannedAcl.PublicReadWrite,
                AccessMode.Private => BosConstants.CannedAcl.Private,
                AccessMode.PublicRead => BosConstants.CannedAcl.PublicRead,
                AccessMode.PublicReadWrite => BosConstants.CannedAcl.PublicReadWrite,
                _ => BosConstants.CannedAcl.PublicReadWrite,
            };
            _client.SetBucketAcl(bucketName, canned);
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
            var result = _client.GetBucketAcl(bucketName).AccessControlList.FirstOrDefault();
            var mode = result.Permission.FirstOrDefault() switch
            {
                BosConstants.CannedAcl.Private => AccessMode.Private,
                BosConstants.CannedAcl.PublicRead => AccessMode.PublicRead,
                BosConstants.CannedAcl.PublicReadWrite => AccessMode.PublicReadWrite,
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
    }
}
