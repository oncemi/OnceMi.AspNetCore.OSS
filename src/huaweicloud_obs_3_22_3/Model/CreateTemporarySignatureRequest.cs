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
    /// Parameters of a temporary authentication request 
    /// </summary>
    public class CreateTemporarySignatureRequest : ObsBucketWebServiceRequest
    {

        private IDictionary<string, string> headers;
        private MetadataCollection metadataCollection;
        private IDictionary<string, string> parameters;

        internal override string GetAction()
        {
            return "CreateTemporarySignature";
        }

        /// <summary>
        /// Bucket name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public override string BucketName
        {
            get;
            set;
        }

        /// <summary>
        /// Object name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string ObjectKey
        {
            get;
            set;
        }


        /// <summary>
        /// Expiration time, in seconds
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public long? Expires
        {
            get;
            set;
        }



        /// <summary>
        /// Request method
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public HttpVerb Method
        {
            get;
            set;
        }


        /// <summary>
        /// Request headers
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public IDictionary<string, string> Headers
        {
            get
            {
                return this.headers ?? (this.headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
            internal set
            {
                this.headers = value;
            }
        }


        /// <summary>
        /// Customized metadata, which can be used when you upload objects, initialize multipart uploads, and copy objects
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public MetadataCollection Metadata
        {
            get
            {
              
                return this.metadataCollection ?? (this.metadataCollection = new MetadataCollection());
            }
            internal set
            {
                this.metadataCollection = value;
            }
        }

        /// <summary>
        /// Sub-resources
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public SubResourceEnum? SubResource
        {
            get;
            set;
        }

        /// <summary>
        /// Request query parameters
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public IDictionary<String, String> Parameters
        {
            get {
     
                return this.parameters ?? (this.parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
            set
            {
                this.parameters = value;
            }
        }


    }
}

