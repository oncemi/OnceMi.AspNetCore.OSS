using Minio;
using System;

namespace OnceMi.AspNetCore.OSS
{
    public enum OSSProvider
    {
        Minio,
        Aliyun,
        QCloud
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
        ///  «∑Ò∆Ù”√Redisª∫¥Ê¡Ÿ ±URL
        /// </summary>
        public bool IsEnableCache { get; set; } = false;
    }
}