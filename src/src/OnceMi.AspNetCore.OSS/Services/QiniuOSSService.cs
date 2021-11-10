using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using OnceMi.AspNetCore.OSS.Models.Qiniu;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class QiniuOSSService : IBaseOSSService, IQiniuOSSService
    {
        private readonly IMemoryCache _cache;
        private readonly Mac _mac;
        private readonly Config _config;
        private readonly Auth _auth;
        private readonly HttpManager _http;
        private readonly Dictionary<string, string> _regionNameMap = new Dictionary<string, string>()
        {
            {"z0", "华东(CN_East)"},
            {"z1", "华北(CN_North)"},
            {"z2", "华南(CN_South)"},
            {"na0", "北美(US_North)"},
            {"as0","东南亚(Asia_South)" },
            {"cn-east-2","华东-浙江2" }
        };
        private readonly Dictionary<string, string> _regionZoneMap = new Dictionary<string, string>()
        {
            {"CN_East", "z0"},
            {"CN_North", "z1"},
            {"CN_South", "z2"},
            {"US_North", "na0"},
            {"Asia_South", "as0"},
        };

        public QiniuOSSService(IMemoryCache cache
            , OSSOptions options) : base(cache, options)
        {
            this._cache = cache ?? throw new ArgumentNullException(nameof(IMemoryCache));

            _mac = new Mac(this.Options.AccessKey, this.Options.SecretKey);
            _config = new Config();
            _auth = new Auth(_mac);
            _http = new HttpManager();
            switch (this.Options.Region.ToLower())
            {
                case "cn_east":
                    _config.Zone = Zone.ZONE_CN_East;
                    break;
                case "cn_north":
                    _config.Zone = Zone.ZONE_CN_North;
                    break;
                case "cn_south":
                    _config.Zone = Zone.ZONE_CN_South;
                    break;
                case "us_north":
                    _config.Zone = Zone.ZONE_US_North;
                    break;
                case "asia_south":
                    _config.Zone = Zone.ZONE_AS_Singapore;
                    break;
                default:
                    throw new InvalidOperationException("Incorrect regional configuration. Qiniu oss only supports the following regional configurations：CN_East(华东)/CN_South(华南)/CN_North(华北)/US_North(北美)/Asia_South(东南亚)");
            }
        }

        #region bucket

        public Task<bool> BucketExistsAsync(string bucketName)
        {
            string bucketsUrl = QiniuApi.GetBaseApi(_config.Zone.RsHost, Options) + "/bucket/" + bucketName;
            string token = _auth.CreateManageToken(bucketsUrl);
            HttpResult hr = _http.Get(bucketsUrl, token);
            if (hr.Code == (int)HttpCode.OK && !string.IsNullOrEmpty(hr.Text))
            {
                return Task.FromResult(true);
            }
            else if (hr.Code == 612)
            {
                return Task.FromResult(false);
            }
            else
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get bucket exist status failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Get bucket exist status failed, error code: {hr.Code}, text: {hr.Text}");
            }
        }

        public Task<bool> CreateBucketAsync(string bucketName)
        {
            string apiUrl = $"{QiniuApi.GetServiceApi(Options)}/mkbucketv3/{bucketName}/region/{GetRegionZone(Options.Region)}";
            string token = _auth.CreateManageToken(apiUrl);

            HttpResult hr = _http.Post(apiUrl, token);
            switch (hr.Code)
            {
                case 200:
                    return Task.FromResult(true);
                case 614:
                    throw new BucketExistException($"Bucket '{bucketName}' already exists.");
                default:
                    {
                        if (!string.IsNullOrEmpty(hr.Text))
                        {
                            QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                            throw new Exception($"Create bucket [{bucketName}] failed, error code: {hr.Code}, error msg: {error.error}");
                        }
                        throw new Exception($"Create bucket [{bucketName}] failed, error code: {hr.Code}, text: {hr.Text}");
                    }
            }
        }

        public Task<bool> RemoveBucketAsync(string bucketName)
        {
            string apiUrl = $"{QiniuApi.GetServiceApi(Options)}/drop/{bucketName}";
            string token = _auth.CreateManageToken(apiUrl);

            HttpResult hr = _http.Post(apiUrl, token);
            switch (hr.Code)
            {
                case 200:
                    return Task.FromResult(true);
                default:
                    {
                        if (!string.IsNullOrEmpty(hr.Text))
                        {
                            QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                            throw new Exception($"Remove bucket [{bucketName}] failed, error code: {hr.Code}, error msg: {error.error}");
                        }
                        throw new Exception($"Remove bucket [{bucketName}] failed, error code: {hr.Code}, text: {hr.Text}");
                    }
            }
        }

        public Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            string bucketsUrl = QiniuApi.GetBaseApi(_config.Zone.RsHost, Options) + "/bucket/" + bucketName;
            string token = _auth.CreateManageToken(bucketsUrl);
            HttpResult hr = _http.Get(bucketsUrl, token);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get bucket acl failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Get bucket acl failed, error code: {hr.Code}, text: {hr.Text}");
            }
            QiniuFullBucketInfo ret = JsonUtil.DeserializeObject<QiniuFullBucketInfo>(hr.Text);
            return ret.@private switch
            {
                1 => Task.FromResult(AccessMode.Private),
                _ => Task.FromResult(AccessMode.PublicRead),
            };
        }

        public Task<List<string>> GetBucketDomainNameAsync(string bucketName)
        {
            string apiUrl = $"{QiniuApi.GetServiceApi(Options)}/v2/domains?tbl={bucketName}";
            string token = _auth.CreateManageToken(apiUrl);
            HttpResult hr = _http.Get(apiUrl, token);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get bucket domain name failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Get bucket domain name failed, error code: {hr.Code}, text: {hr.Text}");
            }
            List<string> ret = JsonUtil.DeserializeObject<List<string>>(hr.Text);
            List<string> domains = new List<string>();
            foreach (var item in ret)
            {
                if (item.Contains("clouddn.com", StringComparison.OrdinalIgnoreCase))
                {
                    //七牛云测试域名不支持https
                    domains.Add($"http://{item}");
                }
                else
                {
                    domains.Add($"{(Options.IsEnableHttps ? "https" : "http")}://{item}");
                }
            }
            return Task.FromResult(domains);
        }

        /// <summary>
        /// 列出全部的bucket
        /// 这个方法有些慢，用的是循环遍历，但是七牛并没有在BucketsAsync方法中提供详细的buckets数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<Bucket>> ListBucketsAsync()
        {
            BucketManager bucketManager = new BucketManager(_mac, _config);
            BucketsResult ret = bucketManager.Buckets(false);
            if (ret.Code != (int)HttpCode.OK)
            {
                throw new Exception($"List buckets failed, {ret.ToString()}");
            }
            List<Bucket> buckets = new List<Bucket>();
            if (ret.Result == null || ret.Result.Count == 0)
            {
                return buckets;
            }
            foreach (string bucket in ret.Result)
            {
                var info = await GetBucketInfoAsync(bucket);
                buckets.Add(info);
            }
            return buckets;
        }

        public Task<Bucket> GetBucketInfoAsync(string bucketName)
        {
            string bucketsUrl = QiniuApi.GetBaseApi(_config.Zone.RsHost, Options) + "/bucket/" + bucketName;
            string token = _auth.CreateManageToken(bucketsUrl);
            HttpResult hr = _http.Get(bucketsUrl, token);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get bucket info failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Get bucket info failed, error code: {hr.Code}, text: {hr.Text}");
            }
            QiniuFullBucketInfo ret = JsonUtil.DeserializeObject<QiniuFullBucketInfo>(hr.Text);
            Bucket bucket = new Bucket()
            {
                Location = _regionNameMap.TryGetValue(ret.region, out string region) ? region : "未知",
                Name = ret.tbl,
                CreationDate = TimeUtil.UnixTimeStampToDateTime(ret.ctime).ToString("yyyy-MM-dd HH:mm:ss"),
                Owner = new Owner()
                {
                    Id = ret.uid.ToString(),
                }
            };
            return Task.FromResult(bucket);
        }

        #endregion

        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            BucketManager bucketManager = new BucketManager(_mac, _config);
            string delimiter = "";
            int limit = 1000;
            string marker = "";
            List<Item> items = new List<Item>();
            do
            {
                ListResult listRet = bucketManager.ListFiles(bucketName, prefix, marker, limit, delimiter);
                if (listRet.Code != (int)HttpCode.OK || listRet.Result == null)
                {
                    throw new Exception("List files error: " + listRet.ToString());
                }
                foreach (var item in listRet.Result.Items)
                {
                    items.Add(new Item()
                    {
                        BucketName = bucketName,
                        Key = item.Key,
                        ETag = item.Hash,
                        Size = (ulong)item.Fsize,
                        LastModified = item.EndUser,
                        LastModifiedDateTime = TimeUtil.UnixTimeStampToDateTime(item.PutTime)
                    });
                }
                marker = listRet.Result.Marker;
            } while (!string.IsNullOrEmpty(marker));
            return Task.FromResult(items);
        }

        public Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);

            string bucketsUrl = QiniuApi.GetBaseApi(_config.Zone.RsHost, Options) + "/stat/" + Base64.UrlSafeBase64Encode(bucketName, objectName);
            string token = _auth.CreateManageToken(bucketsUrl);
            HttpResult hr = _http.Get(bucketsUrl, token);
            switch (hr.Code)
            {
                case 200:
                    return Task.FromResult(true);
                case 612:
                    return Task.FromResult(false);
                default:
                    {
                        if (!string.IsNullOrEmpty(hr.Text))
                        {
                            QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                            throw new Exception($"Get object exists status failed, error code: {hr.Code}, error msg: {error.error}");
                        }
                        throw new Exception($"Get object exists status failed, error code: {hr.Code}, text: {hr.Text}");
                    }
            }
        }

        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Get
                , async (bucketName, objectName, expiresInt) =>
               {
                   objectName = FormatObjectName(objectName);
                   List<string> domains = await GetBucketDomainNameAsync(bucketName);
                   if (domains == null || domains.Count == 0)
                   {
                       throw new Exception("Get bucket domain failed.");
                   }
                   AccessMode accessMode = await GetBucketAclAsync(bucketName);
                   if (accessMode == AccessMode.Private)
                   {
                       return DownloadManager.CreatePrivateUrl(_mac, domains[0], objectName, expiresInt);
                   }
                   return $"{domains[0]}/{objectName}";
               });
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            throw new Exception("Qiniu not support presigned a put object url. Please use PutObjectAsync to upload files.");
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            // 上传策略，参见 
            // https://developer.qiniu.com/kodo/manual/put-policy
            PutPolicy putPolicy = new PutPolicy
            {
                Scope = bucketName + ":" + objectName
            };
            putPolicy.SetExpires(3600);
            //获取文件类型
            string contentType = "application/octet-stream";
            if (data is FileStream fileStream)
            {
                string fileName = fileStream.Name;
                if (!string.IsNullOrEmpty(fileName))
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
                }
            }
            else
            {
                new FileExtensionContentTypeProvider().TryGetContentType(objectName, out contentType);
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }

            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(_mac, jstr);
            HttpResult hr = new UploadManager(_config).UploadStream(data, objectName, token, new PutExtra()
            {
                MimeType = contentType,
            });
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Upload object {objectName} failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"pload object {objectName} failed, error code: {hr.Code}, text: {hr.Text}");
            }
            return Task.FromResult(true);
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new Exception("Upload file is not exist.");
            }
            bool result = false;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                result = await PutObjectAsync(bucketName, objectName, fs, cancellationToken);
            }
            return result;
        }

        #region Object acl

        public Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            throw new Exception("Qiniu not support get object acl.");
        }

        public Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            throw new Exception("Qiniu not support set object acl.");
        }

        public Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            throw new Exception("Qiniu not support remove object acl.");
        }

        #endregion

        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            BucketManager bucketManager = new BucketManager(_mac, _config);
            HttpResult hr = bucketManager.Delete(bucketName, objectName);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Delete object metadata failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Download file [{objectName}] failed, error code: {hr.Code}, text: {hr.Text}");
            }
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
            List<string> delObjects = new List<string>();
            foreach (var item in objectNames)
            {
                delObjects.Add(FormatObjectName(item));
            }
            BucketManager bucketManager = new BucketManager(_mac, _config);
            List<string> ops = new List<string>();
            foreach (string key in delObjects)
            {
                string op = bucketManager.DeleteOp(bucketName, key);
                ops.Add(op);
            }

            BatchResult ret = bucketManager.Batch(ops);
            if (ret.Code != (int)HttpCode.OK)
            {
                if (ret.Result != null && ret.Result.Count > 0)
                {
                    List<string> failedErrs = new List<string>();
                    foreach (var item in ret.Result)
                    {
                        if (item.Code != (int)HttpCode.OK && item.Data != null && !string.IsNullOrEmpty(item.Data.Error))
                        {
                            failedErrs.Add(item.Data.Error);
                        }
                    }
                    if (failedErrs.Count > 0)
                    {
                        throw new Exception($"Batch delete files failed, error code: {ret.Code}, error msg: {string.Join(';', failedErrs)}");
                    }
                }
                throw new Exception($"Batch delete files failed, error code: {ret.Code}, text: {ret.Text}");
            }
            return Task.FromResult(true);
        }

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
            BucketManager bucketManager = new BucketManager(_mac, _config);
            HttpResult hr = bucketManager.Copy(bucketName, objectName, destBucketName, destObjectName, true);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Copy object to {destBucketName}/{destObjectName} failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Copy object to {destBucketName}/{destObjectName} failed, error code: {hr.Code}, text: {hr.Text}");
            }
            return Task.FromResult(true);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            List<string> domains = await GetBucketDomainNameAsync(bucketName);
            if (domains == null || domains.Count == 0)
            {
                throw new Exception("Get bucket domain failed.");
            }
            string privateUrl = DownloadManager.CreatePrivateUrl(_mac, domains[0], objectName);
            if (string.IsNullOrEmpty(privateUrl))
            {
                throw new Exception("Get download url failed.");
            }
            var hr = _http.Get(privateUrl, null, true);
            if (hr.Code != (int)HttpCode.OK)
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get object metadata failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Download file [{objectName}] failed, error code: {hr.Code}, text: {hr.Text}");
            }
            MemoryStream ms = new MemoryStream(hr.Data);
            callback(ms);
            }

        public Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            return GetObjectAsync(bucketName, objectName, (stream) =>
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                    fs.Close();
                    stream.Dispose();
                }
            }, cancellationToken);
        }

        public Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);

            string bucketsUrl = QiniuApi.GetBaseApi(_config.Zone.RsHost, Options) + "/stat/" + Base64.UrlSafeBase64Encode(bucketName, objectName);
            string token = _auth.CreateManageToken(bucketsUrl);
            HttpResult hr = _http.Get(bucketsUrl, token);
            if (hr.Code != (int)HttpCode.OK || string.IsNullOrEmpty(hr.Text))
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Get object metadata failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Get object metadata failed, error code: {hr.Code}, text: {hr.Text}");
            }
            QiniuObjectMetadata metadata = JsonUtil.DeserializeObject<QiniuObjectMetadata>(hr.Text);
            return Task.FromResult(new ItemMeta()
            {
                ObjectName = objectName,
                Size = metadata.fsize,
                LastModified = TimeUtil.UnixTimeStampToDateTime(metadata.putTime),
                ETag = metadata.hash,
                ContentType = metadata.mimeType,
                IsEnableHttps = Options.IsEnableHttps,
            });
        }

        public Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            int aclVal = mode switch
            {
                AccessMode.PublicRead => 0,
                AccessMode.Private => 1,
                _ => throw new Exception($"Qiniu oss not support access mode: {mode}")
            };
            string apiUrl = $"{QiniuApi.GetServiceApi(Options)}/private";
            byte[] data = Encoding.UTF8.GetBytes($"bucket={bucketName}&private={aclVal}");
            string token = new Auth(_mac).CreateManageToken(apiUrl, data);

            HttpResult hr = _http.PostForm(apiUrl, data, token);
            if (hr.Code == (int)HttpCode.OK)
            {
                return Task.FromResult(true);
            }
            else
            {
                if (!string.IsNullOrEmpty(hr.Text))
                {
                    QiniuError error = JsonUtil.DeserializeObject<QiniuError>(hr.Text);
                    throw new Exception($"Set bucket acl failed, error code: {hr.Code}, error msg: {error.error}");
                }
                throw new Exception($"Set bucket acl failed, error code: {hr.Code}, text: {hr.Text}");
            }
        }

        #region private

        /// <summary>
        /// 根据ZoneID获取地域ID，如z0
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        private string GetRegionZone(string zone)
        {
            foreach (var item in _regionZoneMap)
            {
                if (item.Key.Equals(zone, StringComparison.OrdinalIgnoreCase))
                {   
                    return item.Value;
                }
            }
            throw new InvalidOperationException("Incorrect regional configuration. Qiniu oss only supports the following regional configurations：CN_East(华东)/CN_South(华南)/CN_North(华北)/US_North(北美)");
        }

        #endregion
    }
}
