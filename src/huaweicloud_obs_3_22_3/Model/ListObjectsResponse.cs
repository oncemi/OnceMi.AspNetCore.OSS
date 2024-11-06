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
    /// Response to a request for listing objects in a bucket
    /// </summary>
    public class ListObjectsResponse : ObsWebServiceResponse
    {

        private IList<ObsObject> contents;

        private IList<string> commonPrefixes;


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
        /// Start position for this listing
        /// </summary>
        public string Marker
        {
            get;
            internal set;
        }

        /// <summary>
        /// Start position for next listing
        /// </summary>
        public string NextMarker
        {
            get;
            internal set;
        }

        /// <summary>
        /// List of objects in the bucket
        /// </summary>
        public IList<ObsObject> ObsObjects
        {
            get {
                
                return this.contents ?? (this.contents = new List<ObsObject>()); }
            internal set { this.contents = value; }
        }

        /// <summary>
        /// Bucket name
        /// </summary>
        public string BucketName
        {
            get;
            internal set;
        }


        /// <summary>
        /// Object name prefix used in this request
        /// </summary>
        public string Prefix
        {
            get;
            internal set;
        }


        /// <summary>
        /// Maximum number of objects to be listed for this request
        /// </summary>
        public int? MaxKeys
        {
            get;
            internal set;
        }

        /// <summary>
        /// List of prefixes to the names of grouped objects
        /// </summary>
        public IList<string> CommonPrefixes
        {
            get {
                
                return this.commonPrefixes ?? (this.commonPrefixes = new List<string>()); }
            internal set { this.commonPrefixes = value; }
        }


        /// <summary>
        /// Character for grouping object names used in this request
        /// </summary>
        public string Delimiter
        {
            get;
            internal set;
        }

        /// <summary>
        /// Bucket location
        /// </summary>
        public string Location
        {
            get;
            internal set;
        }

    }
   }
    


