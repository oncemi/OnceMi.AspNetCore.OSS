using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public interface IAliyunOSSService : IOSSService
    {
        /// <summary>
        /// 获取储存桶地域
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<string> GetBucketLocationAsync(string bucketName);

        /// <summary>
        /// 管理桶跨域访问
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        Task<bool> SetBucketCorsRequestAsync(string bucketName, List<BucketCorsRule> rules);

        /// <summary>
        /// 获取桶外部访问URL
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<string> GetBucketEndpointAsync(string bucketName);
    }
}
