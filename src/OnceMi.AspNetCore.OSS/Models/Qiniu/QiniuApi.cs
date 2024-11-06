using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS.Models.Qiniu
{
    public class QiniuApi
    {
        public static string GetServiceApi(OSSOptions Options)
        {
            return $"{(Options.IsEnableHttps ? "https" : "http")}://uc.qbox.me";
        }

        public static string GetBaseApi(string host, OSSOptions Options)
        {
            return $"{(Options.IsEnableHttps ? "https" : "http")}://{host}";
        }
    }
}
