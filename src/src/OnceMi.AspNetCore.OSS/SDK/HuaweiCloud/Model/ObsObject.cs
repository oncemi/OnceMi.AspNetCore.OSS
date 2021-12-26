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
    /// Object information
    /// </summary>
    public class ObsObject
    {

        /// <summary>
        /// ETag of the object
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object name
        /// </summary>
        public string ObjectKey
        {
            get;
            internal set;
        }

        /// <summary>
        /// Last modification time of the object
        /// </summary>
        public DateTime? LastModified
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object owner
        /// </summary>
        public Owner Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object size
        /// </summary>
        public long Size
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object storage class
        /// </summary>
        public StorageClassEnum? StorageClass
        {
            get;
            internal set;
        }

        /// <summary>
        /// Whether the object is an appendable object
        /// </summary>
        public bool Appendable
        {
            get;
            internal set;
        }

    }
}


