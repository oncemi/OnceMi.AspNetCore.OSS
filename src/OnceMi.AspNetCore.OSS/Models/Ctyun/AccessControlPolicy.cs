using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OnceMi.AspNetCore.OSS.Models.Ctyun
{
    [XmlRoot("AccessControlPolicy")]
    public class AccessControlPolicy
    {
        [XmlElement("Owner")]
        public owner Owner { get; set; }

        [XmlElement("AccessControlList")]
        public accessControlList AccessControlList { get; set; }
        public class owner
        {
            [XmlElement("ID")]
            public string ID { get; set; }

            [XmlElement("DisplayName")]
            public string DisplayName { get; set; }
        }

        public class accessControlList
        {
            [XmlElement("Grant")]
            public grant Grant { get; set; }
            public class grant
            {
                [XmlElement("Grantee", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
                public grantee Grantee { get; set; }

                [XmlElement("Permission")]
                public string Permission { get; set; }
                public class grantee
                {
                    [XmlAttribute("type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
                    public string Type { get; set; }

                    [XmlElement("URI")]
                    public string URI { get; set; }
                }
            }
        }
    }
}
