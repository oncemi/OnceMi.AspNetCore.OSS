using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS.Models.Qiniu
{
    class QiniuFullBucketInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string tbl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int itbl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string phy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int uid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zone { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string @global { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string line { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ctime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int oitbl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ouid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string otbl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string oid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int perm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string share_users { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string versioning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string allow_nullkey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string encryption_enabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string not_allow_access_by_tbl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int @private { get; set; }
    }
}
