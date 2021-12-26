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
using OBS.Internal;
using System;
using System.IO;

namespace OBS.Model
{
    /// <summary>
    /// Parameters in a part upload request
    /// </summary>

    public class UploadPartRequest : ObsBucketWebServiceRequest
    {

        internal override string GetAction()
        {
            return "UploadPart";
        }

        private bool _autoClose = true;

        private double _metric;

        /// <summary>
        /// Mode for presenting the upload progress. The default value is "ByBytes".
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when "UploadProgress" is set.
        /// </para>
        /// </remarks>
        public ProgressTypeEnum ProgressType
        {
            get;
            set;
        }

        /// <summary>
        /// A progress bar is provided to show the progress. The default value is 100 KB or 1 second.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when "UploadProgress" is set.
        /// </para>
        /// </remarks>
        public double ProgressInterval
        {
            get
            {
                return this._metric <= 0 ? (ProgressType == ProgressTypeEnum.ByBytes ? Constants.DefaultProgressUpdateInterval : 1) : this._metric;
            }
            set
            {
                this._metric = value;
            }
        }

        [Obsolete]
        public double ProgressMetric
        {
            get
            {
                return this.ProgressInterval;
            }
            set
            {
                this.ProgressInterval = value;
            }
        }

        /// <summary>
        /// Progress bar
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public EventHandler<TransferStatus> UploadProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to automatically close the input stream. The default value is "true".
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when "InputStream" is set.
        /// </para>
        /// </remarks>
        public bool AutoClose
        {
            set
            {
                this._autoClose = value;
            }
            get
            {
                return this._autoClose;
            }
        }

        /// <summary>
        /// Data stream of the to-be-uploaded part
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which cannot be used with "FilePath".
        /// </para>
        /// </remarks>
        public Stream InputStream
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the source file of the part
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which cannot be used with "InputStream".
        /// </para>
        /// </remarks>
        public string FilePath
        {
            get;
            set;
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
        /// Part number
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter, whose value ranges from 1 to 10000
        /// </para>
        /// </remarks>
        public int PartNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Part size
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. Except for the part lastly being uploaded whose size ranging from 0 to 5 GB, sizes of the other parts range from 100 KB to 5 GB.
        /// </para>
        /// </remarks>
        public long? PartSize
        {
            get;
            set;
        }

        /// <summary>
        /// Multipart upload ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public string UploadId
        {
            get;
            set;
        }


        /// <summary>
        /// Base64-encoded MD5 value of the part to be uploaded, used for consistency verification on the server
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string ContentMd5
        {
            get;
            set;
        }

        /// <summary>
        /// Start offset of the part in the source file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The default value is 0 and the unit is byte.
        /// </para>
        /// </remarks>
        public long? Offset
        {
            get;
            set;
        }

        /// <summary>
        /// SSE-C encryption headers of the part
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// </para>
        /// </remarks>
        public SseCHeader SseCHeader
        {
            get;
            set;
        }


    }
}



