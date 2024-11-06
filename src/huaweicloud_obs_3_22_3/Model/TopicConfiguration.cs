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
    /// Event notification configuration
    /// </summary>
    public class TopicConfiguration
    {
        List<EventTypeEnum> _events;
        List<FilterRule> _filterRules;

        /// <summary>
        /// Event notification configuration ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string Id { get; set; }


        /// <summary>
        /// URN of the event notification topic. After detecting a specific event in the bucket, OBS sends a message to the topic.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string Topic { get; set; }

       
        /// <summary>
        /// List of event types that need to be notified
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public List<EventTypeEnum> Events
        { 
            get
            {
                return this._events ?? (this._events = new List<EventTypeEnum>());
            }
            set { this._events = value; } 
        }
        
        /// <summary>
        /// List of filtering rules configured for event notification
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public List<FilterRule> FilterRules
        {
            get
            {
                return this._filterRules ?? (this._filterRules = new List<FilterRule>());
            }
            set { this._filterRules = value; }
        }
    }
}
