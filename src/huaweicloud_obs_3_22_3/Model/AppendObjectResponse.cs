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
    /// Response to an appendable upload request
    /// </summary>
    public class AppendObjectResponse : ObsWebServiceResponse
    {

        private long _nextPosition = -1;

        /// <summary>
        /// ETag verification value of the appended data 
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Start postion for next appendable upload
        /// </summary>
        public long NextPosition
        {
            get
            {
                return _nextPosition;
            }
            internal set
            {
                this._nextPosition = value;
            }
        }

        /// <summary>
        /// Object storage class
        /// </summary>
        public StorageClassEnum? StorageClass
        {
            get;
            internal set;
        }

        /// <summary>
        /// Full path to the object
        /// </summary>
        public string ObjectUrl
        {
            get;
            internal set;
        }


    }
}
    


