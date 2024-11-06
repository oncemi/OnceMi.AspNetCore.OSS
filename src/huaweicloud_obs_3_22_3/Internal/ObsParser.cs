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
using OBS.Model;


namespace OBS.Internal
{
    internal class ObsParser : V2Parser
    {
        private ObsParser(IHeaders iheaders) : base(iheaders)
        {

        }

        public new static IParser GetInstance(IHeaders iheaders)
        {
            return new ObsParser(iheaders);
        }

        protected override StorageClassEnum? ParseStorageClass(string value)
        {
            return EnumAdaptor.ObsStorageClassEnumDict.ContainsKey(value) ? EnumAdaptor.ObsStorageClassEnumDict[value] : (StorageClassEnum?)null;
        }

        protected override GroupGranteeEnum? ParseGroupGrantee(string value)
        {
            return EnumAdaptor.ObsGroupGranteeEnumDict.ContainsKey(value) ? EnumAdaptor.ObsGroupGranteeEnumDict[value] : (GroupGranteeEnum?)null;
        }

        protected override EventTypeEnum? ParseEventTypeEnum(string value)
        {
            return EnumAdaptor.ObsEventTypeEnumDict.ContainsKey(value) ? EnumAdaptor.ObsEventTypeEnumDict[value] : (EventTypeEnum?)null;
        }

        protected override string BucketLocationTag
        {
            get
            {
                return "Location";
            }
        }

        protected override string BucketStorageClassTag
        {
            get
            {
                return "StorageClass";
            }
        }

        private AccessControlList ParseAccessControlList(HttpResponse httpResponse, bool isBucket)
        {
            using (XmlReader xmlReader = XmlReader.Create(httpResponse.Content))
            {
                AccessControlList acl = new AccessControlList();
                bool innerOwner = false;
                Grant currentGrant = null;
                while (xmlReader.Read())
                {
                    if ("Owner".Equals(xmlReader.Name))
                    {
                        if (xmlReader.IsStartElement())
                        {
                            acl.Owner = new Owner();
                        }
                        innerOwner = xmlReader.IsStartElement();
                    }
                    else if ("ID".Equals(xmlReader.Name))
                    {
                        if (innerOwner)
                        {
                            acl.Owner.Id = xmlReader.ReadString();
                        }
                        else
                        {
                            CanonicalGrantee grantee = new CanonicalGrantee();
                            grantee.Id = xmlReader.ReadString();
                            currentGrant.Grantee = grantee;
                        }
                    }
                    else if ("Grant".Equals(xmlReader.Name))
                    {
                        if (xmlReader.IsStartElement())
                        {
                            currentGrant = new Grant();
                            acl.Grants.Add(currentGrant);
                        }
                    }
                    else if ("Canned".Equals(xmlReader.Name))
                    {
                        GroupGrantee grantee = new GroupGrantee();
                        grantee.GroupGranteeType = this.ParseGroupGrantee(xmlReader.ReadString());
                        currentGrant.Grantee = grantee;
                    }
                    else if ("Permission".Equals(xmlReader.Name))
                    {
                        currentGrant.Permission = this.ParsePermission(xmlReader.ReadString());
                    }
                    else if ("Delivered".Equals(xmlReader.Name))
                    {
                        if (isBucket)
                        {
                            currentGrant.Delivered = Convert.ToBoolean(xmlReader.ReadString());
                        }
                        else
                        {
                            acl.Delivered = Convert.ToBoolean(xmlReader.ReadString());
                        }
                    }
                }
                return acl;
            }
        }

        public override GetBucketAclResponse ParseGetBucketAclResponse(HttpResponse httpResponse)
        {
            GetBucketAclResponse response = new GetBucketAclResponse();
            response.AccessControlList = this.ParseAccessControlList(httpResponse, true);
            return response;
        }

        public override GetObjectAclResponse ParseGetObjectAclResponse(HttpResponse httpResponse)
        {
            GetObjectAclResponse response = new GetObjectAclResponse();
            response.AccessControlList = this.ParseAccessControlList(httpResponse, false);
            return response;
        }

        public override GetBucketLoggingResponse ParseGetBucketLoggingResponse(HttpResponse httpResponse)
        {
            GetBucketLoggingResponse response = new GetBucketLoggingResponse();

            using (XmlReader xmlReader = XmlReader.Create(httpResponse.Content))
            {
                Grant currentGrant = null;
                while (xmlReader.Read())
                {
                    if ("BucketLoggingStatus".Equals(xmlReader.Name))
                    {
                        if (xmlReader.IsStartElement())
                        {
                            response.Configuration = new LoggingConfiguration();
                        }
                    }
                    else if ("Agency".Equals(xmlReader.Name))
                    {
                        response.Configuration.Agency = xmlReader.ReadString();
                    }
                    else if ("TargetBucket".Equals(xmlReader.Name))
                    {
                        response.Configuration.TargetBucketName = xmlReader.ReadString();
                    }
                    else if ("TargetPrefix".Equals(xmlReader.Name))
                    {
                        response.Configuration.TargetPrefix = xmlReader.ReadString();
                    }
                    else if ("Grant".Equals(xmlReader.Name))
                    {
                        if (xmlReader.IsStartElement())
                        {
                            currentGrant = new Grant();
                            response.Configuration.Grants.Add(currentGrant);
                        }
                    }
                    else if ("ID".Equals(xmlReader.Name))
                    {
                        CanonicalGrantee grantee = new CanonicalGrantee();
                        grantee.Id = xmlReader.ReadString();
                        currentGrant.Grantee = grantee;
                    }
                    else if ("Canned".Equals(xmlReader.Name))
                    {
                        GroupGrantee grantee = new GroupGrantee();
                        grantee.GroupGranteeType = this.ParseGroupGrantee(xmlReader.ReadString());
                        currentGrant.Grantee = grantee;
                    }
                    else if ("Permission".Equals(xmlReader.Name))
                    {
                        currentGrant.Permission = this.ParsePermission(xmlReader.ReadString());
                    }
                }
            }
            return response;
        }

    }
}
