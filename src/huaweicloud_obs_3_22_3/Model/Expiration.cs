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
    /// Expiration time of an object
    /// </summary>
    public class Expiration
    {
        
        /// <summary>
        /// A specified date in which the object will expire 
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this parameter is not set, the "Days" parameter is mandatory.
        /// </para>
        /// </remarks>
        public DateTime? Date
        {
            get;
            set;
        }

        /// <summary>
        /// Object expiration time, specifying how many days after creation will the object expire
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this parameter is not set, the "Date" parameter is mandatory.
        /// </para>
        /// </remarks>
        public int? Days
        {
            get;
            set;
        }
    }
}


