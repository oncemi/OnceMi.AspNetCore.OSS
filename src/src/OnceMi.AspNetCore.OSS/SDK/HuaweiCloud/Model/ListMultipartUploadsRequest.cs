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
    /// Parameters in a request for listing multipart uploads
    /// </summary>
    public class ListMultipartUploadsRequest : ObsBucketWebServiceRequest
    {


        internal override string GetAction()
        {
            return "ListMultipartUploads";
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
        /// Start position for listing multipart uploads (sorted by object name)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string KeyMarker
        {
            get;
            set;
        }



        /// <summary>
        /// Maximum number of listed multipart uploads  
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// The value ranges from 1to 1000. If there are more than 1000 multipart uploads, only 1000 of them will be listed.
        /// </para>
        /// </remarks>
        public int? MaxUploads
        {
            get;
            set;
        }



        /// <summary>
        /// Object name prefix for listing mulitpart uploads
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
        /// Start position for listing multipart uploads (sorted by multipart upload ID)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when used together with "KeyMarker" and specifies the ID from which the listing begins.
        /// </para>
        /// </remarks>
        public string UploadIdMarker
        {
            get;
            set;
        }

    }
}
    


