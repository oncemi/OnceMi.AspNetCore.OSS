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
    /// Response to a request for copying an object
    /// </summary>
    public class CopyObjectResponse : ObsWebServiceResponse
    {

        /// <summary>
        /// ETag value of the target object
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Last modification time of the target object
        /// </summary>
        public DateTime? LastModified
        {
            get;
            internal set;
        }

        /// <summary>
        /// Storage class of the target object
        /// </summary>
        public StorageClassEnum? StorageClass
        {
            get;
            internal set;
        }

        /// <summary>
        /// Version ID of the target object
        /// </summary>
        public string SourceVersionId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Version ID of the target object
        /// </summary>
        public string VersionId
        {
            get;
            internal set;
        }

    }
}
    


