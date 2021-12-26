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
    /// Response to a request for obtaining object metadata
    /// </summary>
    public class GetObjectMetadataResponse : ObsWebServiceResponse
    {

        private MetadataCollection metadataCollection;

        private long _nextPosition = -1;


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
        /// Customized metadata of the object
        /// </summary>
        public MetadataCollection Metadata
        {
            get
            {
                return this.metadataCollection ?? (this.metadataCollection = new MetadataCollection());
            }
            set { this.metadataCollection = value; }
        }

        /// <summary>
        /// MIME type of the object
        /// </summary>
        public string ContentType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object length
        /// </summary>
        public override long ContentLength
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
        /// Object delete marker  
        /// </summary>
        public bool DeleteMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// Expiration details of the object
        /// </summary>
        public ExpirationDetail ExpirationDetail
        {
            get;
            internal set;
        }

        /// <summary>
        /// Restore status of the Archive object. If the object is not in the OBS Archive storage class, the value is null.
        /// </summary>
        public RestoreStatus RestoreStatus
        {
            get;
            set;
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
        /// ETag of the object
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }


        /// <summary>
        /// Object version ID
        /// </summary>
        public string VersionId
        {
            get;
            internal set;
        }


        /// <summary>
        /// Redirect the request to another object in the bucket or to an external URL.
        /// If the bucket is configured with website hosting, the object metadata property can be set.
        /// </summary>
        public string WebsiteRedirectLocation
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

        /// <summary>
        /// Start position for next appendable upload. This parameter is valid only when its value is larger than "0" and when "Appendable" is set to "true".
        /// </summary>
        public long NextPosition
        {
            get
            {
                return _nextPosition;
            }
            internal set
            {
                this._nextPosition = value;
            }
        }

    }
}
    


