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
    /// Parameters in a request for restoring an Archive object
    /// </summary>
    public class RestoreObjectRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "RestoreObject";
        }

        /// <summary>
        /// Object name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string ObjectKey
        {
            get;
            set;
        }

        /// <summary>
        /// Retention period of the restored object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public int Days
        {
            get;
            set;
        }

        /// <summary>
        /// Object version ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string VersionId
        {
            get;
            set;
        }


        /// <summary>
        /// Restore option
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public RestoreTierEnum? Tier
        {
            get;
            set;
        }

    }
}    


