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
using System.Xml;
using System.IO;
using OBS.Model;
using OBS.Internal.Log;
using OBS.Internal.Negotiation;

namespace OBS.Internal
{
    internal class V2Convertor : IConvertor
    {

        protected IHeaders iheaders;

        protected V2Convertor(IHeaders iheaders)
        {
            this.iheaders = iheaders;
        }

        public static IConvertor GetInstance(IHeaders iheaders)
        {
            return new V2Convertor(iheaders);
        }

        protected virtual string TransSseKmsAlgorithm(SseKmsAlgorithmEnum algorithm)
        {
            return "aws:" + EnumAdaptor.GetStringValue(algorithm);
        }

        protected virtual string TransSseCAlgorithmEnum(SseCAlgorithmEnum algorithm)
        {
            return algorithm.ToString().ToUpper();
        }

        protected virtual string TransStorageClass(StorageClassEnum storageClass)
        {
            return EnumAdaptor.GetStringValue(storageClass);
        }

        protected virtual string TransEventType(EventTypeEnum eventType)
        {
            string value = EnumAdaptor.GetStringValue(eventType);
            return "s3:" + value;
        }

        protected virtual string TransDestinationBucket(string bucketName)
        {
            return bucketName.StartsWith("arn:aws:s3:::") ? bucketName : "arn:aws:s3:::" + bucketName;
        }

        protected virtual string TransBucketCannedAcl(CannedAclEnum cannedAcl)
        {
            if (cannedAcl == CannedAclEnum.PublicReadDelivered)
            {
                cannedAcl = CannedAclEnum.PublicRead;
            }
            else if (cannedAcl == CannedAclEnum.PublicReadWriteDelivered)
            {
                cannedAcl = CannedAclEnum.PublicReadWrite;
            }

            return EnumAdaptor.GetStringValue(cannedAcl);
        }

        protected virtual string TransObjectCannedAcl(CannedAclEnum cannedAcl)
        {
            return this.TransBucketCannedAcl(cannedAcl);
        }

        protected virtual string BucketLocationTag
        {
            get
            {
                return "LocationConstraint";
            }
        }

        protected virtual string FilterContainerTag
        {
            get
            {
                return "S3Key";
            }
        }

        protected virtual string BucketStoragePolicyParam
        {
            get
            {
                return EnumAdaptor.GetStringValue(SubResourceEnum.StoragePolicy);
            }
        }

