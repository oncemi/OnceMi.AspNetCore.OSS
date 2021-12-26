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
    /// Parameters in a request for setting bucket quotas
    /// </summary>
    public class SetBucketQuotaRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "SetBucketQuota";
        }

        /// <summary>
        /// Quota
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The value must be a character string and thus an integer must be converted to be a character string. 
        /// A bucket quota must be expressed in bytes and the maximum value is 263-1. Value "0" indicates that no upper limit is set for the bucket quota.
        /// </para>
        /// </remarks>
        public long StorageQuota
        {
            get;
            set;
        }

    }
}
    


