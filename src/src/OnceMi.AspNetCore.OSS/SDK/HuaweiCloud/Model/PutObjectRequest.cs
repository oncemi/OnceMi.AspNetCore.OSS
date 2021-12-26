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
    /// Parameters in an object upload request
    /// </summary>
    public class PutObjectRequest : PutObjectBasicRequest
    {

        internal override string GetAction()
        {
            return "PutObject";
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
        /// Interval for refreshing the upload progress. The default value is 100 KB or 1 second.
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
        /// Upload progress callback function
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
        /// Data stream to be uploaded
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
        /// Path to the source file
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
        /// Expiration time of a successfully uploaded object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public int? Expires
        {
            get;
            set;
        }

        /// <summary>
        /// Base64-encoded MD5 value of the object content to be uploaded, used for consistency verification on the server
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
        /// Start offset of a part in the source file
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
        /// Size of the object content to be uploaded
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public long? ContentLength
        {
            get;
            set;
        } 

    }
}
    


