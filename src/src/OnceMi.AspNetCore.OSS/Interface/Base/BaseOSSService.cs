using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public abstract class BaseOSSService
    {
        protected readonly IMemoryCache _cache;
        public OSSOptions Options { get; private set; }

        public BaseOSSService(IMemoryCache cache, OSSOptions options)
        {
            this._cache = cache ?? throw new ArgumentNullException(nameof(IMemoryCache));
            this.Options = options ?? throw new ArgumentNullException(nameof(OSSOptions));
        }

        internal virtual string BuildPresignedObjectCacheKey(string bucketName, string objectName, PresignedObjectType type)
        {
            return Encrypt.MD5($"{this.GetType().FullName}_{bucketName}_{objectName}_{type.ToString().ToUpper()}");
        }

        internal virtual string FormatObjectName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || objectName == "/")
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (objectName.StartsWith('/'))
            {
                return objectName.TrimStart('/');
            }
            return objectName;
        }

        public virtual Task RemovePresignedUrlCache(string bucketName, string objectName)
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

        internal virtual async Task<string> PresignedObjectAsync(string bucketName
            , string objectName
            , int expiresInt
            , PresignedObjectType type
            , Func<string, string, int, Task<string>> PresignedFunc)
        {
            try
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
                const int minExpiresInt = 600;

                if (Options.IsEnableCache && expiresInt > minExpiresInt)
                {
                    string key = BuildPresignedObjectCacheKey(bucketName, objectName, type);
                    var cacheResult = _cache.Get<PresignedUrlCache>(key);
                    PresignedUrlCache cache = cacheResult != null ? cacheResult : null;
                    //Unix时间
                    long nowTime = TimeUtil.Timestamp();
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
                    else
                    {
                        string presignedUrl = await PresignedFunc(bucketName, objectName, expiresInt);
                        if (string.IsNullOrEmpty(presignedUrl))
                        {
                            throw new Exception("Presigned object url failed.");
                        }
                        PresignedUrlCache urlCache = new PresignedUrlCache()
                        {
                            Url = presignedUrl,
                            CreateTime = nowTime,
                            Name = objectName,
                            BucketName = bucketName,
                            Type = type
                        };
                        int randomSec = new Random().Next(0, 10);
                        _cache.Set(key, urlCache, TimeSpan.FromSeconds(expiresInt + randomSec));
                        return urlCache.Url;
                    }
                }
                else
                {
                    string presignedUrl = await PresignedFunc(bucketName, objectName, expiresInt);
                    if (string.IsNullOrEmpty(presignedUrl))
                    {
                        throw new Exception("Presigned object url failed.");
                    }
                    return presignedUrl;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Presigned {(type == PresignedObjectType.Get ? "get" : "put")} url for object '{objectName}' from {bucketName} failed. {ex.Message}", ex);
            }
        }
    }
}
