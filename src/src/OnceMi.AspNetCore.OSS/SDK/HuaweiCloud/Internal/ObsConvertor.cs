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
using System.IO;
using System.Xml;
using OBS.Model;

namespace OBS.Internal
{
    internal class ObsConvertor : V2Convertor
    {

        protected ObsConvertor(IHeaders iheaders) : base(iheaders)
        {

        }

        public new static IConvertor GetInstance(IHeaders iheaders)
        {
            return new ObsConvertor(iheaders);
        }

        protected override string TransStorageClass(StorageClassEnum storageClass)
        {
            return storageClass.ToString().ToUpper();
        }

        protected override string TransEventType(EventTypeEnum eventType)
        {
            return EnumAdaptor.GetStringValue(eventType);
        }

        protected override string TransDestinationBucket(string bucketName)
        {
            return bucketName;
        }

        protected override string TransBucketCannedAcl(CannedAclEnum cannedAcl)
        {
            return EnumAdaptor.GetStringValue(cannedAcl);
        }

        protected override string TransSseKmsAlgorithm(SseKmsAlgorithmEnum algorithm)
        {
            return EnumAdaptor.GetStringValue(algorithm);
        }

        protected override string TransObjectCannedAcl(CannedAclEnum cannedAcl)
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

        protected override string BucketLocationTag
        {
            get
            {
                return "Location";
            }
        }


        protected override string FilterContainerTag
        {
            get
            {
                return "Object";
            }
        }

        protected override string BucketStoragePolicyParam
        {
            get
            {
                return EnumAdaptor.GetStringValue(SubResourceEnum.StorageClass);
            }
        }

        protected override void TransSetBucketStoragePolicyContent(XmlWriter xmlWriter, StorageClassEnum storageClass)
        {
            xmlWriter.WriteElementString("StorageClass", TransStorageClass(storageClass));
        }

        private void TransGrants(XmlWriter xmlWriter, IList<Grant> grants, bool isBucket, string startElementName)
        {
            xmlWriter.WriteStartElement(startElementName);
            foreach (Grant grant in grants)
            {
                if (grant.Grantee != null && grant.Permission.HasValue)
                {
                    xmlWriter.WriteStartElement("Grant");

                    if (grant.Grantee is GroupGrantee)
                    {
                        GroupGrantee groupGrantee = grant.Grantee as GroupGrantee;
                        if (groupGrantee.GroupGranteeType == GroupGranteeEnum.AllUsers)
                        {
                            xmlWriter.WriteStartElement("Grantee");
                            xmlWriter.WriteElementString("Canned", "Everyone");
                            xmlWriter.WriteEndElement();
                        }
                    }
                    else if (grant.Grantee is CanonicalGrantee)
                    {
                        xmlWriter.WriteStartElement("Grantee");
                        CanonicalGrantee canonicalGrantee = grant.Grantee as CanonicalGrantee;
                        xmlWriter.WriteElementString("ID", canonicalGrantee.Id);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteElementString("Permission", EnumAdaptor.GetStringValue(grant.Permission));
                    if (isBucket)
                    {
                        xmlWriter.WriteElementString("Delivered", grant.Delivered.ToString().ToLower());
                    }
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        protected override void TransAccessControlList(HttpRequest httpRequest, AccessControlList acl, bool isBucket)
        {
            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("AccessControlPolicy");
                if (acl.Owner != null && !string.IsNullOrEmpty(acl.Owner.Id))
                {
                    xmlWriter.WriteStartElement("Owner");
                    xmlWriter.WriteElementString("ID", acl.Owner.Id);
                    xmlWriter.WriteEndElement();
                }
                if (!isBucket)
                {
                    xmlWriter.WriteElementString("Delivered", acl.Delivered.ToString().ToLower());
                }
                if (acl.Grants.Count > 0)
                {
                    this.TransGrants(xmlWriter, acl.Grants, isBucket, "AccessControlList");
                }
                xmlWriter.WriteEndElement();
            });
        }

        protected override void TransLoggingConfiguration(HttpRequest httpRequest, LoggingConfiguration configuration)
        {
            this.TransContent(httpRequest, delegate (XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("BucketLoggingStatus");

                if (!string.IsNullOrEmpty(configuration.Agency))
                {
                    xmlWriter.WriteElementString("Agency", configuration.Agency);
                }

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
                        this.TransGrants(xmlWriter, configuration.Grants, false, "TargetGrants");
                    }

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            });
        }

        protected override void TransTier(RestoreTierEnum? tier, XmlWriter xmlWriter)
        {
            if (tier.HasValue && tier.Value != RestoreTierEnum.Bulk)
            {
                xmlWriter.WriteStartElement("RestoreJob");

                xmlWriter.WriteElementString("Tier", tier.Value.ToString());

                xmlWriter.WriteEndElement();
            }
        }

    }
}