        protected void TransContent(HttpRequest httpRequest, TransContentDelegate transContentDelegate, bool md5)
        {
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Encoding = Encoding.UTF8, OmitXmlDeclaration = true }))
            {
                transContentDelegate(xmlWriter);
            }
            byte[] data = Encoding.UTF8.GetBytes(stringWriter.ToString());
            httpRequest.Content = new MemoryStream(data);
            if (md5)
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentMd5, CommonUtil.Base64Md5(data));
            }
            CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, httpRequest.Content.Length.ToString());
        }

        protected void TransContent(HttpRequest httpRequest, TransContentDelegate transContentDelegate)
        {
            this.TransContent(httpRequest, transContentDelegate, false);
        }

        protected void TransSseCHeader(HttpRequest httpRequest, SseCHeader ssec)
        {
            if (ssec != null)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.SseCHeader(), this.TransSseCAlgorithmEnum(ssec.Algorithm));
                if (ssec.Key != null)
                {
                    CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyHeader(), Convert.ToBase64String(ssec.Key));
                    CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyMd5Header(), CommonUtil.Base64Md5(ssec.Key));
                }
                else if (!string.IsNullOrEmpty(ssec.KeyBase64))
                {
                    CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyHeader(), ssec.KeyBase64);
                    CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyMd5Header(), CommonUtil.Base64Md5(Convert.FromBase64String(ssec.KeyBase64)));
                }
            }
        }

        protected void TransSourceSseCHeader(HttpRequest httpRequest, SseCHeader ssec)
        {
            if (ssec != null)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceSseCHeader(), this.TransSseCAlgorithmEnum(ssec.Algorithm));
                if (ssec.Key != null)
                {
                    CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceSseCKeyHeader(), Convert.ToBase64String(ssec.Key));
                    CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceSseCKeyMd5Header(), CommonUtil.Base64Md5(ssec.Key));
                }
                else if (!string.IsNullOrEmpty(ssec.KeyBase64))
                {
                    CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceSseCKeyHeader(), ssec.KeyBase64);
                    CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceSseCKeyMd5Header(), CommonUtil.Base64Md5(Convert.FromBase64String(ssec.KeyBase64)));
                }
            }

        }

        public HttpRequest Trans(ListBucketsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            if (request.IsQueryLocation)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.LocationHeader(), "true");
            }
            return httpRequest;
        }

        public HttpRequest Trans(CreateBucketRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.BucketName = request.BucketName;

            if (request.CannedAcl.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.AclHeader(), this.TransBucketCannedAcl(request.CannedAcl.Value));
            }
            if (request.StorageClass.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.DefaultStorageClassHeader(), this.TransStorageClass(request.StorageClass.Value));
            }

            if (request.AvailableZone.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.AzRedundancyHeader(), EnumAdaptor.GetStringValue(request.AvailableZone.Value));
            }


            foreach (KeyValuePair<ExtensionBucketPermissionEnum, IList<string>> map in request.ExtensionPermissionMap)
            {
                string header = null;
                switch (map.Key)
                {
                    case ExtensionBucketPermissionEnum.GrantFullControl: header = this.iheaders.GrantFullControlHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantFullControlDelivered: header = this.iheaders.GrantFullControlDeliveredHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantRead: header = this.iheaders.GrantReadHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantReadAcp: header = this.iheaders.GrantReadAcpHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantReadDelivered: header = this.iheaders.GrantReadDeliveredHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantWrite: header = this.iheaders.GrantWriteHeader(); break;
                    case ExtensionBucketPermissionEnum.GrantWriteAcp: header = this.iheaders.GrantWriteAcpHeader(); break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(header) && map.Value != null && map.Value.Count > 0)
                {
                    string[] values = new string[map.Value.Count];
                    for (int i = 0; i < map.Value.Count; i++)
                    {
                        values[i] = "id=" + map.Value[i];
                    }
                    CommonUtil.AddHeader(httpRequest, header, string.Join(",", values));
                }


            }

            if (!string.IsNullOrEmpty(request.Location))
            {
                this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
                {
                    xmlWriter.WriteStartElement("CreateBucketConfiguration");
                    xmlWriter.WriteElementString(this.BucketLocationTag, request.Location);
                    xmlWriter.WriteEndElement();
                });
            }
            return httpRequest;
        }


        public HttpRequest Trans(HeadBucketRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.HEAD;
            httpRequest.BucketName = request.BucketName;
            return httpRequest;
        }

        public HttpRequest Trans(GetBucketMetadataRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.HEAD;
            httpRequest.BucketName = request.BucketName;

            if (!string.IsNullOrEmpty(request.Origin))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.OriginHeader, request.Origin);
            }

            foreach (string header in request.AccessControlRequestHeaders)
            {
                if (!string.IsNullOrEmpty(header))
                {
                    if (!httpRequest.Headers.ContainsKey(Constants.CommonHeaders.AccessControlRequestHeader))
                    {
                        CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.AccessControlRequestHeader, header);
                    }
                    else
                    {
                        httpRequest.Headers[Constants.CommonHeaders.AccessControlRequestHeader] += "," + header;
                    }
                }
            }
            return httpRequest;
        }


        public HttpRequest Trans(SetBucketStoragePolicyRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.BucketName = request.BucketName;

            httpRequest.Params.Add(this.BucketStoragePolicyParam, null);

            if (request.StorageClass.HasValue)
            {
                this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
                {
                    this.TransSetBucketStoragePolicyContent(xmlWriter, request.StorageClass.Value);
                });
            }
            return httpRequest;
        }


        protected virtual void TransSetBucketStoragePolicyContent(XmlWriter xmlWriter, StorageClassEnum storageClass)
        {
            xmlWriter.WriteStartElement("StoragePolicy");
            xmlWriter.WriteElementString("DefaultStorageClass", TransStorageClass(storageClass));
            xmlWriter.WriteEndElement();
        }

        public HttpRequest Trans(GetBucketStoragePolicyRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(this.BucketStoragePolicyParam, null);

            return httpRequest;
        }


        public HttpRequest Trans(DeleteBucketRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.BucketName = request.BucketName;

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketLocationRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Location), null);

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketStorageInfoRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.StorageInfo), null);

            return httpRequest;
        }


        public HttpRequest Trans(ListObjectsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;

            if (request.Prefix != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Prefix, request.Prefix);
            }
            if (!string.IsNullOrEmpty(request.Delimiter))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Delimiter, request.Delimiter);
            }
            if (request.Marker != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Marker, request.Marker);
            }
            if (request.MaxKeys.HasValue)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.MaxKeys, request.MaxKeys.ToString());
            }

            return httpRequest;
        }


        public HttpRequest Trans(ListVersionsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Versions), null);

            if (request.Prefix != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Prefix, request.Prefix);
            }
            if (!string.IsNullOrEmpty(request.Delimiter))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Delimiter, request.Delimiter);
            }
            if (request.KeyMarker != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.KeyMarker, request.KeyMarker);
            }
            if (request.MaxKeys.HasValue)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.MaxKeys, request.MaxKeys.ToString());
            }

            if (!string.IsNullOrEmpty(request.VersionIdMarker))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionIdMarker, request.VersionIdMarker);
            }

            return httpRequest;
        }

        public HttpRequest Trans(SetBucketQuotaRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Quota), null);
            httpRequest.BucketName = request.BucketName;

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("Quota");
                xmlWriter.WriteElementString("StorageQuota", request.StorageQuota.ToString());
                xmlWriter.WriteEndElement();
            });

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketQuotaRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Quota), null);
            httpRequest.BucketName = request.BucketName;
            return httpRequest;
        }

        private void TransGrants(XmlWriter xmlWriter, IList<Grant> grants, string startElementName)
        {
            xmlWriter.WriteStartElement(startElementName);
            foreach (Grant grant in grants)
            {
                if (grant.Grantee != null && grant.Permission.HasValue)
                {
                    xmlWriter.WriteStartElement("Grant");
                    xmlWriter.WriteStartElement("Grantee");
                    if (grant.Grantee is GroupGrantee)
                    {

                        GroupGrantee groupGrantee = grant.Grantee as GroupGrantee;
                        if (groupGrantee.GroupGranteeType.HasValue)
                        {
                            xmlWriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "Group");
                            xmlWriter.WriteElementString("URI", EnumAdaptor.GetStringValue(groupGrantee.GroupGranteeType));
                        }
                    }
                    else if (grant.Grantee is CanonicalGrantee)
                    {
                        xmlWriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "CanonicalUser");
                        CanonicalGrantee canonicalGrantee = grant.Grantee as CanonicalGrantee;
                        xmlWriter.WriteElementString("ID", canonicalGrantee.Id);
                        if (!string.IsNullOrEmpty(canonicalGrantee.DisplayName))
                        {
                            xmlWriter.WriteElementString("DisplayName", canonicalGrantee.DisplayName);
                        }
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteElementString("Permission", EnumAdaptor.GetStringValue(grant.Permission));
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        protected virtual void TransAccessControlList(HttpRequest httpRequest, AccessControlList acl, bool isBucket)
        {
            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("AccessControlPolicy");
                if (acl.Owner != null && !string.IsNullOrEmpty(acl.Owner.Id))
                {
                    xmlWriter.WriteStartElement("Owner");
                    xmlWriter.WriteElementString("ID", acl.Owner.Id);
                    if (!string.IsNullOrEmpty(acl.Owner.DisplayName))
                    {
                        xmlWriter.WriteElementString("DisplayName", acl.Owner.DisplayName);
                    }
                    xmlWriter.WriteEndElement();
                }
                if (acl.Grants.Count > 0)
                {
                    this.TransGrants(xmlWriter, acl.Grants, "AccessControlList");
                }
                xmlWriter.WriteEndElement();
            });
        }

        public HttpRequest Trans(SetBucketAclRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Acl), null);
            httpRequest.BucketName = request.BucketName;
            if (request.CannedAcl.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.AclHeader(), TransBucketCannedAcl(request.CannedAcl.Value));
            }
            else if (request.AccessControlList != null)
            {
                this.TransAccessControlList(httpRequest, request.AccessControlList, true);
            }
            return httpRequest;
        }

        public HttpRequest Trans(GetBucketAclRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Acl), null);
            httpRequest.BucketName = request.BucketName;
            return httpRequest;
        }

        public HttpRequest Trans(ListMultipartUploadsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Uploads), null);
            httpRequest.BucketName = request.BucketName;

            if (request.Prefix != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Prefix, request.Prefix);
            }
            if (!string.IsNullOrEmpty(request.Delimiter))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.Delimiter, request.Delimiter);
            }
            if (request.KeyMarker != null)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.KeyMarker, request.KeyMarker);
            }
            if (request.MaxUploads.HasValue)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.MaxUploads, request.MaxUploads.ToString());
            }

            if (!string.IsNullOrEmpty(request.UploadIdMarker))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.UploadIdMarker, request.UploadIdMarker);
            }

            return httpRequest;
        }

        protected virtual void TransLoggingConfiguration(HttpRequest httpRequest, LoggingConfiguration configuration)
        {
            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("BucketLoggingStatus");

                if (configuration != null && (!string.IsNullOrEmpty(configuration.TargetBucketName) || configuration.TargetPrefix != null))
                {
                    xmlWriter.WriteStartElement("LoggingEnabled");
                    if (!string.IsNullOrEmpty(configuration.TargetBucketName))
                    {
                        xmlWriter.WriteElementString("TargetBucket", configuration.TargetBucketName);
                    }

                    if (configuration.TargetPrefix != null)
                    {
                        xmlWriter.WriteElementString("TargetPrefix", configuration.TargetPrefix);
                    }

                    if (configuration.Grants.Count > 0)
                    {
                        this.TransGrants(xmlWriter, configuration.Grants, "TargetGrants");
                    }

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            });
        }

        public HttpRequest Trans(SetBucketLoggingRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Logging), null);

            this.TransLoggingConfiguration(httpRequest, request.Configuration);
            return httpRequest;
        }

        public HttpRequest Trans(GetBucketLoggingRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Logging), null);

            return httpRequest;
        }

        public HttpRequest Trans(SetBucketPolicyRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Policy), null);


            if (!string.IsNullOrEmpty(request.Policy))
            {
                httpRequest.Content = new MemoryStream(Encoding.UTF8.GetBytes(request.Policy));
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, httpRequest.Content.Length.ToString());
            }

            if (!string.IsNullOrEmpty(request.ContentMD5))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentMd5, request.ContentMD5);
            }

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketPolicyRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Policy), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketPolicyRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Policy), null);
            return httpRequest;
        }

        public HttpRequest Trans(SetBucketCorsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Cors), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("CORSConfiguration");
                if (request.Configuration != null)
                {
                    foreach (CorsRule corsRule in request.Configuration.Rules)
                    {
                        xmlWriter.WriteStartElement("CORSRule");

                        if (!string.IsNullOrEmpty(corsRule.Id))
                        {
                            xmlWriter.WriteElementString("ID", corsRule.Id);
                        }

                        foreach (HttpVerb method in corsRule.AllowedMethods)
                        {
                            xmlWriter.WriteElementString("AllowedMethod", method.ToString());
                        }

                        foreach (string origin in corsRule.AllowedOrigins)
                        {
                            if (!string.IsNullOrEmpty(origin))
                            {
                                xmlWriter.WriteElementString("AllowedOrigin", origin);
                            }
                        }

                        foreach (string header in corsRule.AllowedHeaders)
                        {
                            if (!string.IsNullOrEmpty(header))
                            {
                                xmlWriter.WriteElementString("AllowedHeader", header);
                            }
                        }

                        if (corsRule.MaxAgeSeconds.HasValue)
                        {
                            xmlWriter.WriteElementString("MaxAgeSeconds", corsRule.MaxAgeSeconds.Value.ToString());
                        }

                        foreach (string exposeHeader in corsRule.ExposeHeaders)
                        {
                            if (!string.IsNullOrEmpty(exposeHeader))
                            {
                                xmlWriter.WriteElementString("ExposeHeader", exposeHeader);
                            }
                        }

                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketCorsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Cors), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketCorsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Cors), null);
            return httpRequest;
        }

        public HttpRequest Trans(SetBucketLifecycleRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Lifecyle), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("LifecycleConfiguration");

                if (request.Configuration != null)
                {
                    foreach (LifecycleRule rule in request.Configuration.Rules)
                    {
                        xmlWriter.WriteStartElement("Rule");
                        if (!string.IsNullOrEmpty(rule.Id))
                        {
                            xmlWriter.WriteElementString("ID", rule.Id);
                        }

                        if (rule.Prefix != null)
                        {
                            xmlWriter.WriteElementString("Prefix", rule.Prefix);
                        }

                        xmlWriter.WriteElementString("Status", rule.Status.ToString());

                        if (rule.Expiration != null)
                        {
                            xmlWriter.WriteStartElement("Expiration");
                            if (rule.Expiration.Days.HasValue)
                            {
                                xmlWriter.WriteElementString("Days", rule.Expiration.Days.Value.ToString());
                            }
                            else if (rule.Expiration.Date.HasValue)
                            {
                                xmlWriter.WriteStartElement("Date", rule.Expiration.Date.Value.ToString(Constants.ISO8601DateFormatMidNight, Constants.CultureInfo));
                            }
                            xmlWriter.WriteEndElement();
                        }

                        if (rule.NoncurrentVersionExpiration != null)
                        {
                            xmlWriter.WriteStartElement("NoncurrentVersionExpiration");
                            xmlWriter.WriteElementString("NoncurrentDays", rule.NoncurrentVersionExpiration.NoncurrentDays.ToString());
                            xmlWriter.WriteEndElement();
                        }

                        foreach (Transition transition in rule.Transitions)
                        {
                            if (transition != null)
                            {
                                xmlWriter.WriteStartElement("Transition");
                                if (transition.Days.HasValue)
                                {
                                    xmlWriter.WriteElementString("Days", transition.Days.Value.ToString());
                                }
                                else if (transition.Date.HasValue)
                                {
                                    xmlWriter.WriteStartElement("Date", transition.Date.Value.ToString(Constants.ISO8601DateFormatMidNight, Constants.CultureInfo));
                                }

                                if (transition.StorageClass.HasValue)
                                {
                                    xmlWriter.WriteElementString("StorageClass", this.TransStorageClass(transition.StorageClass.Value));
                                }

                                xmlWriter.WriteEndElement();
                            }
                        }

                        foreach (NoncurrentVersionTransition noncurrentVersionTransition in rule.NoncurrentVersionTransitions)
                        {
                            if (noncurrentVersionTransition != null)
                            {
                                xmlWriter.WriteStartElement("NoncurrentVersionTransition");
                                xmlWriter.WriteElementString("NoncurrentDays", noncurrentVersionTransition.NoncurrentDays.ToString());

                                if (noncurrentVersionTransition.StorageClass.HasValue)
                                {
                                    xmlWriter.WriteElementString("StorageClass", this.TransStorageClass(noncurrentVersionTransition.StorageClass.Value));
                                }

                                xmlWriter.WriteEndElement();
                            }
                        }

                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketLifecycleRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Lifecyle), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketLifecycleRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Lifecyle), null);
            return httpRequest;
        }

        public HttpRequest Trans(SetBucketWebsiteRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Website), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("WebsiteConfiguration");

                if (request.Configuration != null)
                {
                    if (request.Configuration.RedirectAllRequestsTo != null)
                    {
                        xmlWriter.WriteStartElement("RedirectAllRequestsTo");
                        if (!string.IsNullOrEmpty(request.Configuration.RedirectAllRequestsTo.HostName))
                        {
                            xmlWriter.WriteElementString("HostName", request.Configuration.RedirectAllRequestsTo.HostName);
                        }

                        if (request.Configuration.RedirectAllRequestsTo.Protocol.HasValue)
                        {
                            xmlWriter.WriteElementString("Protocol", request.Configuration.RedirectAllRequestsTo.Protocol.Value.ToString().ToLower());
                        }

                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(request.Configuration.IndexDocument))
                        {
                            xmlWriter.WriteStartElement("IndexDocument");
                            xmlWriter.WriteElementString("Suffix", request.Configuration.IndexDocument);
                            xmlWriter.WriteEndElement();
                        }

                        if (!string.IsNullOrEmpty(request.Configuration.ErrorDocument))
                        {
                            xmlWriter.WriteStartElement("ErrorDocument");
                            xmlWriter.WriteElementString("Key", request.Configuration.ErrorDocument);
                            xmlWriter.WriteEndElement();
                        }

                        if (request.Configuration.RoutingRules.Count > 0)
                        {
                            xmlWriter.WriteStartElement("RoutingRules");

                            foreach (RoutingRule routingRule in request.Configuration.RoutingRules)
                            {
                                if (routingRule != null)
                                {
                                    xmlWriter.WriteStartElement("RoutingRule");

                                    if (routingRule.Condition != null)
                                    {
                                        xmlWriter.WriteStartElement("Condition");

                                        if (routingRule.Condition.KeyPrefixEquals != null)
                                        {
                                            xmlWriter.WriteElementString("KeyPrefixEquals", routingRule.Condition.KeyPrefixEquals);
                                        }

                                        if (!string.IsNullOrEmpty(routingRule.Condition.HttpErrorCodeReturnedEquals))
                                        {
                                            xmlWriter.WriteElementString("HttpErrorCodeReturnedEquals", routingRule.Condition.HttpErrorCodeReturnedEquals);
                                        }

                                        xmlWriter.WriteEndElement();

                                    }

                                    if (routingRule.Redirect != null)
                                    {
                                        xmlWriter.WriteStartElement("Redirect");

                                        if (routingRule.Redirect.Protocol.HasValue)
                                        {
                                            xmlWriter.WriteElementString("Protocol", routingRule.Redirect.Protocol.Value.ToString().ToLower());
                                        }

                                        if (!string.IsNullOrEmpty(routingRule.Redirect.HostName))
                                        {
                                            xmlWriter.WriteElementString("HostName", routingRule.Redirect.HostName);
                                        }

                                        if (!string.IsNullOrEmpty(routingRule.Redirect.ReplaceKeyPrefixWith))
                                        {
                                            xmlWriter.WriteElementString("ReplaceKeyPrefixWith", routingRule.Redirect.ReplaceKeyPrefixWith);
                                        }

                                        if (!string.IsNullOrEmpty(routingRule.Redirect.ReplaceKeyWith))
                                        {
                                            xmlWriter.WriteElementString("ReplaceKeyWith", routingRule.Redirect.ReplaceKeyWith);
                                        }

                                        if (!string.IsNullOrEmpty(routingRule.Redirect.HttpRedirectCode))
                                        {
                                            xmlWriter.WriteElementString("HttpRedirectCode", routingRule.Redirect.HttpRedirectCode);
                                        }

                                        xmlWriter.WriteEndElement();
                                    }

                                    xmlWriter.WriteEndElement();
                                }
                            }

                            xmlWriter.WriteEndElement();
                        }

                    }

                }
                xmlWriter.WriteEndElement();
            });

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketWebsiteRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Website), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketWebsiteRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Website), null);
            return httpRequest;
        }

        public HttpRequest Trans(SetBucketVersioningRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Versioning), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("VersioningConfiguration");
                if (request.Configuration != null && request.Configuration.Status.HasValue)
                {
                    xmlWriter.WriteElementString("Status", request.Configuration.Status.Value.ToString());
                }
                xmlWriter.WriteEndElement();
            });

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketVersioningRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Versioning), null);
            return httpRequest;
        }

        public HttpRequest Trans(SetBucketTaggingRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Tagging), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("Tagging");

                if (request.Tags.Count > 0)
                {
                    xmlWriter.WriteStartElement("TagSet");
                    foreach (Tag tag in request.Tags)
                    {
                        if (tag != null && !string.IsNullOrEmpty(tag.Key) && tag.Value != null)
                        {
                            xmlWriter.WriteStartElement("Tag");
                            xmlWriter.WriteElementString("Key", tag.Key);
                            xmlWriter.WriteElementString("Value", tag.Value);
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketTaggingRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Tagging), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketTaggingRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Tagging), null);
            return httpRequest;
        }

        private void WriteFilterRules(XmlWriter xmlWriter, List<FilterRule> filterRules)
        {
            xmlWriter.WriteStartElement("Filter");
            xmlWriter.WriteStartElement(this.FilterContainerTag);
            foreach (FilterRule rule in filterRules)
            {
                if (rule != null)
                {
                    xmlWriter.WriteStartElement("FilterRule");
                    if (rule.Name.HasValue)
                    {
                        xmlWriter.WriteElementString("Name", rule.Name.Value.ToString().ToLower());
                    }

                    if (rule.Value != null)
                    {
                        xmlWriter.WriteElementString("Value", rule.Value);
                    }
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        public HttpRequest Trans(SetBucketNotificationRequest request)
        {
            
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Notification), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {   
                xmlWriter.WriteStartElement("NotificationConfiguration");
                if (request.Configuration != null)
                {

                    foreach (TopicConfiguration tc in request.Configuration.TopicConfigurations)
                    {
                        if (tc != null)
                        {
                            xmlWriter.WriteStartElement("TopicConfiguration");
                            if (!string.IsNullOrEmpty(tc.Id))
                            {
                                xmlWriter.WriteElementString("Id", tc.Id);
                            }

                            if (tc.FilterRules.Count > 0)
                            {
                                this.WriteFilterRules(xmlWriter, tc.FilterRules);
                            }

                            if (!string.IsNullOrEmpty(tc.Topic))
                            {
                                xmlWriter.WriteElementString("Topic", tc.Topic);
                            }

                            foreach (EventTypeEnum e in tc.Events)
                            {
                                xmlWriter.WriteElementString("Event", this.TransEventType(e));
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }

                    foreach (FunctionGraphConfiguration fc in request.Configuration.FunctionGraphConfigurations)
                    {
                        if (fc != null)
                        {
                            xmlWriter.WriteStartElement("FunctionGraphConfiguration");
                            if (!string.IsNullOrEmpty(fc.Id))
                            {
                                xmlWriter.WriteElementString("Id", fc.Id);
                            }

                            if (fc.FilterRules.Count > 0)
                            {
                                this.WriteFilterRules(xmlWriter, fc.FilterRules);
                            }

                            if (!string.IsNullOrEmpty(fc.FunctionGraph))
                            {
                                xmlWriter.WriteElementString("FunctionGraph", fc.FunctionGraph);
                            }

                            foreach (EventTypeEnum e in fc.Events)
                            {
                                xmlWriter.WriteElementString("Event", this.TransEventType(e));
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();
            });
            return httpRequest;
        }

        public HttpRequest Trans(GetBucketNotificationRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Notification), null);
            return httpRequest;
        }

        public HttpRequest Trans(AbortMultipartUploadRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.Params.Add(Constants.ObsRequestParams.UploadId, request.UploadId);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteObjectRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.DELETE;
            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }
            return httpRequest;
        }

        public HttpRequest Trans(DeleteObjectsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.Method = HttpVerb.POST;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Delete), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("Delete");
                if (request.Quiet.HasValue)
                {
                    xmlWriter.WriteElementString("Quiet", request.Quiet.Value.ToString().ToLower());
                }

                foreach (KeyVersion obj in request.Objects)
                {
                    if (obj != null)
                    {
                        xmlWriter.WriteStartElement("Object");

                        if (obj.Key != null)
                        {
                            xmlWriter.WriteElementString("Key", obj.Key);
                        }

                        if (!string.IsNullOrEmpty(obj.VersionId))
                        {
                            xmlWriter.WriteElementString("VersionId", obj.VersionId);
                        }

                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        protected virtual void TransTier(RestoreTierEnum? tier, XmlWriter xmlWriter)
        {
            if (tier.HasValue)
            {
                xmlWriter.WriteStartElement("GlacierJobParameters");

                xmlWriter.WriteElementString("Tier", tier.Value.ToString());

                xmlWriter.WriteEndElement();
            }
        }

        public HttpRequest Trans(RestoreObjectRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.POST;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Restore), null);

            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("RestoreRequest");
                xmlWriter.WriteElementString("Days", request.Days.ToString());

                this.TransTier(request.Tier, xmlWriter);

                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        public HttpRequest Trans(ListPartsRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(Constants.ObsRequestParams.UploadId, request.UploadId);

            if (request.MaxParts.HasValue)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.MaxParts, request.MaxParts.Value.ToString());
            }

            if (request.PartNumberMarker.HasValue)
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.PartNumberMarker, request.PartNumberMarker.Value.ToString());
            }
            return httpRequest;
        }

        public HttpRequest Trans(CompleteMultipartUploadRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.POST;
            httpRequest.Params.Add(Constants.ObsRequestParams.UploadId, request.UploadId);


            List<PartETag> temp = request.PartETags as List<PartETag>;

            if (temp == null)
            {
                temp = new List<PartETag>();
                foreach (PartETag part in request.PartETags)
                {
                    temp.Add(part);
                }
            }

            temp.Sort(delegate (PartETag part1, PartETag part2)
            {
                return part1.PartNumber.CompareTo(part2.PartNumber);
            });

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("CompleteMultipartUpload");
                foreach (PartETag part in temp)
                {
                    xmlWriter.WriteStartElement("Part");
                    xmlWriter.WriteElementString("PartNumber", part.PartNumber.ToString());
                    if (!string.IsNullOrEmpty(part.ETag))
                    {
                        xmlWriter.WriteElementString("ETag", part.ETag);
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            });

            return httpRequest;
        }

        public HttpRequest Trans(SetObjectAclRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Acl), null);
            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }

            if (request.CannedAcl.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.AclHeader(), TransObjectCannedAcl(request.CannedAcl.Value));
            }
            else if (request.AccessControlList != null)
            {
                this.TransAccessControlList(httpRequest, request.AccessControlList, false);
            }

            return httpRequest;
        }

        public HttpRequest Trans(GetObjectAclRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            httpRequest.Method = HttpVerb.GET;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Acl), null);
            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }

            return httpRequest;
        }

        protected void TransPutObjectBasicRequest(PutObjectBasicRequest request, HttpRequest httpRequest)
        {
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;

            if (!string.IsNullOrEmpty(request.WebsiteRedirectLocation))
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.WebsiteRedirectLocationHeader(), request.WebsiteRedirectLocation);
            }
            if (!string.IsNullOrEmpty(request.SuccessRedirectLocation))
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.SuccessRedirectLocationHeader(), request.SuccessRedirectLocation);
            }
            if (request.StorageClass.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.StorageClassHeader(), this.TransStorageClass(request.StorageClass.Value));
            }
            if (request.CannedAcl.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.AclHeader(), this.TransObjectCannedAcl(request.CannedAcl.Value));
            }


            foreach (KeyValuePair<ExtensionObjectPermissionEnum, IList<string>> map in request.ExtensionPermissionMap)
            {
                string header = null;
                switch (map.Key)
                {
                    case ExtensionObjectPermissionEnum.GrantFullControl: header = this.iheaders.GrantFullControlHeader(); break;
                    case ExtensionObjectPermissionEnum.GrantRead: header = this.iheaders.GrantReadHeader(); break;
                    case ExtensionObjectPermissionEnum.GrantReadAcp: header = this.iheaders.GrantReadAcpHeader(); break;
                    case ExtensionObjectPermissionEnum.GrantWriteAcp: header = this.iheaders.GrantWriteAcpHeader(); break;
                    default: break;
                }
                if (!string.IsNullOrEmpty(header) && map.Value != null && map.Value.Count > 0)
                {
                    string[] values = new string[map.Value.Count];
                    for (int i = 0; i < map.Value.Count; i++)
                    {
                        values[i] = "id=" + map.Value[i];
                    }
                    CommonUtil.AddHeader(httpRequest, header, string.Join(",", values));
                }
            }

            foreach (KeyValuePair<string,string> entry in request.Metadata.KeyValuePairs)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }
                string _key = entry.Key;
                if (!entry.Key.StartsWith(this.iheaders.HeaderMetaPrefix(), StringComparison.OrdinalIgnoreCase) && !entry.Key.StartsWith(Constants.ObsHeaderMetaPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    _key = this.iheaders.HeaderMetaPrefix() + _key;
                }
                CommonUtil.AddHeader(httpRequest, _key, entry.Value);
            }

            if (request.SseHeader != null)
            {
                SseCHeader ssec = request.SseHeader as SseCHeader;
                if (ssec != null)
                {
                    CommonUtil.AddHeader(httpRequest, this.iheaders.SseCHeader(), this.TransSseCAlgorithmEnum(ssec.Algorithm));
                    if (ssec.Key != null)
                    {
                        CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyHeader(), Convert.ToBase64String(ssec.Key));
                        CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyMd5Header(), CommonUtil.Base64Md5(ssec.Key));
                    }
                    else if (!string.IsNullOrEmpty(ssec.KeyBase64))
                    {
                        CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyHeader(), ssec.KeyBase64);
                        CommonUtil.AddHeader(httpRequest, this.iheaders.SseCKeyMd5Header(), CommonUtil.Base64Md5(Convert.FromBase64String(ssec.KeyBase64)));
                    }
                }
                else
                {
                    SseKmsHeader sseKms = request.SseHeader as SseKmsHeader;
                    if (sseKms != null)
                    {
                        CommonUtil.AddHeader(httpRequest, this.iheaders.SseKmsHeader(), this.TransSseKmsAlgorithm(sseKms.Algorithm));
                        if (!string.IsNullOrEmpty(sseKms.Key))
                        {
                            CommonUtil.AddHeader(httpRequest, this.iheaders.SseKmsKeyHeader(), sseKms.Key);
                        }
                    }
                }
            }

        }

        public HttpRequest Trans(PutObjectRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();

            this.TransPutObjectBasicRequest(request, httpRequest);

            httpRequest.Method = HttpVerb.PUT;
            httpRequest.AutoClose = request.AutoClose;

            if (string.IsNullOrEmpty(request.ContentType))
            {
                string suffix = request.ObjectKey.Substring(request.ObjectKey.LastIndexOf(".") + 1);
                if (Constants.MimeTypes.ContainsKey(suffix))
                {
                    request.ContentType = Constants.MimeTypes[suffix];
                }
            }

            long contentLength = -1;
            if (request.InputStream != null)
            {
                httpRequest.Content = request.InputStream;
                if (request.ContentLength.HasValue && request.ContentLength.Value > 0)
                {
                    contentLength = request.ContentLength.Value;
                    CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, contentLength.ToString());
                }
            }
            else if (!string.IsNullOrEmpty(request.FilePath))
            {
                if (string.IsNullOrEmpty(request.ContentType))
                {
                    string suffix = request.FilePath.Substring(request.FilePath.LastIndexOf(".") + 1);
                    if (Constants.MimeTypes.ContainsKey(suffix))
                    {
                        request.ContentType = Constants.MimeTypes[suffix];
                    }
                }

                long fileLength = new FileInfo(request.FilePath).Length;
                httpRequest.Content = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
                long offset = 0L;
                if (request.Offset.HasValue)
                {
                    offset = request.Offset.Value;
                    offset = offset >= 0 && offset < fileLength ? offset : 0L;
                    httpRequest.Content.Seek(offset, SeekOrigin.Begin);
                }
                if (request.ContentLength.HasValue && contentLength > 0 && contentLength <= fileLength - offset)
                {
                    contentLength = request.ContentLength.Value;
                }
                else
                {
                    contentLength = fileLength - offset;
                }
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, contentLength.ToString());
            }

            if(request.UploadProgress != null && httpRequest.Content != null)
            {
                TransferStream stream = new TransferStream(httpRequest.Content);
                if (contentLength < 0)
                {
                    try
                    {
                        contentLength = stream.Length;
                    }catch(Exception e)
                    {
                        LoggerMgr.Warn("Cannot get content length from origin stream", e);
                    }
                }
                TransferStreamManager mgr;
                if (request.ProgressType == ProgressTypeEnum.ByBytes) {
                    mgr = new TransferStreamByBytes(request.Sender, request.UploadProgress,
                    contentLength, 0, request.ProgressInterval);
                }
                else
                {
                   mgr = new ThreadSafeTransferStreamBySeconds(request.Sender, request.UploadProgress,
                   contentLength, 0, request.ProgressInterval);
                   stream.CloseStream += mgr.TransferEnd;
                }
                stream.BytesReaded += mgr.BytesTransfered;
                stream.StartRead += mgr.TransferStart;
                httpRequest.Content = stream;
            }

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentType, request.ContentType);
            }

            if (!string.IsNullOrEmpty(request.ContentMd5))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentMd5, request.ContentMd5);
            }
            if (request.Expires.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.ExpiresHeader(), request.Expires.Value.ToString());
            }


            return httpRequest;
        }

        public HttpRequest Trans(AppendObjectRequest request)
        {
            HttpRequest httpRequest = this.Trans(request as PutObjectRequest);
            httpRequest.Method = HttpVerb.POST;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Append), null);
            httpRequest.Params.Add(Constants.ObsRequestParams.Position, request.Position.ToString());
            return httpRequest;
        }

        public HttpRequest Trans(CopyObjectRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();

            this.TransPutObjectBasicRequest(request, httpRequest);

            httpRequest.Method = HttpVerb.PUT;

            string copySource = string.Format("{0}/{1}", request.SourceBucketName, CommonUtil.UrlEncode(request.SourceObjectKey));

            if (!string.IsNullOrEmpty(request.SourceVersionId))
            {
                copySource += "?versionId" + request.SourceVersionId;
            }

            CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceHeader(), copySource);

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentType, request.ContentType);
            }

            CommonUtil.AddHeader(httpRequest, this.iheaders.MetadataDirectiveHeader(), request.MetadataDirective.ToString().ToUpper());

            this.TransSourceSseCHeader(httpRequest, request.SourceSseCHeader);

            if (!string.IsNullOrEmpty(request.IfMatch))
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceIfMatchHeader(), request.IfMatch);
            }

            if (!string.IsNullOrEmpty(request.IfNoneMatch))
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceIfNoneMatchHeader(), request.IfNoneMatch);
            }

            if (request.IfModifiedSince.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceIfModifiedSinceHeader(), request.IfModifiedSince.Value.ToString(Constants.RFC822DateFormat, Constants.CultureInfo));
            }

            if (request.IfUnmodifiedSince.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceIfUnmodifiedSinceHeader(), request.IfUnmodifiedSince.Value.ToString(Constants.RFC822DateFormat, Constants.CultureInfo));
            }

            return httpRequest;
        }

        public HttpRequest Trans(InitiateMultipartUploadRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();

            this.TransPutObjectBasicRequest(request, httpRequest);

            httpRequest.Method = HttpVerb.POST;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Uploads), null);

            if (string.IsNullOrEmpty(request.ContentType))
            {
                string suffix = request.ObjectKey.Substring(request.ObjectKey.LastIndexOf(".") + 1);
                if (Constants.MimeTypes.ContainsKey(suffix))
                {
                    request.ContentType = Constants.MimeTypes[suffix];
                }
            }

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentType, request.ContentType);
            }

            if (request.Expires.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.ExpiresHeader(), request.Expires.Value.ToString());
            }

            return httpRequest;
        }



        public HttpRequest Trans(CopyPartRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;

            httpRequest.Params.Add(Constants.ObsRequestParams.UploadId, request.UploadId);
            httpRequest.Params.Add(Constants.ObsRequestParams.PartNumber, request.PartNumber.ToString());

            string copySource = string.Format("{0}/{1}", request.SourceBucketName, CommonUtil.UrlEncode(request.SourceObjectKey));

            if (!string.IsNullOrEmpty(request.SourceVersionId))
            {
                copySource += "?versionId" + request.SourceVersionId;
            }

            CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceHeader(), copySource);

            if(request.ByteRange != null && request.ByteRange.Start >=0 && request.ByteRange.Start <= request.ByteRange.End)
            {
                CommonUtil.AddHeader(httpRequest, this.iheaders.CopySourceRangeHeader(), string.Format("bytes={0}-{1}", request.ByteRange.Start, request.ByteRange.End));
            }

            this.TransSourceSseCHeader(httpRequest, request.SourceSseCHeader);

            this.TransSseCHeader(httpRequest, request.DestinationSseCHeader);

            return httpRequest;
        }

        public HttpRequest Trans(UploadPartRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.AutoClose = request.AutoClose;
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;

            httpRequest.Params.Add(Constants.ObsRequestParams.UploadId, request.UploadId);
            httpRequest.Params.Add(Constants.ObsRequestParams.PartNumber, request.PartNumber.ToString());

            this.TransSseCHeader(httpRequest, request.SseCHeader);

            if (!string.IsNullOrEmpty(request.ContentMd5))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentMd5, request.ContentMd5);
            }

            long contentLength = -1;
            if (request.InputStream != null)
            {
                httpRequest.Content = request.InputStream;
                if (request.PartSize.HasValue)
                {
                    long partSize = request.PartSize.Value;
                    if(partSize > 0)
                    {
                        contentLength = partSize;
                        CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, contentLength.ToString());
                    }
                }
            }
            else if (!string.IsNullOrEmpty(request.FilePath))
            {
                long fileSize = new FileInfo(request.FilePath).Length;
                httpRequest.Content = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
                long offset = request.Offset.HasValue ? request.Offset.Value : 0L;
                offset = offset >=0 && offset < fileSize ? offset : 0L;
                long partSize = request.PartSize.HasValue ? request.PartSize.Value : 0L;
                partSize = partSize > 0 && partSize <= (fileSize - offset) ? partSize : fileSize - offset;
                httpRequest.Content.Seek(offset, SeekOrigin.Begin);
                contentLength = partSize;
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.ContentLength, contentLength.ToString());
            }

            if (request.UploadProgress != null && httpRequest.Content != null)
            {
                TransferStream stream = new TransferStream(httpRequest.Content);
                if (contentLength < 0)
                {
                    try
                    {
                        contentLength = stream.Length;
                    }
                    catch (Exception e)
                    {
                        LoggerMgr.Warn("Cannot get content length from origin stream", e);
                    }
                }
                TransferStreamManager mgr;
                if (request.ProgressType == ProgressTypeEnum.ByBytes)
                {
                    mgr = new TransferStreamByBytes(request.Sender, request.UploadProgress,
                    contentLength, 0, request.ProgressInterval);
                }
                else
                {
                    mgr = new ThreadSafeTransferStreamBySeconds(request.Sender, request.UploadProgress,
                    contentLength, 0, request.ProgressInterval);
                    stream.CloseStream += mgr.TransferEnd;
                }
                stream.BytesReaded += mgr.BytesTransfered;
                stream.StartRead += mgr.TransferStart;
                stream.BytesReset += mgr.TransferReset;
                httpRequest.Content = stream;
            }

            return httpRequest;
        }

        public HttpRequest Trans(SetBucketReplicationRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.PUT;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Replication), null);

            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("ReplicationConfiguration");
                if(request.Configuration != null)
                {
                    if (!string.IsNullOrEmpty(request.Configuration.Agency))
                    {
                        xmlWriter.WriteElementString("Agency", request.Configuration.Agency);
                    }

                    foreach(ReplicationRule rule in request.Configuration.Rules)
                    {
                        if(rule != null)
                        {
                            xmlWriter.WriteStartElement("Rule");
                            if (!string.IsNullOrEmpty(rule.Id))
                            {
                                xmlWriter.WriteElementString("ID", rule.Id);
                            }
                            if (!string.IsNullOrEmpty(rule.Prefix))
                            {
                                xmlWriter.WriteElementString("Prefix", rule.Prefix);
                            }
                            xmlWriter.WriteElementString("Status", rule.Status.ToString());

                            if (!string.IsNullOrEmpty(rule.TargetBucketName))
                            {
                                xmlWriter.WriteStartElement("Destination");
                                xmlWriter.WriteElementString("Bucket", this.TransDestinationBucket(rule.TargetBucketName));
                                if (rule.TargetStorageClass.HasValue)
                                {
                                    xmlWriter.WriteElementString("StorageClass", this.TransStorageClass(rule.TargetStorageClass.Value));
                                }
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }

                }
                xmlWriter.WriteEndElement();
            }, true);

            return httpRequest;
        }

        public HttpRequest Trans(GetBucketReplicationRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.GET;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Replication), null);
            return httpRequest;
        }

        public HttpRequest Trans(DeleteBucketReplicationRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.DELETE;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(EnumAdaptor.GetStringValue(SubResourceEnum.Replication), null);
            return httpRequest;
        }

        public HttpRequest Trans(GetObjectMetadataRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.HEAD;
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }
            this.TransSseCHeader(httpRequest, request.SseCHeader);

            return httpRequest;
        }

        public HttpRequest Trans(GetObjectRequest request)
        {
            HttpRequest httpRequest = this.Trans(request as GetObjectMetadataRequest);
            httpRequest.Method = HttpVerb.GET;

            if (!string.IsNullOrEmpty(request.ImageProcess))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.ImageProcess, request.ImageProcess);
            }

            if(request.ResponseHeaderOverrides != null)
            {
                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.CacheControl))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseCacheControl, request.ResponseHeaderOverrides.CacheControl);
                }

                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.ContentDisposition))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseContentDisposition, request.ResponseHeaderOverrides.ContentDisposition);
                }
                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.ContentEncoding))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseContentEncoding, request.ResponseHeaderOverrides.ContentEncoding);
                }
                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.ContentLanguage))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseContentLanguage, request.ResponseHeaderOverrides.ContentLanguage);
                }
                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.ContentType))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseContentType, request.ResponseHeaderOverrides.ContentType);
                }

                if (!string.IsNullOrEmpty(request.ResponseHeaderOverrides.Expires))
                {
                    httpRequest.Params.Add(Constants.ObsRequestParams.ResponseExpires, request.ResponseHeaderOverrides.Expires);
                }
            }

            if (!string.IsNullOrEmpty(request.IfMatch))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.IfMatch, request.IfMatch);
            }

            if (!string.IsNullOrEmpty(request.IfNoneMatch))
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.IfNoneMatch, request.IfNoneMatch);
            }

            if (request.IfModifiedSince.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.IfModifiedSince, request.IfModifiedSince.Value.ToString(Constants.RFC822DateFormat, Constants.CultureInfo));
            }

            if (request.IfUnmodifiedSince.HasValue)
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.IfUnmodifiedSince, request.IfUnmodifiedSince.Value.ToString(Constants.RFC822DateFormat, Constants.CultureInfo));
            }


            if (request.ByteRange != null && request.ByteRange.Start >= 0 && request.ByteRange.Start <= request.ByteRange.End)
            {
                CommonUtil.AddHeader(httpRequest, Constants.CommonHeaders.Range, string.Format("bytes={0}-{1}", request.ByteRange.Start, request.ByteRange.End));
            }

            return httpRequest;
        }

        public HttpRequest Trans(GetApiVersionRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.HEAD;
            httpRequest.BucketName = request.BucketName;
            httpRequest.Params.Add(Constants.SubResourceApiVersion, null);
            return httpRequest;
        }

        public HttpRequest Trans(HeadObjectRequest request)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = HttpVerb.HEAD;
            httpRequest.BucketName = request.BucketName;
            httpRequest.ObjectKey = request.ObjectKey;
            if (!string.IsNullOrEmpty(request.VersionId))
            {
                httpRequest.Params.Add(Constants.ObsRequestParams.VersionId, request.VersionId);
            }
            return httpRequest;
        }
    }
}
