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
    /// Multipart upload
    /// </summary>
    public class MultipartUpload
    {


        /// <summary>
        /// Creation time of the multipart upload 
        /// </summary>
        public DateTime? Initiated
        {
            get;
            internal set;
        }

        /// <summary>
        /// Creator of the multipart upload
        /// </summary>
        public Initiator Initiator
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
        /// Owner of the multipart upload
        /// </summary>
        public Owner Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// Storage class of the object uploaded in multipart mode  
        /// </summary>
        public StorageClassEnum? StorageClass
        {
            get;
            internal set;
        }

        /// <summary>
        /// Multipart upload ID
        /// </summary>
        public string UploadId
        {
            get;
            internal set;
        }

    }
}


