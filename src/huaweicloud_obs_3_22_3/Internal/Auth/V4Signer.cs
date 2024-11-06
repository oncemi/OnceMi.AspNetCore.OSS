/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using OBS.Internal.Log;

namespace OBS.Internal.Auth
{
    internal class V4Signer : Signer
    {

        private static V4Signer instance = new V4Signer();
        private const string ContentSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        private const string UnsignedPayload = "UNSIGNED-PAYLOAD";
        private const string RegionKey = "region";
        private const string ServiceKey = "s3";
        private const string RequestKey = "aws4_request";
        internal const string Algorithm = "AWS4-HMAC-SHA256";
        internal static readonly string ScopeSuffix = string.Format("/{0}/{1}/{2}", RegionKey, ServiceKey, RequestKey);


        private V4Signer()
        {

        }

        public static Signer GetInstance()
        {
            return instance;
        }

        protected override void _DoAuth(HttpRequest request, HttpContext context, IHeaders iheaders)
        {
            IDictionary<string, string> ret = this.GetSignature(request, context, iheaders);

            string auth = new StringBuilder(Algorithm).Append(" ")
                .Append("Credential=").Append(context.SecurityProvider.Ak).Append("/").Append(ret["ShortDate"])
                .Append(ScopeSuffix).Append(",SignedHeaders=").Append(ret["SignedHeaders"])
                .Append(",Signature=").Append(ret["Signature"])
                .ToString();
            request.Headers.Add(Constants.CommonHeaders.Authorization, auth);
        }

        internal static string CaculateSignature(string stringToSign, string shortDate, string sk)
        {
            byte[] key = CommonUtil.HmacSha256("AWS4" + sk, shortDate);
            key = CommonUtil.HmacSha256(key, RegionKey);
            key = CommonUtil.HmacSha256(key, ServiceKey);
            key = CommonUtil.HmacSha256(key, RequestKey);

            key = CommonUtil.HmacSha256(key, stringToSign);
            return CommonUtil.ToHex(key);
        }

        internal static IDictionary<string, string> GetLongDateAndShortDate(HttpRequest request, IHeaders iheaders)
        {
            string longDate;
            if (request.Headers.ContainsKey(iheaders.DateHeader()))
            {
                longDate = request.Headers[iheaders.DateHeader()];
            }
            else if (request.Headers.ContainsKey(Constants.CommonHeaders.Date))
            {
                longDate =
                    DateTime.ParseExact(request.Headers[Constants.CommonHeaders.Date], Constants.RFC822DateFormat, Constants.CultureInfo)
                    .ToString(Constants.LongDateFormat, Constants.CultureInfo);
            }
            else
            {
                longDate = DateTime.UtcNow.ToString(Constants.LongDateFormat, Constants.CultureInfo);
            }

            string shortDate = longDate.Substring(0, longDate.IndexOf("T"));
            IDictionary<string, string> tempDict = new Dictionary<string, string>();
            tempDict.Add("LongDate", longDate);
            tempDict.Add("ShortDate", shortDate);
            return tempDict;
        }

        internal static List<string> GetSignedHeaderList(IDictionary<string, string> tempDict)
        {
            List<string> klist = new List<string>(tempDict.Keys);

            klist.Sort(delegate (string x, string y)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            });
            return klist;
        }

        internal static string GetSignedHeaders(List<string> klist)
        {
            StringBuilder signedHeaders = new StringBuilder();
            int index = 0;
            int cnt = klist.Count;
            foreach (string k in klist)
            {
                signedHeaders.Append(k);
                if (index++ != cnt - 1)
                {
                    signedHeaders.Append(";");
                }
            }
            return signedHeaders.ToString();
        }


        internal static string GetTemporarySignature(HttpRequest request, HttpContext context, IHeaders iheaders, IDictionary<string, string> dateDict, string signedHeaders,
            IDictionary<string, string> headerDict, List<string> signedHeaderList, string payload)
        {
            StringBuilder canonicalRequest = new StringBuilder();
            canonicalRequest.Append(request.Method).Append("\n");

            // Canonical URI
            canonicalRequest.Append("/");
            if (!string.IsNullOrEmpty(request.BucketName))
            {
                if (request.PathStyle)
                {
                    canonicalRequest.Append(CommonUtil.UrlEncode(request.BucketName));
                }

                if (!string.IsNullOrEmpty(request.ObjectKey))
                {
                    if (request.PathStyle)
                    {
                        canonicalRequest.Append("/");
                    }
                    canonicalRequest.Append(CommonUtil.UrlEncode(request.ObjectKey, null, "/"));
                }
            }

            canonicalRequest.Append("\n");

            //CanonicalQueryString
            IDictionary<string, string> tempDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in request.Params)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }
                tempDict.Add(entry.Key, entry.Value);
            }

            List<KeyValuePair<string, string>> kvlist = new List<KeyValuePair<string, string>>(tempDict);

            tempDict.Clear();

            kvlist.Sort(delegate (KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(x.Key, y.Key, StringComparison.Ordinal);
            });

            canonicalRequest.Append(CommonUtil.ConvertParamsToCanonicalQueryString(kvlist));

            canonicalRequest.Append("\n");

            if (headerDict == null)
            {
                // Canonical Headers
                headerDict = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> entry in request.Headers)
                {
                    if (string.IsNullOrEmpty(entry.Key))
                    {
                        continue;
                    }
                    headerDict.Add(entry.Key.Trim().ToLower(), entry.Value);
                }
            }

            foreach (string key in signedHeaderList)
            {
                canonicalRequest.Append(key).Append(":").Append(headerDict[key]).Append("\n");
            }

            canonicalRequest.Append("\n");

            // SignedHeaders
            canonicalRequest.Append(signedHeaders);
            canonicalRequest.Append("\n");

            // Hashed Payload
            canonicalRequest.Append(string.IsNullOrEmpty(payload) ? UnsignedPayload : payload);

            if (LoggerMgr.IsDebugEnabled)
            {
                LoggerMgr.Debug("CanonicalRequest: ******");
            }

            StringBuilder stringToSign = new StringBuilder(Algorithm).Append("\n")
                .Append(dateDict["LongDate"]).Append("\n")
                .Append(dateDict["ShortDate"]).Append(ScopeSuffix).Append("\n")
                .Append(CommonUtil.HexSha256(canonicalRequest.ToString()));

            if (LoggerMgr.IsDebugEnabled)
            {
                LoggerMgr.Debug("StringToSign:  ******");
            }

            return CaculateSignature(stringToSign.ToString(), dateDict["ShortDate"], context.SecurityProvider.Sk);
        }


        internal override IDictionary<string, string> GetSignature(HttpRequest request, HttpContext context, IHeaders iheaders)
        {
            CommonUtil.AddHeader(request, iheaders.ContentSha256Header(), ContentSha256);
            IDictionary<string, string> tempDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in request.Headers)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }
                tempDict.Add(entry.Key.Trim().ToLower(), entry.Value);
            }

            List<string> signedHeadersList = V4Signer.GetSignedHeaderList(tempDict);
            string signedHeaders = V4Signer.GetSignedHeaders(signedHeadersList);

            IDictionary<string, string> dateDict = V4Signer.GetLongDateAndShortDate(request, iheaders);

            string signature = GetTemporarySignature(request, context, iheaders, dateDict, signedHeaders, tempDict, signedHeadersList, ContentSha256);

            IDictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("Signature", signature);
            ret.Add("ShortDate", dateDict["ShortDate"]);
            ret.Add("SignedHeaders", signedHeaders);
            return ret;
        }
    }
}
