using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OnceMi.AspNetCore.OSS.Models.Ctyun
{
    [XmlRoot("ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class ListBucketResult
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Prefix")]
        public string Prefix { get; set; }

        [XmlElement("Marker")]
        public string Marker { get; set; }

        [XmlElement("MaxKeys")]
        public int MaxKeys { get; set; }

        [XmlElement("IsTruncated")]
        public bool IsTruncated { get; set; }

        [XmlElement("EncodingType")]
        public string EncodingType { get; set; }

        [XmlElement("Contents")]
        public List<ContentItem> Contents { get; set; }
        public class ContentItem
        {
            [XmlElement("Key")]
            public string Key { get; set; }

            [XmlElement("LastModified")]
            public DateTime LastModified { get; set; }

            [XmlElement("ETag")]
            public string ETag { get; set; }

            [XmlElement("Size")]
            public long Size { get; set; }

            [XmlElement("StorageClass")]
            public string StorageClass { get; set; }

            [XmlElement("Owner")]
            public owner Owner { get; set; }
            public class owner
            {
                [XmlElement("ID")]
                public string ID { get; set; }

                [XmlElement("DisplayName")]
                public string DisplayName { get; set; }
            }
        }
    }

}
