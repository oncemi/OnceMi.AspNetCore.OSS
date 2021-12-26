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
    /// Response to an object batch deletion request
    /// </summary>
    public class DeleteObjectsResponse : ObsWebServiceResponse
    {
        private IList<DeletedObject> deleted;
        private IList<DeleteError> errors;

        /// <summary>
        /// List of objects that have been deleted successfully
        /// </summary>
        public IList<DeletedObject> DeletedObjects
        {
            get {
               
                return this.deleted ?? (this.deleted = new List<DeletedObject>());
            }
            internal set { this.deleted = value; }
        }

        /// <summary>
        /// List of objects failed to be deleted
        /// </summary>
        public IList<DeleteError> DeleteErrors
        {
            get {
               
                return this.errors ?? (this.errors = new List<DeleteError>());
            }
            internal set { this.errors = value; }
        }

    }
}
    


