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
    /// <summary>
    /// 获取IOSSServiceFactory，更具名称创建对应的OSS服务
    /// </summary>
    public class QCloudController : Controller
    {
        private readonly ILogger<QCloudController> _logger;
        private readonly IOSSService _OSSService;
        private readonly string _bucketName = "default-dev";

        public QCloudController(ILogger<QCloudController> logger
            , IOSSServiceFactory ossServiceFactory)
        {
            _logger = logger;
            _OSSService = ossServiceFactory.Create("QCloud");
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BucketExists()
        {
            try
            {
                bool result = await _OSSService.BucketExistsAsync(_bucketName);
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
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

        public async Task<IActionResult> SetBucketAcl()
        {
            try
            {
                var result = await _OSSService.SetBucketAclAsync(_bucketName, AccessMode.PublicRead);
                return Content(result.ToString());
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
                    using (FileStream fs = new FileStream(@"D:\Temp\PHOTO-1.jpg", FileMode.Create, FileAccess.Write))
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

        public async Task<IActionResult> GetObjectByPath()
        {
            try
            {
                await _OSSService.GetObjectAsync(_bucketName, "PHOTO-1.jpg", @"D:\Temp\PHOTO-1.jpg");
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
                await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", @"C:\Users\sysru\Desktop\PHOTO-1.jpg");
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
                byte[] bs = System.IO.File.ReadAllBytes(@"C:\Users\sysru\Desktop\PHOTO-1.jpg");
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

        public IActionResult CleanPresignedObjectCache()
        {
            try
            {
                _OSSService.RemovePresignedUrlCache(_bucketName, "PHOTO-1.jpg");
                return Content("OK");
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

        public async Task<IActionResult> PresignedPutObject()
        {
            try
            {
                var result = await _OSSService.PresignedPutObjectAsync(_bucketName, "PHOTO-2.jpg", 60 * 60 * 3);
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
                var result = await _OSSService.CopyObjectAsync(_bucketName, "PHOTO-1.jpg", null, "old/2.jpg");
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
                var result = await _OSSService.ObjectsExistsAsync(_bucketName, "PHOTO-1.jpg");
                return Content(result.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> SetObjectAcl()
        {
            try
            {
                var result = await _OSSService.SetObjectAclAsync(_bucketName, "PHOTO-1.jpg", AccessMode.Private);
                return Content(result.ToString());
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
                var result = await _OSSService.GetObjectAclAsync(_bucketName, "PHOTO-1.jpg");
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
    }
}
