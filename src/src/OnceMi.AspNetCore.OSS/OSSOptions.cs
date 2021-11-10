using Minio;
using System;

namespace OnceMi.AspNetCore.OSS
{
    public enum OSSProvider
    {
        /// <summary>
        /// 无效
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Minio自建对象储存
        /// </summary>
        Minio = 1,

        /// <summary>
        /// 阿里云OSS
        /// </summary>
        Aliyun = 2,

        /// <summary>
        /// 腾讯云OSS
        /// </summary>
        QCloud = 3,

        /// <summary>
        /// 七牛云 OSS
        /// </summary>
        Qiniu = 4,
        
        HaweiCloud = 5,
    }

    public class OSSOptions
    {
        public OSSProvider Provider { get; set; }

        public string Endpoint { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        private string _region = "us-east-1";

        public string Region
        {
            get
            {
                return _region;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _region = "us-east-1";
                }
                else
                {
                    _region = value;
                }
            }
        }

        public string SessionToken { get; set; }

        public bool IsEnableHttps { get; set; } = true;

        /// <summary>
        /// 是否启用Redis缓存临时URL
        /// </summary>
        public bool IsEnableCache { get; set; } = false;
    }
}