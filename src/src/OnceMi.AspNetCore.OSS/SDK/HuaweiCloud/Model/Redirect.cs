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
    /// Redirection configuration
    /// </summary>
    public class Redirect : RedirectBasic
    {

        /// <summary>
        /// Configuration of the HTTP status code
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string HttpRedirectCode
        {
            get;
            set;
        }



        /// <summary>
        /// Object name prefix used for redirecting the request  
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string ReplaceKeyPrefixWith
        {
            get;
            set;
        }



        /// <summary>
        /// Object name used for redirecting the request
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is exclusive with "ReplaceKeyPrefixWith".
        /// </para>
        /// </remarks>
        public string ReplaceKeyWith
        {
            get;
            set;
        }


    }
}


