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
using System.Text;
using OBS.Internal.Log;
using System.Collections.Generic;


namespace OBS.Internal.Auth
{
    internal abstract class AbstractSigner : Signer
    {
        protected override void _DoAuth(HttpRequest request, HttpContext context, IHeaders iheaders)
        {
            string signature = this.GetSignature(request, context, iheaders)["Signature"];

            string auth = new StringBuilder(this.GetAuthPrefix()).Append(" ")
                .Append(context.SecurityProvider.Ak).Append(":").Append(signature).ToString();
            request.Headers.Add(Constants.CommonHeaders.Authorization, auth);
        }

        internal override IDictionary<string, string> GetSignature(HttpRequest request, HttpContext context, IHeaders iheaders)
        {
            StringBuilder stringToSign = new StringBuilder();

            stringToSign.Append(request.Method.ToString()).Append("\n");

            string dateHeader = Constants.CommonHeaders.Date.ToLower();
            string contentTypeHeader = Constants.CommonHeaders.ContentType.ToLower();
            string contentMd5Header = Constants.CommonHeaders.ContentMd5.ToLower();
            string headerPrefix = iheaders.HeaderPrefix();
            string headerMetaPrefix = iheaders.HeaderMetaPrefix();

            IDictionary<string, string> tempDict = new Dictionary<string, string>();

            if (request.Headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in request.Headers)
                {
                    if (string.IsNullOrEmpty(entry.Key))
                    {
                        continue;
                    }

                    string key = entry.Key.Trim().ToLower();
                    if (key.StartsWith(headerPrefix) || key.Equals(contentTypeHeader) || key.Equals(contentMd5Header))
                    {
                        tempDict.Add(key, entry.Value);
                    }
                }
            }

            if (request.Headers.ContainsKey(dateHeader))
            {
                tempDict.Add(dateHeader, request.Headers[dateHeader]);
            }
            else
            {
                tempDict.Add(dateHeader, "");
            }

            if (!tempDict.ContainsKey(contentMd5Header))
            {
                tempDict.Add(contentMd5Header, "");
            }

            if (!tempDict.ContainsKey(contentTypeHeader))
            {
                tempDict.Add(contentTypeHeader, "");
            }

            List<KeyValuePair<string, string>> kvlist = new List<KeyValuePair<string, string>>(tempDict);

            tempDict.Clear();

            kvlist.Sort(delegate (KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(x.Key, y.Key, StringComparison.Ordinal);
            });

            foreach (KeyValuePair<string, string> kv in kvlist)
            {
                if (kv.Key.StartsWith(headerMetaPrefix))
                {
                    stringToSign.Append(kv.Key).Append(":").Append(kv.Value.Trim());
                }
                else if (kv.Key.StartsWith(headerPrefix))
                {
                    stringToSign.Append(kv.Key).Append(":").Append(kv.Value);
                }
                else
                {
                    stringToSign.Append(kv.Value);
                }
                stringToSign.Append("\n");
            }

            kvlist.Clear();

            stringToSign.Append("/");
            if (!string.IsNullOrEmpty(request.BucketName))
            {
                stringToSign.Append(CommonUtil.UrlEncode(request.BucketName));

                if (!request.PathStyle)
                {
                    stringToSign.Append("/");
                }

                if (!string.IsNullOrEmpty(request.ObjectKey))
                {
                    if (request.PathStyle)
                    {
                        stringToSign.Append("/");
                    }
                    stringToSign.Append(CommonUtil.UrlEncode(request.ObjectKey, null, "/"));
                }
            }

            if (request.Params.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in request.Params)
                {
                    if (string.IsNullOrEmpty(entry.Key))
                    {
                        continue;
                    }
                    if (Constants.AllowedResourceParameters.Contains(entry.Key.ToLower()) || entry.Key.ToLower().StartsWith(iheaders.HeaderPrefix()))
                    {
                        tempDict.Add(entry.Key, entry.Value);
                    }
                }
            }

            kvlist = new List<KeyValuePair<string, string>>(tempDict);

            tempDict.Clear();

            kvlist.Sort(delegate (KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(x.Key, y.Key, StringComparison.Ordinal);
            });
            if (kvlist.Count > 0)
            {
                bool isFirst = true;
                foreach (KeyValuePair<string, string> kv in kvlist)
                {
                    if (isFirst)
                    {
                        stringToSign.Append("?");
                        isFirst = false;
                    }
                    else
                    {
                        stringToSign.Append("&");
                    }
                    stringToSign.Append(kv.Key);
                    if (kv.Value != null)
                    {
                        stringToSign.Append("=").Append(kv.Value);
                    }
                }
            }

            if (LoggerMgr.IsDebugEnabled)
            {
                LoggerMgr.Debug("StringToSign: ******");
            }

            IDictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("Signature", Convert.ToBase64String(CommonUtil.HmacSha1(context.SecurityProvider.Sk, stringToSign.ToString())));

            return ret;
        }

        protected abstract string GetAuthPrefix();
    }
}
