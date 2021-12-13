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
    public class BaseOSSController : Controller
    {
        private readonly ILogger _logger;
        public IOSSService _ossService;
        public const string _bucketName = @"qtest-1213";

        public const string _objectFilePath = @"/1.jpeg";
        public const string _copyObjectDestFilePath = @"/1_copy.jpeg";
        public const string _uploadFilePath = @"D:\OSSTest\001.jpg";
        public const string _downloadFilePath = @"D:\OSSTest\001_download.jpg";

        public BaseOSSController(ILogger logger)
        {
            _logger = logger;
        }

        #region Bucket

        public async Task<IActionResult> BucketExists()
        {
            try
            {
                bool result = await _ossService.BucketExistsAsync(_bucketName);
                return Json(new ResultObject()
                {
                    Status = result
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

        public async Task<IActionResult> CreateBucket()
        {
            try
            {
                bool result = await _ossService.CreateBucketAsync(_bucketName);
                return Json(new ResultObject()
                {
                    Status = result,
                    Message = result ? $"创建储存桶'{_bucketName}'成功。" : $"创建储存桶'{_bucketName}'失败。",
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

        public async Task<IActionResult> RemoveBucket()
        {
            try
            {
                bool result = await _ossService.RemoveBucketAsync(_bucketName);
                return Json(new ResultObject()
                {
                    Status = result,
                    Message = result ? $"移除储存桶'{_bucketName}'成功。" : $"移除储存桶'{_bucketName}'失败。",
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

        public async Task<IActionResult> ListBuckets()
        {
            try
            {
                var result = await _ossService.ListBucketsAsync();
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

        #endregion

        #region Objects
        public async Task<IActionResult> ListObjects()
        {
            try
            {
                var result = await _ossService.ListObjectsAsync(_bucketName);
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

        public async Task<IActionResult> GetObject()
        {
            try
            {
                await _ossService.GetObjectAsync(_bucketName, _objectFilePath, (stream) =>
                {
                    using (FileStream fs = new FileStream(_downloadFilePath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                        fs.Close();
                        stream.Dispose();
                    }
                });
                if (System.IO.File.Exists(_downloadFilePath))
                {
                    return Json(new ResultObject()
                    {
                        Status = true,
                        Message = $"文件【{_objectFilePath}】下载成功，路径：{_downloadFilePath}"
                    });
                }
                else
                {
                    return Json(new ResultObject()
                    {
                        Status = false,
                        Message = $"文件【{_objectFilePath}】下载失败"
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

        public async Task<IActionResult> PutObject()
        {
            try
            {
                await _ossService.PutObjectAsync(_bucketName, _objectFilePath, _uploadFilePath);
                if (await _ossService.ObjectsExistsAsync(_bucketName, _objectFilePath))
                {
                    return Json(new ResultObject()
                    {
                        Status = true,
                        Message = $"文件【{_uploadFilePath}】上传成功，路径：{_objectFilePath}"
                    });
                }
                else
                {
                    return Json(new ResultObject()
                    {
                        Status = false,
                        Message = $"文件【{_uploadFilePath}】上传失败"
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

        public async Task<IActionResult> PutObjectUseStream()
        {
            try
            {
                using (var filestream = System.IO.File.OpenRead(_uploadFilePath))
                {
                    await _ossService.PutObjectAsync(_bucketName, _objectFilePath, filestream);
                }
                if (await _ossService.ObjectsExistsAsync(_bucketName, _objectFilePath))
                {
                    return Json(new ResultObject()
                    {
                        Status = true,
                        Message = $"文件【{_uploadFilePath}】上传成功，路径：{_objectFilePath}"
                    });
                }
                else
                {
                    return Json(new ResultObject()
                    {
                        Status = false,
                        Message = $"文件【{_uploadFilePath}】上传失败"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Json(new ResultObject()
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> GetObjectMetadata()
        {
            try
            {
                var result = await _ossService.GetObjectMetadataAsync(_bucketName, _objectFilePath);
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

        public async Task<IActionResult> CopyObject()
        {
            try
            {
                var result = await _ossService.CopyObjectAsync(_bucketName, _objectFilePath, null, _copyObjectDestFilePath);
                return Json(new ResultObject()
                {
                    Status = result,
                    Message = !result ? $"复制文件【{_objectFilePath}】到【{_copyObjectDestFilePath}】失败" : $"复制文件【{_objectFilePath}】到【{_copyObjectDestFilePath}】成功"
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

        public async Task<IActionResult> RemoveObject()
        {
            try
            {
                var result = await _ossService.RemoveObjectAsync(_bucketName, _objectFilePath);
                return Json(new ResultObject()
                {
                    Status = result,
                    Message = !result ? $"删除文件/文件夹【{_objectFilePath}】失败" : $"删除文件/文件夹【{_objectFilePath}】成功"
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

        public async Task<IActionResult> RemoveObjects()
        {
            try
            {
                var result = await _ossService.RemoveObjectAsync(_bucketName, new List<string>() { _objectFilePath, _copyObjectDestFilePath });
                return Json(new ResultObject()
                {
                    Status = result,
                    Message = !result ? $"删除文件/文件夹【{_objectFilePath}、{_copyObjectDestFilePath}】失败" : $"删除文件/文件夹【{_objectFilePath}、{_copyObjectDestFilePath}】成功"
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

        public async Task<IActionResult> ObjectsExists()
        {
            try
            {
                var result = await _ossService.ObjectsExistsAsync(_bucketName, _objectFilePath);
                return Json(new ResultObject()
                {
                    Status = true,
                    Message = !result ? $"文件/文件夹【{_objectFilePath}】不存在" : $"文件/文件夹【{_objectFilePath}】存在"
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

        #endregion

        #region 访问权限

        public async Task<IActionResult> SetBucketAclPrivate()
        {
            try
            {
                var result = await _ossService.SetBucketAclAsync(_bucketName, AccessMode.Private);
                if (result)
                {
                    var acl = await _ossService.GetBucketAclAsync(_bucketName);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> SetBucketAclPublicRead()
        {
            try
            {
                var result = await _ossService.SetBucketAclAsync(_bucketName, AccessMode.PublicRead);
                if (result)
                {
                    var acl = await _ossService.GetBucketAclAsync(_bucketName);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> SetBucketAclPublicReadWrite()
        {
            try
            {
                var result = await _ossService.SetBucketAclAsync(_bucketName, AccessMode.PublicReadWrite);
                if (result)
                {
                    var acl = await _ossService.GetBucketAclAsync(_bucketName);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> GetBucketAcl()
        {
            try
            {
                var result = await _ossService.GetBucketAclAsync(_bucketName);
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
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> SetObjectAclPrivate()
        {
            try
            {
                var result = await _ossService.SetObjectAclAsync(_bucketName, _objectFilePath, AccessMode.Private);
                if (result)
                {
                    var acl = await _ossService.GetObjectAclAsync(_bucketName, _objectFilePath);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> SetObjectAclPublicRead()
        {
            try
            {
                var result = await _ossService.SetObjectAclAsync(_bucketName, _objectFilePath, AccessMode.PublicRead);
                if (result)
                {
                    var acl = await _ossService.GetObjectAclAsync(_bucketName, _objectFilePath);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> SetObjectAclPublicReadWrite()
        {
            try
            {
                var result = await _ossService.SetObjectAclAsync(_bucketName, _objectFilePath, AccessMode.PublicReadWrite);
                if (result)
                {
                    var acl = await _ossService.GetObjectAclAsync(_bucketName, _objectFilePath);
                    return Json(new ResultObject()
                    {
                        Status = result,
                        Data = acl.ToString(),
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

        public async Task<IActionResult> GetObjectAcl()
        {
            try
            {
                var result = await _ossService.GetObjectAclAsync(_bucketName, _objectFilePath);
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
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> RemoveObjectAcl()
        {
            try
            {
                var result = await _ossService.RemoveObjectAclAsync(_bucketName, _objectFilePath);
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
                    Message = ex.Message
                });
            }
        }

        #endregion

        public async Task<IActionResult> PresignedGetObject()
        {
            try
            {
                var result = await _ossService.PresignedGetObjectAsync(_bucketName, _objectFilePath, 7 * 24 * 3600);
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

        public async Task<IActionResult> PresignedPutObject()
        {
            try
            {
                var result = await _ossService.PresignedPutObjectAsync(_bucketName, _objectFilePath, 60 * 60 * 3);
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

        public async Task<IActionResult> RemovePresignedUrlCache()
        {
            try
            {
                await _ossService.RemovePresignedUrlCache(_bucketName, _objectFilePath);
                return Json(new ResultObject()
                {
                    Status = true,
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
