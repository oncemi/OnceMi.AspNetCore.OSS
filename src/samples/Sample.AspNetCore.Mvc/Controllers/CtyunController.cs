using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.OSS;
using OnceMi.AspNetCore.OSS.SDK.Ctyun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc.Controllers
{
    public class CtyunController : BaseOSSController
    {
        private readonly ILogger<CtyunController> _logger;

        public CtyunController(ILogger<CtyunController> logger
            , IOSSServiceFactory ossServiceFactory) : base(logger)
        {
            _logger = logger;
            _ossService = ossServiceFactory.Create("ctyunoos");
        }


        public async Task<IActionResult> ListGetBucketLocation()
        {
            try
            {
                var result = await (_ossService as ICtyunOSSService).ListBucketsAsync();
                return Json(true);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpGet("CreateBucket")]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            try
            {
                var result = await (_ossService as ICtyunOSSService).CreateBucketAsync(bucketName);
                return Json(true);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpGet("GetBucketLocal")]
        public async Task<IActionResult> GetBucketLocal(string bucketName)
        {
            try
            {
                var result = await (_ossService as ICtyunOSSService).GetBucketLocationAsync(bucketName);
                return Json(true);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpGet("GetBucketAcl")]
        public async Task<IActionResult> GetBucketAcl(string bucketName)
        {
            try
            {
                var result = await (_ossService as ICtyunOSSService).GetBucketAclAsync(bucketName);
                return Json(true);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}
