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

namespace OBS.Internal.Negotiation
{
    internal class AuthTypeCache
    {
        internal class AuthTypeCacheItem
        {
            internal AuthTypeEnum AuthTypeEnum
            {
                get;
                set;
            }

            internal DateTime ExpireDateTime
            {
                get;
                set;
            }

        }
        private const int basicExipreMinutes = 15;
        private IDictionary<string, AuthTypeCacheItem> dict;
        private Random rd;

        public AuthTypeCache()
        {
            dict = new Dictionary<string, AuthTypeCacheItem>();
            rd = new Random();
        }

        public AuthTypeEnum? GetAuthType(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (this.dict.ContainsKey(key))
            {
                AuthTypeCacheItem item = this.dict[key];
                if(item.ExpireDateTime.CompareTo(DateTime.Now) > 0)
                {
                    return item.AuthTypeEnum;
                }
            }
            return null;
        }

        public void RefreshAuthType(string key, AuthTypeEnum authType)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this.dict.Remove(key);
            AuthTypeCacheItem item = new AuthTypeCacheItem();
            item.AuthTypeEnum = authType;
            item.ExpireDateTime = DateTime.Now.AddMinutes(basicExipreMinutes + rd.Next(-5, 5));
            this.dict.Add(key, item);
        }

    }
}
