using Aliyun.OSS;
using COSXML;
using COSXML.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using System;

namespace OnceMi.AspNetCore.OSS
{
    public class OSSServiceFactory : IOSSServiceFactory
    {
        private readonly IOptionsMonitor<OSSOptions> optionsMonitor;
        private readonly ICacheProvider _cache;
        private readonly ILoggerFactory logger;

        public OSSServiceFactory(IOptionsMonitor<OSSOptions> optionsMonitor
            , ICacheProvider provider
            , ILoggerFactory logger)
        {
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException();
            this._cache = provider ?? throw new ArgumentNullException(nameof(IMemoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(ILoggerFactory));
        }

        public IOSSService Create()
        {
            return Create(DefaultOptionName.Name);
        }

        public IOSSService Create(string name)
        {
            #region 参数验证

            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            var options = optionsMonitor.Get(name);
            if (options == null ||
                (options.Provider == OSSProvider.Invalid
                && string.IsNullOrEmpty(options.Endpoint)
                && string.IsNullOrEmpty(options.SecretKey)
                && string.IsNullOrEmpty(options.AccessKey)))
                throw new ArgumentException($"Cannot get option by name '{name}'.");
            if (options.Provider == OSSProvider.Invalid)
                throw new ArgumentNullException(nameof(options.Provider));
            if (string.IsNullOrEmpty(options.Endpoint) && options.Provider != OSSProvider.Qiniu)
                throw new ArgumentNullException(nameof(options.Endpoint), "When your provider is Minio/QCloud/Aliyun/HuaweiCloud, endpoint can not null.");
            if (string.IsNullOrEmpty(options.SecretKey))
                throw new ArgumentNullException(nameof(options.SecretKey), "SecretKey can not null.");
            if (string.IsNullOrEmpty(options.AccessKey))
                throw new ArgumentNullException(nameof(options.AccessKey), "AccessKey can not null.");
            if ((options.Provider == OSSProvider.Minio
                || options.Provider == OSSProvider.QCloud
                || options.Provider == OSSProvider.Qiniu
                || options.Provider == OSSProvider.HuaweiCloud)
                && string.IsNullOrEmpty(options.Region))
            {
                throw new ArgumentNullException(nameof(options.Region), "When your provider is Minio/QCloud/Qiniu/HuaweiCloud, region can not null.");
            }

            #endregion

            switch (options.Provider)
            {
                case OSSProvider.Aliyun:
                    return new AliyunOSSService(_cache, options);
                case OSSProvider.Minio:
                    return new MinioOSSService(_cache, options);
                case OSSProvider.QCloud:
                    return new QCloudOSSService(_cache, options);
                case OSSProvider.Qiniu:
                    return new QiniuOSSService(_cache, options);
                case OSSProvider.HuaweiCloud:
                    return new HaweiOSSService(_cache, options);
                case OSSProvider.BaiduCloud:
                    return new BaiduOSSService(_cache, options);
                default:
                    throw new Exception("Unknow provider type");
            }
        }
    }
}