using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.OSS;
using Sample.AspNetCore.Mvc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc.Controllers
{
    /// <summary>
    /// 获取IOSSServiceFactory，更具名称创建对应的OSS服务
    /// </summary>
    public class HuaweiCloudController : BaseOSSController
    {
        private readonly ILogger<HuaweiCloudController> _logger;

        public HuaweiCloudController(ILogger<HuaweiCloudController> logger
            , IOSSServiceFactory ossServiceFactory) : base(logger)
        {
            _logger = logger;
            _ossService = ossServiceFactory.Create("huaweiobs");
        }


    }
}
