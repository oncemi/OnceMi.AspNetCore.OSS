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
    /// Objects that have been successfully deleted in a batch
    /// </summary>
    public class DeletedObject
    {
        

        /// <summary>
        /// Identifier specifying whether an object is deleted
        /// </summary>
        public bool DeleteMarker
        {
            get;
            internal set;
        }

        /// <summary>
        /// Version ID of the delete marker
        /// </summary>
        public string DeleteMarkerVersionId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object name
        /// </summary>
        public string ObjectKey
        {
            get;
            internal set;
        }

        /// <summary>
        /// Object version ID
        /// </summary>
        public string VersionId
        {
            get;
            internal set;
        }

    }
}


