using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OnceMi.AspNetCore.OSS.SDK.Ctyun
{
    /// <summary>
    /// 天翼云OOS签名V2版本
    /// </summary>
    public class CtyunOOSSignatureV2
    {
        /// <summary>
        /// 账号
        /// </summary>
        private String secretkey;

        /// <summary>
        /// 秘钥
        /// </summary>
        private string accessKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretkey">秘钥</param>
        /// <param name="accessKey">账号</param>
        /// <param name="region"></param>
        public CtyunOOSSignatureV2(string accessKey, String secretkey)
        {
            this.secretkey = secretkey;
            this.accessKey = accessKey;
        }

        /// <summary>

        /// HMACSHA1算法加密并返回ToBase64String

        /// </summary>

        /// <param name="strText">签名参数字符串</param>

        /// <param name="strKey">密钥参数</param>

        /// <returns>返回一个签名值(即哈希值)</returns>

        private string ToBase64hmac(string strKey, string strText)

        {

            HMACSHA1 myHMACSHA1 = new HMACSHA1(Encoding.UTF8.GetBytes(strKey));

            byte[] byteText = myHMACSHA1.ComputeHash(Encoding.UTF8.GetBytes(strText));

            return System.Convert.ToBase64String(byteText);

        }


        private string GetStringToSign(string httpVerb, String contentType, string date, string uri)
        {
            String stringToSign = httpVerb + "\n\n" + contentType + "\n" + date + "\n" + uri;


            return stringToSign;
        }

        /// <summary>
        /// 获取认证签名信息
        /// </summary>
        /// <param name="httpVerb">请求方法</param>
        /// <param name="date">请求时间</param>
        /// <param name="uri">存储桶</param>
        /// <param name="objectname">存储对象名称</param>
        /// <returns></returns>
        public string AuthorizationSignature(string httpVerb, String contentType, string date, string uri)
        {

            var stringToSign = GetStringToSign(httpVerb, contentType, date, uri);

            var signature = ToBase64hmac(this.secretkey, stringToSign);

            return "AWS " + this.accessKey + ":" + signature;

        }
    }
}
