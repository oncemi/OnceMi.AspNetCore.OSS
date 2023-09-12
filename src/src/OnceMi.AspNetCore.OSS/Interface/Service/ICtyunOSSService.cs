using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public interface ICtyunOSSService : IOSSService
    {
        /// <summary>
        /// 获取储存桶地域
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<string> GetBucketLocationAsync(string bucketName);

    }
}
