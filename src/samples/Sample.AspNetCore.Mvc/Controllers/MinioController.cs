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
    public class MinioController : Controller
    {
        private readonly ILogger<MinioController> _logger;
        private readonly IOSSService _OSSService;
        private readonly string _bucketName = "default-dev";

        public MinioController(ILogger<MinioController> logger
            , IOSSService OSSService)
        {
            _logger = logger;
            _OSSService = OSSService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateBucket()
        {
            try
            {
                bool result = await _OSSService.CreateBucketAsync(_bucketName);
                return Content(result ? $"创建'{_bucketName}'成功。" : $"创建'{_bucketName}'失败。");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> RemoveBucket()
        {
            try
            {
                bool result = await _OSSService.RemoveBucketAsync(_bucketName);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> ListBuckets()
        {
            try
            {
                var result = await _OSSService.ListBucketsAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> ListBucketFiles()
        {
            try
            {
                var result = await _OSSService.ListObjectsAsync(_bucketName);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetObject()
        {
            try
            {
                await _OSSService.GetObjectAsync(_bucketName, "PHOTO-1.jpg", (stream) =>
                {
                    using (FileStream fs = new FileStream("PHOTO-1.jpg", FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                        fs.Close();
                    }
                });
                return Json("OK");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }



        public async Task<IActionResult> PutObject()
        {
            try
            {
                await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", @"PHOTO-1.jpg");
                return Json("OK");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> PutObjectUseStream()
        {
            try
            {
                byte[] bs = System.IO.File.ReadAllBytes("PHOTO-1.jpg");
                using (MemoryStream filestream = new MemoryStream(bs))
                {
                    await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", filestream);
                }

                return Json("OK");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetObjectMetadata()
        {
            try
            {
                var result = await _OSSService.GetObjectMetadataAsync(_bucketName, "PHOTO-1.jpg");
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> PresignedGetObject()
        {
            try
            {
                var result = await _OSSService.PresignedGetObjectAsync(_bucketName, "PHOTO-1.jpg", 60 * 60 * 3);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> CopyObject()
        {
            try
            {
                var result = await _OSSService.CopyObjectAsync(_bucketName, "PHOTO-1.jpg", null, "2.jpg");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> RemoveObject()
        {
            try
            {
                var result = await _OSSService.RemoveObjectAsync(_bucketName, "PHOTO-1.jpg");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> RemoveObjects()
        {
            try
            {
                var result = await _OSSService.RemoveObjectAsync(_bucketName, new List<string>() { "1.gif", "2.gif", "3.jpg" });
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> ObjectsExists()
        {
            try
            {
                var result = await _OSSService.ObjectsExistsAsync(_bucketName, "1.gif");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetPolicy()
        {
            try
            {
                var service = (MinioOSSService)_OSSService;
                var result = await service.GetPolicyAsync(_bucketName);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

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

                var result = await ((MinioOSSService)_OSSService).SetPolicyAsync(_bucketName, items);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> RemovePolicy()
        {
            try
            {
                var service = (MinioOSSService)_OSSService;
                var result = await service.RemovePolicyAsync(_bucketName);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> PolicyExists()
        {
            try
            {
                var service = (MinioOSSService)_OSSService;
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
                        "arn:aws:s3:::berrypi-dev/public*",
                    },
                    IsDelete = false
                };
                var result = await service.PolicyExistsAsync(_bucketName, item);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetBucketAclPrivate()
        {
            try
            {
                var result = await _OSSService.SetBucketAclAsync(_bucketName, AccessMode.Private);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetBucketAclPublicRead()
        {
            try
            {
                var result = await _OSSService.SetBucketAclAsync(_bucketName, AccessMode.PublicRead);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetBucketAclPublicReadWrite()
        {
            try
            {
                var result = await _OSSService.SetBucketAclAsync(_bucketName, AccessMode.PublicReadWrite);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetBucketAcl()
        {
            try
            {
                var result = await _OSSService.GetBucketAclAsync(_bucketName);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetObjectAclPrivate()
        {
            try
            {
                var result = await _OSSService.SetObjectAclAsync(_bucketName, "default-dev/PHOTO-1.jpg", AccessMode.Private);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetObjectAclPublicRead()
        {
            try
            {
                var result = await _OSSService.SetObjectAclAsync(_bucketName, "default-dev/PHOTO-1.jpg", AccessMode.PublicRead);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetObjectAclPublicReadWrite()
        {
            try
            {
                var result = await _OSSService.SetObjectAclAsync(_bucketName, "default-dev/PHOTO-1.jpg", AccessMode.PublicReadWrite);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> GetObjectAcl()
        {
            try
            {
                var result = await _OSSService.GetObjectAclAsync(_bucketName, "default-dev/PHOTO-1.jpg");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> RemoveObjectAcl()
        {
            try
            {
                var result = await _OSSService.RemoveObjectAclAsync(_bucketName, "default-dev/PHOTO-1.jpg");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> DeletePath()
        {
            try
            {
                string path = "001";

                List<Item> items = await _OSSService.ListObjectsAsync(_bucketName, path);
                List<string> removes = new List<string>();
                foreach (var item in items)
                {
                    if (item.Key.StartsWith($"{path}/"))
                    {
                        removes.Add(item.Key);
                    }
                }
                bool result = await _OSSService.RemoveObjectAsync(_bucketName, removes);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
