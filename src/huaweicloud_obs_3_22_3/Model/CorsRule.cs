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
    /// Bucket CORS rules
    /// </summary>
    public class CorsRule
    {
        private IList<HttpVerb> allowedMethods;
        private IList<string> allowedOrigins;
        private IList<string> exposeHeaders;
        private IList<string> allowedHeaders;

        /// <summary>
        /// Method list of the CORS rules 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public IList<HttpVerb> AllowedMethods
        {
            get {
                
                return this.allowedMethods ?? (this.allowedMethods = new List<HttpVerb>()); }
            set { this.allowedMethods = value; }
        }

        /// <summary>
        /// List of origins (character strings representing domain names) allowed by the CORS rules
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public IList<string> AllowedOrigins
        {
            get {
                
                return this.allowedOrigins ?? (this.allowedOrigins = new List<string>()); }
            set { this.allowedOrigins = value; }
        }

        /// <summary>
        /// CORS rule ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// A rule ID is a string of 1 to 255 characters.
        /// </para>
        /// </remarks>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Response headers allowed by the CORS rules
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public IList<string> ExposeHeaders
        {
            get {
                
                return this.exposeHeaders ?? (this.exposeHeaders = new List<string>()); }
            set { this.exposeHeaders = value; }
        }

        /// <summary>
        /// Cache duration (in seconds) of the request result on the OBS client
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public int? MaxAgeSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Request headers allowed by the CORS rules
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public IList<string> AllowedHeaders
        {
            get {
               
                return this.allowedHeaders ?? (this.allowedHeaders = new List<string>()); }
            set { this.allowedHeaders = value; }
        }

    }
}


