using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.AspNetCore.OSS
{
    public class Item
    {
        public string Key { get; internal set; }

        public string LastModified { get; internal set; }

        public string ETag { get; internal set; }

        public ulong Size { get; internal set; }

        public bool IsDir { get; internal set; }

        public string BucketName { get; internal set; }

        public string VersionId { get; set; }

        public DateTime? LastModifiedDateTime { get; internal set; }
    }
}
