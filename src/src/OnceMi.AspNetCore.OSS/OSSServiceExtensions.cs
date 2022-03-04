using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OnceMi.AspNetCore.OSS
{
    public static class OSSServiceExtensions
    {
        /// <summary>
        /// 从配置文件中加载默认配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string key)
        {
            return services.AddOSSService(DefaultOptionName.Name, key);
        }

        /// <summary>
        /// 从配置文件中加载
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string name, string key)
        {
            ServiceProvider provider = services.BuildServiceProvider();
            IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(IConfiguration));
            }
            provider.Dispose();

            IConfigurationSection section = configuration.GetSection(key);
            if (!section.Exists())
            {
                throw new Exception($"Config file not exist '{key}' section.");
            }
            OSSOptions options = section.Get<OSSOptions>();
            if (options == null)
            {
                throw new Exception($"Get OSS option from config file failed.");
            }
            return services.AddOSSService(name, o =>
             {
                 o.AccessKey = options.AccessKey;
                 o.Endpoint = options.Endpoint;
                 o.IsEnableCache = options.IsEnableCache;
                 o.IsEnableHttps = options.IsEnableHttps;
                 o.Provider = options.Provider;
                 o.Region = options.Region;
                 o.SecretKey = options.SecretKey;
                 o.SessionToken = options.SessionToken;
             });
        }

        /// <summary>
        /// 配置默认配置
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, Action<OSSOptions> option)
        {
            return services.AddOSSService(DefaultOptionName.Name, option);
        }

        /// <summary>
        /// 根据名称配置
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string name, Action<OSSOptions> option)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            services.Configure(name, option);
            //对于IOSSServiceFactory只需要注入一次
            if (!services.Any(p => p.ServiceType == typeof(IOSSServiceFactory)))
            {
                using (var provider = services.BuildServiceProvider())
                {
                    OSSOptions optVal = provider.GetRequiredService<IOptionsMonitor<OSSOptions>>().Get(name);
                    if (optVal == null)
                    {
                        throw new Exception("Can not get OSSOptions from service collection");
                    }
                    if (optVal.UseCustumCacheProvider && optVal.IsEnableCache)
                    {
                        ICacheProvider cacheProvider = provider.GetService<ICacheProvider>();
                        if (cacheProvider == null)
                        {
                            throw new Exception("Unable to get interface 'ICacheProvider's implementation.");
                        }
                    }
                    else
                    {
                        services.AddMemoryCache();
                        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();
                    }
                }
                services.TryAddSingleton<IOSSServiceFactory, OSSServiceFactory>();
            }
            //
            services.TryAddScoped(sp => sp.GetRequiredService<IOSSServiceFactory>().Create(name));
            return services;
        }
    }
}