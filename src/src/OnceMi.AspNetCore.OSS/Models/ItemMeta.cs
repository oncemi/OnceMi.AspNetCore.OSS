using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.AspNetCore.OSS
{
    public class ItemMeta
    {
        public string ObjectName { get; internal set; }

        public long Size { get; internal set; }

        public DateTime LastModified { get; internal set; }

        public string ETag { get; internal set; }

        public string ContentType { get; internal set; }

        public bool IsEnableHttps { get; set; }

        public Dictionary<string, string> MetaData { get; internal set; }

    }
}
