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
    public class BaiduController : BaseOSSController
    {
        private readonly ILogger<BaiduController> _logger;

        public BaiduController(ILogger<BaiduController> logger
            , IOSSServiceFactory ossServiceFactory) : base(logger)
        {
            _logger = logger;
            _ossService = ossServiceFactory.Create("baidubos");
        }


        public async Task<IActionResult> ListGetBucketLocation()
        {
            try
            {
                var result = await (_ossService as IBaiduOSSService).GetBucketLocationAsync(_bucketName);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

    }
}
