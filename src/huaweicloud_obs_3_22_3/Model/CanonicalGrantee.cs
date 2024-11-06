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

namespace OBS.Model
{
    /// <summary>
    /// Grantee information
    /// </summary>
    public class CanonicalGrantee : Grantee
    {
        
        public CanonicalGrantee()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID of the domain to which the grantee belongs</param></param>
        public CanonicalGrantee(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Grantee name
        /// </summary>
        [Obsolete]       
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the domain to which the grantee belongs
        /// </summary>
        public string Id
        {
            get;
            set;
        }


        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(this.GetType() != obj.GetType())
            {
                return false;
            }

            CanonicalGrantee _obj = obj as CanonicalGrantee;
            if (string.IsNullOrEmpty(this.Id))
            {
                if (string.IsNullOrEmpty(_obj.Id))
                {
                    return true;
                }
                return false;
            }
            return this.Id.Equals(_obj.Id);        
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(this.Id) ? 0 : this.Id.GetHashCode();
        }
    }
}


