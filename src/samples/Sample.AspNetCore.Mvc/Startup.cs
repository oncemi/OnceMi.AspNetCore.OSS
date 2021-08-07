using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnceMi.AspNetCore.OSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //default minio
            //添加默认对象储存配置信息
            services.AddOSSService(option =>
            {
                option.Provider = OSSProvider.Minio;
                option.Endpoint = "192.168.100.252:9000";
                option.AccessKey = "root";
                option.SecretKey = "uZapbJwj82CoyHcrztA5K9Mx";
                option.IsEnableHttps = false;
                option.IsEnableCache = true;
            });

            //aliyun oss
            //添加名称为‘aliyunoss’的OSS对象储存配置信息
            services.AddOSSService("aliyunoss", option =>
            {
                 option.Provider = OSSProvider.Aliyun;
                 option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
                 option.AccessKey = "LTAI5tS4xmXhF7TnbZaNUV4U";
                 option.SecretKey = "Djyc2QRSbje5tOHFH90bom8ksHp6QM";
                 option.IsEnableCache = true;
            });

            //qcloud oss
            //从配置文件中加载节点为‘OSSProvider’的配置信息
            services.AddOSSService("QCloud", "OSSProvider");

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
