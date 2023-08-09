using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnceMi.AspNetCore.OSS.SDK.Ctyun
{
    public interface IOOSHelper
    {
        /// <summary>
        /// 设置配置信息
        /// </summary>
        /// <param name="serviceURL">云地址</param>
        /// <param name="accessKey">oos账号</param>
        /// <param name="secretKey">oos密码</param>
        /// <param name="bucketName">存储桶</param>
        void InitOOSHelper(string serviceURL, string accessKey, string secretKey, string bucketName);


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="objectname">商户号+文件名称，例如：99999/1.JPG</param>
        /// <param name="msg">如果成功返回文件存储地址；失败返回错误原因</param>
        /// <returns></returns>
        Boolean UploadFile(Stream inputStream, string objectname, ref string msg);
    }
}
