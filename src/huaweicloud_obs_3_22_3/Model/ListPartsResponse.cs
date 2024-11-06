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
using System.Collections.Generic;


namespace OBS.Model
{
    /// <summary>
    /// Response to a request for listing uploaded parts
    /// </summary>
    public class ListPartsResponse : ObsWebServiceResponse
    {

        private IList<PartDetail> parts;

        /// <summary>
        /// Bucket name
        /// </summary>
        public string BucketName
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
        /// Multipart upload ID
        /// </summary>
        public string UploadId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Start position for listing parts
        /// </summary>
        public int? PartNumberMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// Start position for next listing
        /// </summary>
        public int? NextPartNumberMarker
        {
            get;
            internal set;
        }

        /// <summary>
        /// Maximum number of uploaded parts to be listed 
        /// </summary>
        public int? MaxParts
        {
            get;
            internal set;
        }

        /// <summary>
        /// Check whether the listing results are truncated. 
        /// Value "true" indicates that the results are incomplete while value "false" indicates that the results are complete.
        /// </summary>
        public bool IsTruncated
        {
            get;
            internal set;
        }

        /// <summary>
        /// List of uploaded parts
        /// </summary>
        public IList<PartDetail> Parts
        {
            get {
                
                return this.parts ?? (this.parts = new List<PartDetail>()); }
            internal set { this.parts = value; }
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
        /// Owner of the multipart upload
        /// </summary>
        public Owner Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// Storage class of the object generated after the multipart upload is complete
        /// </summary>
        public StorageClassEnum? StorageClass
        {
            get;
            internal set;
        }

    }
}
    


