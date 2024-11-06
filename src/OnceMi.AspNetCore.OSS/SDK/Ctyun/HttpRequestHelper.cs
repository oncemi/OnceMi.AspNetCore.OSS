using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace OnceMi.AspNetCore.OSS.SDK.Ctyun
{
    public class HttpRequestHelper
    {
        private static string BaseUri;

        /// <summary>
        ///
        /// </summary>
        private Dictionary<String, string> headers;

        public HttpRequestHelper(string baseUri)
        {
            BaseUri = baseUri;
        }


        #region Delete方式
        private string Delete(string uri, byte[] data)
        {
            string serviceUrl = "";
            if (BaseUri == "" || BaseUri == null)
            {
                serviceUrl = uri;
            }
            else
            {
                serviceUrl = string.Format("{0}/{1}", BaseUri, uri);
            }
            return CommonHttpRequest(serviceUrl, "DELETE", data);
        }
        #endregion
        #region Put方式
        private string Put(string uri, byte[] data)
        {
            string serviceUrl = "";
            if (BaseUri == "" || BaseUri == null)
            {
                serviceUrl = uri;
            }
            else
            {
                serviceUrl = string.Format("{0}/{1}", BaseUri, uri);
            }
            return CommonHttpRequest(serviceUrl, "PUT", data);
        }
        #endregion
        #region POST方式实现


        private string Post(string uri, byte[] data)
        {
            string serviceUrl = "";
            if (BaseUri == "" || BaseUri == null)
            {
                serviceUrl = uri;
            }
            else
            {
                serviceUrl = string.Format("{0}/{1}", BaseUri, uri);
            }
            return CommonHttpRequest(serviceUrl, "Post", data);
        }
        #endregion
        #region GET方式实现
        private string Get(string uri)
        {
            string serviceUrl = "";
            if (BaseUri == "" || BaseUri == null)
            {
                serviceUrl = uri;
            }
            else
            {
                serviceUrl = string.Format("{0}/{1}", BaseUri, uri);
            }
            return CommonHttpRequest(serviceUrl, "GET", null);
        }

        #endregion
        #region  私有方法
        private string CommonHttpRequest(string url, string reqType, byte[] data)
        {
            HttpWebRequest webRequest = null;
            Stream outstream = null;
            HttpWebResponse myResponse = null;
            StreamReader reader = null;
            try
            {
                //构造http请求的对象
                webRequest = (HttpWebRequest)WebRequest.Create(url);


                //设置
                webRequest.ProtocolVersion = HttpVersion.Version11;
                webRequest.Method = reqType;

                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                        webRequest.Headers.Add(header.Key, header.Value);
                }

                if (data != null && data.Length > 0)
                {
                    webRequest.ContentLength = data.Length;

                    //转成网络流
                    byte[] buf = data;

                    outstream = webRequest.GetRequestStream();
                    outstream.Flush();
                    outstream.Write(buf, 0, buf.Length);
                    outstream.Flush();
                    outstream.Close();
                }
                // 获得接口返回值
                myResponse = (HttpWebResponse)webRequest.GetResponse();
                reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string ReturnXml = reader.ReadToEnd();
                reader.Close();
                myResponse.Close();
                webRequest.Abort();
                return ReturnXml;
            }
            catch (Exception ex)
            {
                if (outstream != null) outstream.Close();
                if (reader != null) reader.Close();
                if (myResponse != null) myResponse.Close();
                if (webRequest != null) webRequest.Abort();
                throw ex;
            }
        }

        #endregion


        #region 通用请求
        /// <summary>
        /// Http通用请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string HttpRequest(string url, HttpType type, byte[] data)
        {
            switch (type)
            {
                case HttpType.PUT:
                    return Put(url, data);
                case HttpType.GET:
                    return Get(url);
                case HttpType.POST:
                    return Post(url, data);
                case HttpType.DELETE:
                    return Delete(url, data);
                default:
                    break;
            }
            return "";
        }

        /// <summary>
        /// 添加请求头
        /// </summary>
        /// <param name="headers"></param>
        public void AddRequestHeaders(Dictionary<String, string> headers)
        {
            this.headers = headers;
        }

        /// <summary>
        /// Http通用请求
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="uri"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string HttpRequest(string ip, string port, string uri, HttpType type, byte[] data)
        {
            string url = "http://" + ip + ":" + port + uri;
            return HttpRequest(url, type, data);
        }


        #endregion


        public enum HttpType
        {
            PUT = 0,
            GET = 1,
            POST = 2,
            DELETE = 3
        }
    }
}
