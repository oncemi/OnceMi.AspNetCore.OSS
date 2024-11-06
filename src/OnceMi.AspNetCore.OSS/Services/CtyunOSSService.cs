using OnceMi.AspNetCore.OSS.Models.Ctyun;
using OnceMi.AspNetCore.OSS.SDK.Ctyun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OnceMi.AspNetCore.OSS
{
    /// <summary>
    /// 天翼云OOS对接
    /// build by zjy
    /// QQ：365015
    /// </summary>
    public class CtyunOSSService : BaseOSSService, ICtyunOSSService
    {
        private CtyunOOSSignatureV2 signature;
        /// <summary>
        /// 天翼云地址
        /// </summary>
        private string _serviceURL;

        public CtyunOSSService(ICacheProvider cache
            , OSSOptions options) : base(cache, options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "The OSSOptions can not null");
            signature = new CtyunOOSSignatureV2(options.AccessKey, options.SecretKey);
            _serviceURL = options.Endpoint;
        }
        #region 辅助方法
        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();

            switch (extension)
            {
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                // 添加其他文件扩展名和对应的 ContentType
                default:
                    return null; // 未知的 ContentType
            }
        }
        private T DeserializeFromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                try
                {
                    return (T)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    return default;
                }
            }
        }
        #endregion
        #region Bucket

        public async Task<List<Bucket>> ListBucketsAsync()
        {
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, "/");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("", HttpRequestHelper.HttpType.GET, null);
            var buckets = DeserializeFromXml<ListAllMyBucketsResult>(msg);
            if (buckets == null)
            {
                return null;
            }
            if (buckets.Buckets.Length == 0)
            {
                return new List<Bucket>();
            }
            var resultList = new List<Bucket>();
            foreach (var item in buckets.Buckets)
            {
                var location = await GetBucketLocationAsync(item.Name);
                resultList.Add(new Bucket()
                {
                    Location = location,
                    Name = item.Name,
                    CreationDate = item.CreationDate,
                });
            }
            return resultList;
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var buckets = await ListBucketsAsync();
            var result = buckets.Any(x => x.Name == bucketName);
            return result;
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
            // 创建存储空间。
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.PUT.ToString(), "", datetime, $"/{bucketName}/");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("", HttpRequestHelper.HttpType.PUT, null);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.DELETE.ToString(), "", datetime, $"/{bucketName}/");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("", HttpRequestHelper.HttpType.DELETE, null);
            return Task.FromResult(true);
        }

        public async Task<string> GetBucketLocationAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, $"/{bucketName}/?location");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("?location", HttpRequestHelper.HttpType.GET, null);
            var result = DeserializeFromXml<BucketConfiguration>(msg);
            return result.MetadataLocationConstraint.Location;
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
            throw new Exception("暂不支持");
        }

        /// <summary>
        /// 获取储存桶的访问权限
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public async Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, $"/{bucketName}/?acl");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("?acl", HttpRequestHelper.HttpType.GET, null);
            var result = DeserializeFromXml<AccessControlPolicy>(msg);
            var mode = result.AccessControlList.Grant.Permission.ToString() switch
            {
                "READ" => AccessMode.PublicRead,
                "FULL_CONTROL" => AccessMode.PublicReadWrite,
                _ => AccessMode.Private,
            };
            return mode;
        }

        #endregion

        #region Object

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            using (var httpClient = new HttpClient())
            {
                string datetime = DateTime.UtcNow.ToString("R");
                var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, $"/{bucketName}/{objectName}");

                httpClient.DefaultRequestHeaders.Add("Date", datetime);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

                string url = $"https://{bucketName}.{this._serviceURL}/{objectName}";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    callback(await response.Content.ReadAsStreamAsync());
                }
                else
                {
                    throw new Exception("对象不存在");
                }
            }
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

        public async Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, $"/{bucketName}/");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("", HttpRequestHelper.HttpType.GET, null);
            var listObjectsResponse = DeserializeFromXml<ListBucketResult>(msg);

            List<Item> result = new List<Item>();
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
            return result;
        }

        public async Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            using (var httpClient = new HttpClient())
            {
                string datetime = DateTime.UtcNow.ToString("R");
                var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.GET.ToString(), "", datetime, $"/{bucketName}/{objectName}");

                httpClient.DefaultRequestHeaders.Add("Date", datetime);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

                string url = $"https://{bucketName}.{this._serviceURL}/{objectName}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response != null)
                    return true;
                else
                    return false;
            }
        }

        public async Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            throw new Exception("暂不支持");
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            throw new Exception("暂不支持");
        }

        public Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            int filelength = 0;
            filelength = (int)data.Length; //获得文件长度
            byte[] b = new Byte[filelength]; //建立一个字节数组
            data.Read(b, 0, filelength); //按字节流读取
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string uri = bucketName + "/" + objectName;
            String datetime = DateTime.Now.ToUniversalTime().ToString("R");
            string typename = GetContentType(objectName);
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.PUT.ToString(), typename, datetime, "/" + uri);

            Dictionary<String, string> headers = new Dictionary<string, string>();

            headers.Add("Date", datetime);
            headers.Add("Content-Type", typename);
            headers.Add("Authorization", authorization);
            try
            {
                httpReqHelper.AddRequestHeaders(headers);
                var msg = httpReqHelper.HttpRequest(uri, HttpRequestHelper.HttpType.PUT, b);
                return Task.FromResult(true);
            }
            catch (Exception ex)
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
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                int filelength = 0;
                filelength = (int)fs.Length; //获得文件长度
                byte[] b = new Byte[filelength]; //建立一个字节数组
                fs.Read(b, 0, filelength); //按字节流读取
                HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
                string uri = bucketName + "/" + objectName;
                String datetime = DateTime.Now.ToUniversalTime().ToString("R");
                string typename = GetContentType(objectName);
                var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.PUT.ToString(), typename, datetime, "/" + uri);

                Dictionary<String, string> headers = new Dictionary<string, string>();

                headers.Add("Date", datetime);
                headers.Add("Content-Type", typename);
                headers.Add("Authorization", authorization);
                try
                {
                    httpReqHelper.AddRequestHeaders(headers);
                    var msg = httpReqHelper.HttpRequest(uri, HttpRequestHelper.HttpType.PUT, b);
                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(false);
                }
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
            using (var httpClient = new HttpClient())
            {
                string datetime = DateTime.UtcNow.ToString("R");
                string source = $"/{bucketName}/{objectName}";
                string authorization = signature.AuthorizationSignature("PUT", "", datetime, $"/{destBucketName}/{destObjectName}");

                httpClient.DefaultRequestHeaders.Add("Date", datetime);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

                var response = httpClient.PutAsync($"https://{destBucketName}.{this._serviceURL}/{destObjectName}", new StringContent(string.Empty));

                if (response.Result.IsSuccessStatusCode)
                {
                    response = httpClient.PutAsync($"https://{destBucketName}.{this._serviceURL}/{destObjectName}?copy-source={Uri.EscapeDataString(source)}", new StringContent(string.Empty));

                    if (response.Result.IsSuccessStatusCode)
                    {
                        return Task.FromResult(true);
                    }
                }

                return Task.FromResult(false);
            }
        }

        public Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            HttpRequestHelper httpReqHelper = new HttpRequestHelper($"https://{bucketName}.{this._serviceURL}");
            string datetime = DateTime.Now.ToUniversalTime().ToString("R");
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.DELETE.ToString(), "", datetime, $"/{bucketName}/{objectName}");
            Dictionary<String, string> headers = new Dictionary<string, string>();
            headers.Add("Date", datetime);
            headers.Add("Authorization", authorization);
            httpReqHelper.AddRequestHeaders(headers);
            var msg = httpReqHelper.HttpRequest("{objectName}", HttpRequestHelper.HttpType.DELETE, null);
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
                RemoveObjectAsync(bucketName, item);
            }
            return Task.FromResult(true);
        }

        public async Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            throw new Exception("暂不支持");
        }

        public async Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            throw new Exception("暂不支持");
        }

        public async Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            throw new Exception("暂不支持");
        }

        public async Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            throw new Exception("暂不支持");
        }
        #endregion
    }
}
