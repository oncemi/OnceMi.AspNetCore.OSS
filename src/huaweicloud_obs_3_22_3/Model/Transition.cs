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
    /// Object transition policy
    /// </summary>
    public class Transition
    {
        

        /// <summary>
        /// Transition date of an object, indicating on which date the object will be transited
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
        /// Object transition time, indicating the number of days after which an object will be transited since its creation 
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


        /// <summary>
        /// Storage class of the object after transition
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public StorageClassEnum? StorageClass
        {
            get;
            set;
        }
    }
}


