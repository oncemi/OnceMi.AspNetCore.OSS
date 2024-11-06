using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.OSS;
using Sample.AspNetCore.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc.Controllers
{
    public class MinioController : BaseOSSController
    {
        private readonly ILogger<MinioController> _logger;

        public MinioController(ILogger<MinioController> logger
            , IOSSService OSSService) : base(logger)
        {
            _logger = logger;
            _ossService = OSSService;
        }

        /// <summary>
        /// 获取策略
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetPolicy()
        {
            try
            {
                var service = (MinioOSSService)_ossService;
                var result = await service.GetPolicyAsync(_bucketName);
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
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 设置策略
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SetPolicy()
        {
            try
            {
                List<StatementItem> items = new List<StatementItem>();
                items.Add(new StatementItem()
                {
                    Effect = "Allow",
                    Principal = new Principal()
                    {
                        AWS = new List<string>()
                        {
                            "*"
                        },
                    },
                    Action = new List<string>()
                    {
                        "s3:GetObject"
                    },
                    Resource = new List<string>()
                    {
                        $"arn:aws:s3:::{_bucketName}/public*",
                    },
                    IsDelete = false
                });

                var result = await ((MinioOSSService)_ossService).SetPolicyAsync(_bucketName, items);
                if (result)
                {
                    var policies = await ((MinioOSSService)_ossService).GetPolicyAsync(_bucketName);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = policies,
                    });
                }
                else
                {
                    return Json(new ResultObject()
                    {
                        Status = false,
                    });
                }

            }
            catch (Exception ex)
            {
                return Json(new ResultObject()
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 移除策略
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RemovePolicy()
        {
            try
            {
                var service = (MinioOSSService)_ossService;
                var result = await service.RemovePolicyAsync(_bucketName);
                if (result)
                {
                    var policies = await ((MinioOSSService)_ossService).GetPolicyAsync(_bucketName);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = policies,
                    });
                }
                else
                {
                    return Json(new ResultObject()
                    {
                        Status = false,
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ResultObject()
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> PolicyExists()
        {
            try
            {
                var service = (MinioOSSService)_ossService;
                var item = new StatementItem()
                {
                    Effect = "Allow",
                    Principal = new Principal()
                    {
                        AWS = new List<string>()
                        {
                            "*"
                        },
                    },
                    Action = new List<string>()
                    {
                        "s3:GetObject"
                    },
                    Resource = new List<string>()
                    {
                        $"arn:aws:s3:::{_bucketName}/public*",
                    },
                    IsDelete = false
                };
                var result = await service.PolicyExistsAsync(_bucketName, item);
                return Json(new ResultObject()
                {
                    Status = true,
                    Message = result ? "策略存在" : "策略不存在",
                    Data = item,
                });
            }
            catch (Exception ex)
            {
                return Json(new ResultObject()
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
    }
}
