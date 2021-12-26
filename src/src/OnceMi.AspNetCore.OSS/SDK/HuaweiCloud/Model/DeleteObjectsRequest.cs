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
using System.Xml.Serialization;

namespace OBS.Model
{
    /// <summary>
    /// Parameters in an object batch deletion request
    /// </summary>
    public partial class DeleteObjectsRequest : ObsBucketWebServiceRequest
    {
        private IList<KeyVersion> objects;

        internal override string GetAction()
        {
            return "DeleteObjects";
        }

        /// <summary>
        /// List of objects to be deleted
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public IList<KeyVersion> Objects
        {
            get 
            {
                return this.objects ?? (this.objects = new List<KeyVersion>()); 
            }
            set { this.objects = value; }
        }

        /// <summary>
        /// Response mode of the object batch deletion request
        /// </summary>
        /// <remarks>
        /// <para>
        /// This parameter is optional. Value "false" indicates that the verbose mode is used, and value "true" indicates that the quiet mode is used. The default mode is verbose.
        /// </para>
        /// </remarks>
        public bool? Quiet
        {
            get;
            set;
        }

    }
}
    


