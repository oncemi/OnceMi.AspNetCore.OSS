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

namespace OBS.Model
{
    /// <summary>
    /// Authorized user group information
    /// </summary>
    public class GroupGrantee : Grantee
    {

        public GroupGrantee()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupGranteeType">Type of the authorized user group</param>
        public GroupGrantee(GroupGranteeEnum groupGranteeType)
        {
            this.GroupGranteeType = groupGranteeType;
        }

        /// <summary>
        /// Type of the authorized user group
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public GroupGranteeEnum? GroupGranteeType
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

            if(obj.GetType() != this.GetType())
            {
                return false;
            }

            GroupGrantee _obj = obj as GroupGrantee;
            return this.GroupGranteeType == _obj.GroupGranteeType;
        }

        public override int GetHashCode()
        {
            return this.GroupGranteeType.GetHashCode();
        }

    }
}


