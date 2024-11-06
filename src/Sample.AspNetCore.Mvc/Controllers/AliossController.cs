using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc.Controllers
{
    public class AliossController : BaseOSSController
    {
        private readonly ILogger<AliossController> _logger;

        public AliossController(ILogger<AliossController> logger
            , IOSSServiceFactory ossServiceFactory) : base(logger)
        {
            _logger = logger;
            _ossService = ossServiceFactory.Create("aliyunoss");
        }


        public async Task<IActionResult> ListGetBucketLocation()
        {
            try
            {
                var result = await (_ossService as IAliyunOSSService).GetBucketLocationAsync(_bucketName);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetBucketWebsiteUrl()
        {
            try
            {
                var result = await (_ossService as IAliyunOSSService).GetBucketEndpointAsync(_bucketName);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}
