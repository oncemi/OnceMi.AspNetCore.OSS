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
using System.Text;

namespace OBS.Model
{
    /// <summary>
    /// Cross-region replication configuration of a bucket
    /// </summary>
    public class ReplicationConfiguration
    {
        /// <summary>
        /// Agent name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string Agency
        {
            get;
            set;
        }

        private IList<ReplicationRule> rules;

        /// <summary>
        /// List of cross-region replication configuration rules
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public IList<ReplicationRule> Rules
        {
            get
            {

                return this.rules ?? (this.rules = new List<ReplicationRule>());
            }
            set { this.rules = value; }
        }
    }
}


