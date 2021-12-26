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
    /// Website hosting configuration of a bucket
    /// </summary>
    public class WebsiteConfiguration
    {

        private IList<RoutingRule> routingRules;
        /// <summary>
        /// Hosting error page
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string ErrorDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Hosting homepage
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. This field will be appended to the end of the request to a folder. (For example, if the parameter value is "index.html" and the requested folder is "samplebucket/images/",
        /// content of object named "images/index.html" in the "samplebucket" bucket will be returned.) The parameter value cannot be blank or contain slashes (/).
        /// </para>
        /// </remarks>
        public string IndexDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Redirection configuration for all requests
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public RedirectBasic RedirectAllRequestsTo
        {
            get;
            set;
        }

        /// <summary>
        /// List of request redirection rules
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. It is used together with "ErrorDocument" and "IndexDocument" and cannot be used together with "RedirectAllRequestsTo."
        /// </para>
        /// </remarks>
        public IList<RoutingRule> RoutingRules
        {
            get {
                
                return this.routingRules ?? (this.routingRules = new List<RoutingRule>()); }
            set { this.routingRules = value; }
        }

    }
}


