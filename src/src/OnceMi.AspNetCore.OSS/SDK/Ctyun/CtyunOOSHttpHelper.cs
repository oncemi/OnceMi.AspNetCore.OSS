using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace OnceMi.AspNetCore.OSS.SDK.Ctyun
{
    public class CtyunOOSHttpHelper : IOOSHelper
    {
        /// <summary>
        /// 天翼云oos账号
        /// </summary>
        private string _accessKey;

        /// <summary>
        /// 天翼云oos密码
        /// </summary>
        private string _secretKey;

        /// <summary>
        /// 存储桶
        /// </summary>
        private string _bucketName;

        /// <summary>
        /// 天翼云地址
        /// </summary>
        private string _serviceURL;

        private CtyunOOSSignatureV2 signature;

        public void InitOOSHelper(string serviceURL, string accessKey, string secretKey, string bucketName)
        {
            this._serviceURL = serviceURL;
            this._accessKey = accessKey;
            this._secretKey = secretKey;
            this._bucketName = bucketName;

            signature = new CtyunOOSSignatureV2(accessKey, secretKey);

        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="objectname">文件名</param>
        /// <param name="typename">文件类型</param>
        /// <param name="msg">如果成功返回文件存储地址；失败返回错误原因</param>
        /// <returns></returns>
        public bool UploadFile(Stream stream, string objectname, ref String msg)
        {
            Boolean success = true;
            int filelength = 0;
            filelength = (int)stream.Length; //获得文件长度
            byte[] data = new Byte[filelength]; //建立一个字节数组
            stream.Read(data, 0, filelength); //按字节流读取
            HttpRequestHelper httpReqHelper = new HttpRequestHelper(this._serviceURL);
            string uri = this._bucketName + "/" + objectname;
            String datetime = DateTime.Now.ToUniversalTime().ToString("R");
            string typename = GetContentType(objectname);
            var authorization = signature.AuthorizationSignature(HttpRequestHelper.HttpType.PUT.ToString(), typename, datetime, " / " + uri);

            Dictionary<String, string> headers = new Dictionary<string, string>();

            headers.Add("Date", datetime);
            headers.Add("Content-Type", typename);
            headers.Add("Authorization", authorization);
            try
            {
                httpReqHelper.AddRequestHeaders(headers);
                msg = httpReqHelper.HttpRequest(uri, HttpRequestHelper.HttpType.PUT, data);

                if (String.IsNullOrEmpty(msg))
                    msg = this._serviceURL + "/" + uri;

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                success = false;
            }
            return success;
        }
        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();

            switch (extension)
            {
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                // 添加其他文件扩展名和对应的 ContentType
                default:
                    return null; // 未知的 ContentType
            }
        }
    }
}
