using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.AspNetCore.OSS
{
    class PresignedUrlCache
    {
        public string Name { get; set; }

        public string BucketName { get; set; }

        public long CreateTime { get; set; }

        public string Url { get; set; }

        public PresignedObjectType Type { get; set; }
    }
}
