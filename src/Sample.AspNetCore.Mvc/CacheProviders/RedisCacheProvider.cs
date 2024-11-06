using FreeRedis;
using OnceMi.AspNetCore.OSS;
using OnceMi.Framework.Util.Json;
using System;

namespace Sample.AspNetCore.Mvc.CacheProviders
{
    /// <summary>
    /// 默认实现的缓存提供
    /// </summary>
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
}
