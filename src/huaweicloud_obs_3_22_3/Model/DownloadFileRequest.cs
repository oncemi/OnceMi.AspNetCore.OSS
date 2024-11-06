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

namespace OBS.Model
{
    /// <summary>
    /// Parameters in a request for downloading a file
    /// </summary>
    public class DownloadFileRequest : GetObjectRequest
    {
        internal override string GetAction()
        {
            return "DownloadFile";
        }

       
        private int taskNum = 1;

       
        private long partSize = 9 * 1024 * 1024L;

        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadFileRequest()
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public DownloadFileRequest(string bucketName, string objectKey)
        {
            this.BucketName = bucketName;
            this.ObjectKey = objectKey;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="downloadFile">Full path to which the object is downloaded</param>
        public DownloadFileRequest(string bucketName, string objectKey, string downloadFile)
            : this(bucketName, objectKey)
        {
            this.DownloadFile = downloadFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="downloadFile">Full path to which the object is downloaded</param>
        /// <param name="partSize">Part size</param>
        public DownloadFileRequest(string bucketName, string objectKey, string downloadFile, long partSize)
            :this(bucketName, objectKey)
        {
            this.DownloadFile = downloadFile;
            this.partSize = partSize;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="downloadFile">Full path to which the object is downloaded</param>
        /// <param name="partSize">Part size</param>
        /// <param name="taskNum">Number of threads for uploading parts</param>
        /// <param name="enableCheckpoint">Whether to use the resumable mode</param>
        public DownloadFileRequest(string bucketName, string objectKey, string downloadFile, long partSize, int taskNum,
                bool enableCheckpoint): this(bucketName, objectKey, downloadFile, partSize, taskNum, enableCheckpoint, null)
        {           
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="downloadFile">Full path to which the object is downloaded</param>
        /// <param name="partSize">Part size</param>
        /// <param name="taskNum">Number of threads for uploading parts</param>
        /// <param name="enableCheckpoint">Whether to use resumable upload</param>
        /// <param name="checkpointFile">File used to record the download progress</param>
        public DownloadFileRequest(string bucketName, string objectKey, string downloadFile, long partSize, int taskNum,
                bool enableCheckpoint, string checkpointFile)
            : this(bucketName, objectKey)
        {
            this.partSize = partSize;
            this.DownloadFile = downloadFile;
            this.EnableCheckpoint = enableCheckpoint;
            this.CheckpointFile = checkpointFile;
            this.taskNum = taskNum;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="downloadFile">Full path to which the object is downloaded</param>
        /// <param name="partSize">Part size</param>
        /// <param name="enableCheckpoint">Whether to use the resumable mode</param>
        /// <param name="checkpointFile">File used to record the download progress</param>
        /// <param name="versionId">Object version ID</param>
        public DownloadFileRequest(string bucketName, string objectKey, string downloadFile, long partSize, 
                bool enableCheckpoint, string checkpointFile, string versionId)
            : this(bucketName, objectKey)
        {
            this.partSize = partSize;
            this.DownloadFile = downloadFile;
            this.EnableCheckpoint = enableCheckpoint;
            this.CheckpointFile = checkpointFile;
            this.VersionId = versionId;
        }


        /// <summary>
        /// Download event callback
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public EventHandler<ResumableDownloadEvent> DownloadEventHandler
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of parts that can be concurrently downloaded
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The default value is "1".
        /// </para>
        /// </remarks>
        public int TaskNum
        {
            get { return this.taskNum; }
            set
            {
                if (value < 1)
                    this.taskNum = 1;
                else
                    this.taskNum = value;
            }
        }

        /// <summary>
        /// Part size
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The unit is byte. The value ranges from 100 KB to 5 GB and defaults to 9 MB.
        /// </para>
        /// </remarks>
        public long DownloadPartSize
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
        /// Part size
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The unit is byte. The value ranges from 100 KB to 5 GB and defaults to 5 MB.
        /// </para>
        /// </remarks>
        [Obsolete]
        public long PartSize
        {
            get
            {
                return this.DownloadPartSize;
            }
            set
            {
                this.DownloadPartSize = value;
            }
        }

        /// <summary>
        /// Local path to which the object is downloaded
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. If the value is null, the downloaded object is saved in the directory where the program is executed.
        /// </para>
        /// </remarks>
        public string DownloadFile
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to enable the resumable mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The default value is "false", indicating that the resumable mode is not enabled.
        /// </para>
        /// </remarks>
        public bool EnableCheckpoint
        {
            get;
            set;
        }

        /// <summary>
        /// File used to record the download progress
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which is effective only in the resumable mode. If the value is null, the file is in the same local directory as the downloaded object.
        /// </para>
        /// </remarks>
        public string CheckpointFile
        {
            get;
            set;
        }

        /// <summary>
        /// Temporary file generated during the download
        /// </summary>
        public string TempDownloadFile
        {
            get { return DownloadFile + ".tmp"; }
        }
    }
}



