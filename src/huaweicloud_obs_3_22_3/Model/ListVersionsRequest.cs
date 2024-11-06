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
    /// Parameters in a request for listing versioning objects in a bucket
    /// </summary>
    public class ListVersionsRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "ListVersions";
        }

        /// <summary>
        /// Character for grouping object names
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// If the object name contains the "Delimiter" parameter, the character string from the first character to the first delimiter in the object name is grouped under a single result element, "CommonPrefix". 
        /// (If a prefix is specified in the request, the prefix must be removed from the object name.)
        /// </para>
        /// </remarks>
        public string Delimiter
        {
            get;
            set;
        }



        /// <summary>
        /// Start position for listing versioning objects (sorted by object name)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// This parameter specifies the object name from which the listing begins. Listed objects are sorted by object name in lexicographical order.
        /// </para>
        /// </remarks>
        public string KeyMarker
        {
            get;
            set;
        }



        /// <summary>
        /// Maximum number of versioning objects to be listed
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// The value ranges from 1to 1000. If there are more than 1000 versioning objects, only 1000 of them will be listed.
        /// </para>
        /// </remarks>
        public int? MaxKeys
        {
            get;
            set;
        }


        /// <summary>
        /// Object name prefix used for listing versioning objects
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string Prefix
        {
            get;
            set;
        }



        /// <summary>
        /// Start position for listing versioning objects (sorted by version ID)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// This parameter works together with "KeyMarker" and specifies the object name from which the listing begins. Listed objects are sorted by object name and version ID in lexicographical order.
        /// If the values of "VersionIdMarker" and "KeyMarker" are inconsistent, this parameter is ineffective.
        /// </para>
        /// </remarks>
        public string VersionIdMarker
        {
            get;
            set;
        }

    }
}



