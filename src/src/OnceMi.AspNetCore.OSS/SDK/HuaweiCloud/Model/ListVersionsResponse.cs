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
    /// Response to a request for listing versioning objects in a bucket
    /// </summary>
    public class ListVersionsResponse : ObsWebServiceResponse
    {

        private IList<ObsObjectVersion> versions;
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
        /// Start position for this listing (sorted by object name)
        /// </summary>
        public string KeyMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// Start position for this listing (sorted by version ID)
        /// </summary>
        public string VersionIdMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// Start position for next listing (sorted by object name)
        /// </summary>
        public string NextKeyMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// Start position for next listing (sorted by version ID)
        /// </summary>
        public string NextVersionIdMarker
        {
            get;
            internal set;
        }


        /// <summary>
        /// List of versioning objects
        /// </summary>
        public IList<ObsObjectVersion> Versions
        {
            get {
               
                return this.versions ?? (this.versions = new List<ObsObjectVersion>()); }
            internal set { this.versions = value; }
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
    


