using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OnceMi.AspNetCore.OSS.Models.Ctyun
{
    [XmlRoot("BucketConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class BucketConfiguration
    {
        [XmlElement("MetadataLocationConstraint")]
        public metadataLocationConstraint MetadataLocationConstraint { get; set; }

        [XmlElement("DataLocationConstraint")]
        public dataLocationConstraint DataLocationConstraint { get; set; }
        public class metadataLocationConstraint
        {
            [XmlElement("Location")]
            public string Location { get; set; }
        }

        public class dataLocationConstraint
        {
            [XmlElement("Type")]
            public string Type { get; set; }
        }
    }
}
