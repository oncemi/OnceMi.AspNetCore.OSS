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
    /// Bucket or object owner information
    /// </summary>
    public class Owner
    {
        /// <summary>
        /// Owner name
        /// </summary>
        [Obsolete]
        public string DisplayName { set; get; }

        /// <summary>
        /// ID of the domain to which the owner belongs
        /// </summary>
        public string Id { get; set; }
    }
}


