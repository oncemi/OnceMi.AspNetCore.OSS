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
    public class QiniuController : BaseOSSController
    {
        private readonly ILogger<QCloudController> _logger;

        public QiniuController(ILogger<QCloudController> logger
            , IOSSServiceFactory ossServiceFactory) : base(logger)
        {
            _logger = logger;
            _ossService = ossServiceFactory.Create("qiuniu");
        }

        public async Task<IActionResult> GetBucketDomainName()
        {
            try
            {
                List<string> result = await (_ossService as IQiniuOSSService).GetBucketDomainNameAsync(_bucketName);
                return Json(new ResultObject()
                {
                    Status = true,
                    Data = result,
                });
            }
            catch (Exception ex)
            {
                return Json(new ResultObject()
                {
                    Status = false,
                    Message = ex.Message,
                });
            }
        }
    }
}
