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
using OBS.Model;
using System.Reflection;

namespace OBS.Internal
{

    internal static class EnumAdaptor
    {

        public static IDictionary<Enum, string> EnumValueDict = new Dictionary<Enum, string>();

        private static readonly object _lock = new object();

        public static string GetStringValue(Enum value)
        {
            if (value == null)
            {
                return "";
            }

            string ret;
            EnumValueDict.TryGetValue(value, out ret);
            if (ret == null)
            {
                lock (_lock)
                {
                    EnumValueDict.TryGetValue(value, out ret);
                    if (ret == null)
                    {
                        FieldInfo field = value.GetType().GetField(value.ToString());
                        object[] attribArray = field.GetCustomAttributes(false);
                        StringValueAttribute attrib = attribArray.Length > 0 ? attribArray[0] as StringValueAttribute : null;
                        ret = attrib != null ? attrib.StringValue : value.ToString();
                        EnumValueDict.Add(value, ret);
                    }
                }
            }
            return ret;
        }

        private static volatile IDictionary<string, StorageClassEnum> _V2StorageClassEnumDict;


        public static IDictionary<string, StorageClassEnum> V2StorageClassEnumDict
        {
            get
            {
                if (_V2StorageClassEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_V2StorageClassEnumDict == null)
                        {
                            IDictionary<string, StorageClassEnum>  tempV2StorageClassEnumDict = new Dictionary<string, StorageClassEnum>();
                            tempV2StorageClassEnumDict.Add("STANDARD", StorageClassEnum.Standard);
                            tempV2StorageClassEnumDict.Add("STANDARD_IA", StorageClassEnum.Warm);
                            tempV2StorageClassEnumDict.Add("GLACIER", StorageClassEnum.Cold);
                            _V2StorageClassEnumDict = tempV2StorageClassEnumDict;
                        }
                    }
                }
                return _V2StorageClassEnumDict;
            }
        }



        private static volatile IDictionary<string, StorageClassEnum> _ObsStorageClassEnumDict;

        public static IDictionary<string, StorageClassEnum> ObsStorageClassEnumDict
        {
            get
            {
                if (_ObsStorageClassEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_ObsStorageClassEnumDict == null)
                        {
                            IDictionary<string, StorageClassEnum>  tempObsStorageClassEnumDict = new Dictionary<string, StorageClassEnum>();
                            tempObsStorageClassEnumDict.Add("STANDARD", StorageClassEnum.Standard);
                            tempObsStorageClassEnumDict.Add("WARM", StorageClassEnum.Warm);
                            tempObsStorageClassEnumDict.Add("COLD", StorageClassEnum.Cold);
                            _ObsStorageClassEnumDict = tempObsStorageClassEnumDict;
                        }
                    }
                }
                return _ObsStorageClassEnumDict;
            }
        }


        private static volatile IDictionary<string, GroupGranteeEnum> _V2GroupGranteeEnumDict;

        public static IDictionary<string, GroupGranteeEnum> V2GroupGranteeEnumDict
        {
            get
            {
                if (_V2GroupGranteeEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_V2GroupGranteeEnumDict == null)
                        {
                            IDictionary<string, GroupGranteeEnum> tempV2GroupGranteeEnumDict = new Dictionary<string, GroupGranteeEnum>();
                            tempV2GroupGranteeEnumDict.Add("http://acs.amazonaws.com/groups/global/AllUsers", GroupGranteeEnum.AllUsers);
                            tempV2GroupGranteeEnumDict.Add("http://acs.amazonaws.com/groups/global/AuthenticatedUsers", GroupGranteeEnum.AuthenticatedUsers);
                            tempV2GroupGranteeEnumDict.Add("http://acs.amazonaws.com/groups/s3/LogDelivery", GroupGranteeEnum.LogDelivery);
                            _V2GroupGranteeEnumDict = tempV2GroupGranteeEnumDict;
                        }
                    }
                }
                return _V2GroupGranteeEnumDict;
            }
        }


        private static volatile IDictionary<string, GroupGranteeEnum> _ObsGroupGranteeEnumDict;

        public static IDictionary<string, GroupGranteeEnum> ObsGroupGranteeEnumDict
        {
            get
            {
                if (_ObsGroupGranteeEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_ObsGroupGranteeEnumDict == null)
                        {
                            IDictionary<string, GroupGranteeEnum> tempObsGroupGranteeEnumDict = new Dictionary<string, GroupGranteeEnum>();
                            tempObsGroupGranteeEnumDict.Add("Everyone", GroupGranteeEnum.AllUsers);
                            _ObsGroupGranteeEnumDict = tempObsGroupGranteeEnumDict;
                        }
                    }
                }
                return _ObsGroupGranteeEnumDict;
            }
        }

        private static volatile IDictionary<string, PermissionEnum> _PermissionEnumDict;

        public static IDictionary<string, PermissionEnum> PermissionEnumDict
        {
            get
            {
                if (_PermissionEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_PermissionEnumDict == null)
                        {
                            IDictionary<string, PermissionEnum> tempPermissionEnumDict = new Dictionary<string, PermissionEnum>();
                            tempPermissionEnumDict.Add("READ", PermissionEnum.Read);
                            tempPermissionEnumDict.Add("WRITE", PermissionEnum.Write);
                            tempPermissionEnumDict.Add("READ_ACP", PermissionEnum.ReadAcp);
                            tempPermissionEnumDict.Add("WRITE_ACP", PermissionEnum.WriteAcp);
                            tempPermissionEnumDict.Add("FULL_CONTROL", PermissionEnum.FullControl);
                            _PermissionEnumDict = tempPermissionEnumDict;
                        }
                    }
                }
                return _PermissionEnumDict;
            }
        }

        private static volatile IDictionary<string, HttpVerb> _HttpVerbEnumDict;

        public static IDictionary<string, HttpVerb> HttpVerbEnumDict
        {

            get
            {
                if (_HttpVerbEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_HttpVerbEnumDict == null)
                        {
                            IDictionary<string, HttpVerb>  tempHttpVerbEnumDict = new Dictionary<string, HttpVerb>();
                            tempHttpVerbEnumDict.Add("GET", HttpVerb.GET);
                            tempHttpVerbEnumDict.Add("POST", HttpVerb.POST);
                            tempHttpVerbEnumDict.Add("PUT", HttpVerb.PUT);
                            tempHttpVerbEnumDict.Add("DELETE", HttpVerb.DELETE);
                            tempHttpVerbEnumDict.Add("HEAD", HttpVerb.HEAD);
                            _HttpVerbEnumDict = tempHttpVerbEnumDict;
                        }
                    }
                }
                return _HttpVerbEnumDict;
            }
        }

        private static volatile IDictionary<string, RuleStatusEnum> _RuleStatusEnumDict;

        public static IDictionary<string, RuleStatusEnum> RuleStatusEnumDict
        {
            get
            {
                if (_RuleStatusEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_RuleStatusEnumDict == null)
                        {
                            IDictionary<string, RuleStatusEnum> tempRuleStatusEnumDict = new Dictionary<string, RuleStatusEnum>();
                            tempRuleStatusEnumDict.Add("Enabled", RuleStatusEnum.Enabled);
                            tempRuleStatusEnumDict.Add("Disabled", RuleStatusEnum.Disabled);
                            _RuleStatusEnumDict = tempRuleStatusEnumDict;
                        }
                    }
                }
                return _RuleStatusEnumDict;
            }
        }

        private static volatile IDictionary<string, VersionStatusEnum> _VersionStatusEnumDict;

        public static IDictionary<string, VersionStatusEnum> VersionStatusEnumDict
        {
            get
            {
                if (_VersionStatusEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_VersionStatusEnumDict == null)
                        {
                            IDictionary<string, VersionStatusEnum> tempVersionStatusEnumDict = new Dictionary<string, VersionStatusEnum>();
                            tempVersionStatusEnumDict.Add("Enabled", VersionStatusEnum.Enabled);
                            tempVersionStatusEnumDict.Add("Suspended", VersionStatusEnum.Suspended);
                            _VersionStatusEnumDict = tempVersionStatusEnumDict;
                        }
                    }
                }
                return _VersionStatusEnumDict;
            }
        }

        private static volatile IDictionary<string, ProtocolEnum> _ProtocolEnumDict;

        public static IDictionary<string, ProtocolEnum> ProtocolEnumDict
        {
            get
            {
                if (_ProtocolEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_ProtocolEnumDict == null)
                        {
                            IDictionary<string, ProtocolEnum> tempProtocolEnumDict = new Dictionary<string, ProtocolEnum>();
                            tempProtocolEnumDict.Add("http", ProtocolEnum.Http);
                            tempProtocolEnumDict.Add("https", ProtocolEnum.Https);
                            _ProtocolEnumDict = tempProtocolEnumDict;
                        }
                    }
                }
                return _ProtocolEnumDict;
            }
        }

        private static volatile IDictionary<string, FilterNameEnum> _FilterNameEnumDict;

        public static IDictionary<string, FilterNameEnum> FilterNameEnumDict
        {
            get
            {
                if (_FilterNameEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_FilterNameEnumDict == null)
                        {
                            IDictionary<string, FilterNameEnum> tempFilterNameEnumDict = new Dictionary<string, FilterNameEnum>();
                            tempFilterNameEnumDict.Add("prefix", FilterNameEnum.Prefix);
                            tempFilterNameEnumDict.Add("suffix", FilterNameEnum.Suffix);
                            _FilterNameEnumDict = tempFilterNameEnumDict;
                        }
                    }
                }
                return _FilterNameEnumDict;
            }
        }

        private static volatile IDictionary<string, EventTypeEnum> _V2EventTypeEnumDict;

        public static IDictionary<string, EventTypeEnum> V2EventTypeEnumDict
        {
            get
            {
                if (_V2EventTypeEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_V2EventTypeEnumDict == null)
                        {
                            IDictionary<string, EventTypeEnum> tempV2EventTypeEnumDict = new Dictionary<string, EventTypeEnum>();
                            tempV2EventTypeEnumDict.Add("s3:ObjectCreated:*", EventTypeEnum.ObjectCreatedAll);
                            tempV2EventTypeEnumDict.Add("s3:ObjectCreated:Put", EventTypeEnum.ObjectCreatedPut);
                            tempV2EventTypeEnumDict.Add("s3:ObjectCreated:Post", EventTypeEnum.ObjectCreatedPost);
                            tempV2EventTypeEnumDict.Add("s3:ObjectCreated:Copy", EventTypeEnum.ObjectCreatedCopy);
                            tempV2EventTypeEnumDict.Add("s3:ObjectCreated:CompleteMultipartUpload", EventTypeEnum.ObjectCreatedCompleteMultipartUpload);
                            tempV2EventTypeEnumDict.Add("s3:ObjectRemoved:*", EventTypeEnum.ObjectRemovedAll);
                            tempV2EventTypeEnumDict.Add("s3:ObjectRemoved:Delete", EventTypeEnum.ObjectRemovedDelete);
                            tempV2EventTypeEnumDict.Add("s3:ObjectRemoved:DeleteMarkerCreated", EventTypeEnum.ObjectRemovedDeleteMarkerCreated);
                            _V2EventTypeEnumDict = tempV2EventTypeEnumDict;
                        }
                    }
                }
                return _V2EventTypeEnumDict;
            }
        }

        private static volatile IDictionary<string, EventTypeEnum> _ObsEventTypeEnumDict;

        public static IDictionary<string, EventTypeEnum> ObsEventTypeEnumDict
        {
            get
            {
                if (_ObsEventTypeEnumDict == null)
                {
                    lock (_lock)
                    {
                        if (_ObsEventTypeEnumDict == null)
                        {
                            IDictionary<string, EventTypeEnum> tempObsEventTypeEnumDict = new Dictionary<string, EventTypeEnum>();
                            tempObsEventTypeEnumDict.Add("ObjectCreated:*", EventTypeEnum.ObjectCreatedAll);
                            tempObsEventTypeEnumDict.Add("ObjectCreated:Put", EventTypeEnum.ObjectCreatedPut);
                            tempObsEventTypeEnumDict.Add("ObjectCreated:Post", EventTypeEnum.ObjectCreatedPost);
                            tempObsEventTypeEnumDict.Add("ObjectCreated:Copy", EventTypeEnum.ObjectCreatedCopy);
                            tempObsEventTypeEnumDict.Add("ObjectCreated:CompleteMultipartUpload", EventTypeEnum.ObjectCreatedCompleteMultipartUpload);
                            tempObsEventTypeEnumDict.Add("ObjectRemoved:*", EventTypeEnum.ObjectRemovedAll);
                            tempObsEventTypeEnumDict.Add("ObjectRemoved:Delete", EventTypeEnum.ObjectRemovedDelete);
                            tempObsEventTypeEnumDict.Add("ObjectRemoved:DeleteMarkerCreated", EventTypeEnum.ObjectRemovedDeleteMarkerCreated);
                            _ObsEventTypeEnumDict = tempObsEventTypeEnumDict;
                        }
                    }
                }
                return _ObsEventTypeEnumDict;
            }
        }
    }


    internal class StringValueAttribute : Attribute
    {
        private string _value;

        public StringValueAttribute(string value)
        {
            this._value = value;
        }
        public string StringValue
        {
            get { return this._value; }
        }
    }







}
