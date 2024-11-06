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
    /// Cross-region replication configuration rule
    /// </summary>
    public class ReplicationRule
    {


        /// <summary>
        /// Rule ID, which consists of 1 to 255 characters
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Object name prefix that identifies objects to which the rule applies  
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter, which identifies one or more objects to which the rule applies. The value can be empty, indicating that the rule applies to all objects in the bucket. 
        /// </para>
        /// </remarks>
        public string Prefix
        {
            get;
            set;
        }

        /// <summary>
        /// Rule status
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public RuleStatusEnum Status
        {
            get;
            set;
        }

        /// <summary>
        /// Destination bucket name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string TargetBucketName
        {
            get;
            set;
        }

        /// <summary>
        /// Object storage class
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public StorageClassEnum? TargetStorageClass
        {
            get;
            set;
        }

    }
}


