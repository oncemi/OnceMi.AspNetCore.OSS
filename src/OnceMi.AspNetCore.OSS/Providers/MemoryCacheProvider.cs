using Microsoft.Extensions.Caching.Memory;
using System;

namespace OnceMi.AspNetCore.OSS
{
    /// <summary>
    /// 默认实现的缓存提供
    /// </summary>
    class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheProvider(IMemoryCache cache)
        {
            this._cache = cache ?? throw new ArgumentNullException(nameof(IMemoryCache));
        }

        public T Get<T>(string key) where T : class
        {
            return _cache.Get<T>(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Set<T>(string key, T value, TimeSpan ts) where T : class
        {
            _cache.Set(key, value, ts);
        }
    }
}
