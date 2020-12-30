using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class BucketCorsRule
    {
        public string Origin { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Post;

        public string AllowedHeader { get; set; }

        public string ExposeHeader { get; set; }
    }
}
