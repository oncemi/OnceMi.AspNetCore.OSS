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
    /// Response to the request for combining parts
    /// </summary>
    public class CompleteMultipartUploadResponse : ObsWebServiceResponse
    {

        /// <summary>
        /// URL of the object obtained after part combination
        /// </summary>
        public string Location
        {
            get;
            internal set;
        }

        /// <summary>
        /// Bucket in which parts are combined
        /// </summary>
        public string BucketName
        {
            get;
            internal set;
        }


        /// <summary>
        /// Name of the object obtained after part combination
        /// </summary>
        public string ObjectKey
        {
            get;
            internal set;
        }

        /// <summary>
        /// ETag calculated based on the ETags of all combined parts
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Version ID of the object obtained after part combination
        /// </summary>
        public string VersionId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Full path to the object obtained after part combination
        /// </summary>
        public string ObjectUrl
        {
            get;
            internal set;
        }
    }
}
    


