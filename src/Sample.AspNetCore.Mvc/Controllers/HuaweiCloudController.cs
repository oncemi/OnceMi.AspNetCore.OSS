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

        public async Task<IActionResult> GetBucketStorageInfo()
        {
            try
            {
                var result = await (_ossService as HaweiOSSService).GetBucketStorageInfoAsync(_bucketName);
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

        public async Task<IActionResult> GetBucketStoragePolicy()
        {
            try
            {
                var result = await (_ossService as HaweiOSSService).GetBucketStoragePolicyAsync(_bucketName);
                return Json(new ResultObject()
                {
                    Status = true,
                    Data = result.ToString(),
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

        public async Task<IActionResult> SetBucketStoragePolicyStandard()
        {
            try
            {
                var result = await (_ossService as HaweiOSSService).SetBucketStoragePolicyAsync(_bucketName, OBS.Model.StorageClassEnum.Standard);
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

        public async Task<IActionResult> SetBucketStoragePolicyCold()
        {
            try
            {
                var result = await (_ossService as HaweiOSSService).SetBucketStoragePolicyAsync(_bucketName, OBS.Model.StorageClassEnum.Cold);
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
