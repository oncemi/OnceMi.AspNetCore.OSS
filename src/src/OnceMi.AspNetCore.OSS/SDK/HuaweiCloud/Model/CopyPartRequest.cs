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
using System.Collections.Generic;

namespace OBS.Model
{
    /// <summary>
    /// Parameters in the request for copying parts
    /// </summary>
    public class CopyPartRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "CopyPart";
        }

        /// <summary>
        /// Source bucket name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string SourceBucketName
        {
            get;
            set;
        }


        /// <summary>
        /// Source object name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string SourceObjectKey
        {
            get;
            set;
        }


        /// <summary>
        /// Version ID of the source object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string SourceVersionId
        {
            get;
            set;
        }

      

        /// <summary>
        /// Target object name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string ObjectKey
        {
            get;
            set;
        }

        /// <summary>
        /// Multipart upload ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string UploadId
        {
            get;
            set;
        }


        /// <summary>
        /// Target part number
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public int PartNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Copy range of the source object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public ByteRange ByteRange
        {
            get;
            set;
        }


        /// <summary>
        /// SSE-C decryption headers of the source object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public SseCHeader SourceSseCHeader
        {
            get;
            set;
        }

        /// <summary>
        /// SSE-C encryption headers of parts
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public SseCHeader DestinationSseCHeader
        {
            get;
            set;
        }

    }
}
    


