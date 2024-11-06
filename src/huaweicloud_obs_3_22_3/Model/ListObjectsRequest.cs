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
    /// Parameters in a request for listing objects in a bucket
    /// </summary>
    public class ListObjectsRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "ListObjectsRequest";
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
        /// Start position for listing objects
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// Object name to start with when listing objects in a bucket. All objects are listed in the lexicographical order.
        /// </para>
        /// </remarks>
        public string Marker
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of objects to be listed
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, whose default value is 1000
        /// </para>
        /// </remarks>
        public int? MaxKeys
        {
            get;
            set;
        }



        /// <summary>
        /// Object name prefix, used for filtering objects to be listed
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

    }
}
    


