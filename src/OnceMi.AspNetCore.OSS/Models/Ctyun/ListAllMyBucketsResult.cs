using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OnceMi.AspNetCore.OSS.Models.Ctyun
{
    [XmlRoot("ListAllMyBucketsResult", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
    public class ListAllMyBucketsResult
    {
        [XmlElement("Owner")]
        public owner Owner { get; set; }

        [XmlArray("Buckets")]
        [XmlArrayItem("Bucket")]
        public Bucket[] Buckets { get; set; }
        public class owner
        {
            [XmlElement("ID")]
            public string ID { get; set; }

            [XmlElement("DisplayName")]
            public string DisplayName { get; set; }
        }

        public class Bucket
        {
            [XmlElement("Name")]
            public string Name { get; set; }

            [XmlElement("CreationDate")]
            public string CreationDate { get; set; }
        }
    }
}
