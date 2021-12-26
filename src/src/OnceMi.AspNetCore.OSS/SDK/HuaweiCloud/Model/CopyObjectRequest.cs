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
    /// Parameters in a request for copying an object
    /// </summary>
    public class CopyObjectRequest : PutObjectBasicRequest
    {

        internal override string GetAction()
        {
            return "CopyObject";
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
        /// Copy the source object if its ETag is the same as the one specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string IfMatch
        {
            get;
            set;
        }


        /// <summary>
        /// Copy the source object if its ETag is different from the one specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string IfNoneMatch
        {
            get;
            set;
        }

        /// <summary>
        /// Copy the source object if it is changed after the time specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public DateTime? IfModifiedSince
        {
            get;
            set;
        }


        /// <summary>
        /// Copy the source object if it is changed before the time specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public DateTime? IfUnmodifiedSince
        {
            get;
            set;
        }


        /// <summary>
        /// Replication policy
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public MetadataDirectiveEnum MetadataDirective
        {
            get;
            set;
        }

        /// <summary>
        /// SSE-C encryption headers of the source object
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

    }
}
    


