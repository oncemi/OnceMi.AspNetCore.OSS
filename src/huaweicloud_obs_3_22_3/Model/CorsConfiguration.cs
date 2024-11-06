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
    /// Bucket CORS configuration
    /// </summary>
    public class CorsConfiguration
    {

        private IList<CorsRule> rules;

        /// <summary>
        /// Bucket CORS rule list
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public IList<CorsRule> Rules
        {
            get {
                
                return this.rules ?? (this.rules = new List<CorsRule>()); }
            set { this.rules = value; }
        }

    }
}


