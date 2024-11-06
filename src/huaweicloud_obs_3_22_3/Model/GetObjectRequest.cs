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
namespace OBS.Model
{
    /// <summary>
    /// Parameters in an object download request
    /// </summary>
    public class GetObjectRequest : GetObjectMetadataRequest
    {

        internal override string GetAction()
        {
            return "GetObject";
        }

        private double _metric;

        /// <summary>
        /// Mode for presenting the download progress. The default value is "ByBytes".
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when "DownloadProgress" is set.
        /// </para>
        /// </remarks>
        public ProgressTypeEnum ProgressType
        {
            get;
            set;
        }

        /// <summary>
        /// Interval for refreshing the download progress. The default value is 100 KB or 1 second.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only when "DownloadProgress" is set.
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

        /// <summary>
        /// Download progress callback function
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public EventHandler<TransferStatus> DownloadProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Return the object if its ETag is the same as the one specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string IfMatch
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the object if it is modified after the time specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public DateTime? IfModifiedSince
        {
            get;
            set;
        }


        /// <summary>
        /// Return the object if its ETag is different from the one specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string IfNoneMatch
        {
            get;
            set;
        }


        /// <summary>
        /// Return the object if it remains unchanged since the time specified by this parameter; otherwise, an error code is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public DateTime? IfUnmodifiedSince
        {
            get;
            set;
        }


       

        /// <summary>
        /// Download range of the object
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public ByteRange ByteRange
        {
            get;
            set;
        }

        /// <summary>
        /// Rewritten response headers
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public ResponseHeaderOverrides ResponseHeaderOverrides
        {
            get;
            set;
        }


        /// <summary>
        /// Image processing parameters
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string ImageProcess
        {
            get;
            set;
        }


    }
}
    


