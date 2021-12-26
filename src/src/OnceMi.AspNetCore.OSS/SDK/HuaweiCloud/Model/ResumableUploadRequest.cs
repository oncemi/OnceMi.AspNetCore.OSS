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

    public abstract class ResumableUploadRequest : PutObjectBasicRequest
    {

        protected long partSize = 1024 * 1024 * 9L;

        protected double _metric;

        internal override string GetAction()
        {
            return "ResumableUpload";
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResumableUploadRequest()
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public ResumableUploadRequest(string bucketName, string objectKey)
        {
            this.BucketName = bucketName;
            this.ObjectKey = objectKey;
        }


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
        /// Upload event callback function
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public EventHandler<ResumableUploadEvent> UploadEventHandler
        {
            get;
            set;
        }
        

        /// <summary>
        /// Part size
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The unit is byte. The value ranges from 100 KB to 5 GB and defaults to 9 MB.
        /// </para>
        /// </remarks>
        public long UploadPartSize
        {
            get { return this.partSize; }
            set
            {
                if (value < 100 * 1024L)
                    this.partSize = 100 * 1024L;
                else if (value > 5 * 1024 * 1024 * 1024L)
                    this.partSize = 5 * 1024 * 1024 * 1024L;
                else
                    this.partSize = value;
            }
        }


        /// <summary>
        /// Identifier specifying whether the resumable mode is enabled
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public bool EnableCheckpoint
        {
            get;
            set;
        }

        /// <summary>
        /// File used to record the upload progress
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. This file is saved in the same directory as the "UploadFile".
        /// </para>
        /// </remarks>
        public virtual string CheckpointFile
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier specifying whether the to-be-uploaded content will be verified
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public bool EnableCheckSum
        {
            get;
            set;
        }


        /// <summary>
        /// Expiration time of the generated object
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

    }
}

