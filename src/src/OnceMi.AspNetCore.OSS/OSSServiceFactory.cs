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
        private readonly IMemoryCache _cache;
        private readonly ILoggerFactory logger;

        public OSSServiceFactory(IOptionsMonitor<OSSOptions> optionsMonitor
            , IMemoryCache provider
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
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            var options = optionsMonitor.Get(name);
            if (options == null || (options.Provider == 0 && string.IsNullOrEmpty(options.Endpoint) && string.IsNullOrEmpty(options.SecretKey) && string.IsNullOrEmpty(options.AccessKey)))
                throw new ArgumentException($"Cannot get option by name '{name}'.");
            if(options.Provider == OSSProvider.Invalid)
                throw new ArgumentNullException(nameof(options.Provider));
            if (string.IsNullOrEmpty(options.Endpoint) && options.Provider != OSSProvider.Qiniu)
                throw new ArgumentNullException(nameof(options.Endpoint));
            if (string.IsNullOrEmpty(options.SecretKey))
                throw new ArgumentNullException(nameof(options.SecretKey));
            if (string.IsNullOrEmpty(options.AccessKey))
                throw new ArgumentNullException(nameof(options.AccessKey));

            switch (options.Provider)
            {
                case OSSProvider.Aliyun:
                    {
                        OssClient client = new OssClient(options.Endpoint, options.AccessKey, options.SecretKey);
                        return new AliyunOSSService(client, _cache, options);
                    }
                case OSSProvider.Minio:
                    {
                        MinioClient client = new MinioClient()
                                .WithEndpoint(options.Endpoint)
                                .WithRegion(options.Region)
                                .WithSessionToken(options.SessionToken)
                                .WithCredentials(options.AccessKey, options.SecretKey);

                        if (options.IsEnableHttps)
                        {
                            client = client.WithSSL();
                        }
                        return new MinioOSSService(client.Build(), _cache, options);
                    }
                case OSSProvider.QCloud:
                    {
                        CosXmlConfig config = new CosXmlConfig.Builder()
                          .IsHttps(options.IsEnableHttps)
                          .SetRegion(options.Region)
                          .SetDebugLog(false)
                          .Build();
                        QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(options.AccessKey, options.SecretKey, 600);
                        CosXml cosXml = new CosXmlServer(config, cosCredentialProvider);
                        return new QCloudOSSService(cosXml, _cache, options);
                    }
                case OSSProvider.HaweiCloud:
                    {
                        return new HaweiOSSService(options);
                    }
                case OSSProvider.Qiniu:
                    {
                        return new QiniuOSSService(_cache, options);
                    }
                default:
                    throw new Exception("Unknow provider type");
            }
        }
    }
}