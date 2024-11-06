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
    /// Event type of resumable downloads
    /// </summary>
    public class ResumableDownloadEvent : EventArgs
    {
        internal ResumableDownloadEvent()
        {

        }

        /// <summary>
        /// Event type
        /// </summary>
        public ResumableDownloadEventTypeEnum EventType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Part number
        /// </summary>
        public int PartNumber
        {
            get;
            internal set;
        }

    }

    /// <summary>
    /// Event type of resumable uploads
    /// </summary>
    public class ResumableUploadEvent : EventArgs
    {

        internal ResumableUploadEvent()
        {
           
        }
       
        /// <summary>
        /// Event type
        /// </summary>
        public ResumableUploadEventTypeEnum EventType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Multipart upload ID
        /// </summary>
        public string UploadId
        {
            get;
            internal set;
        }
    
        /// <summary>
        /// Part number
        /// </summary>
        public int PartNumber
        {
            get;
            internal set;
        }

        /// <summary>
        /// ETag value
        /// </summary>
        public string ETag
        {
            get;
            internal set;
        }


    }
}


