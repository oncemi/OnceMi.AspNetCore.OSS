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
	/// Parameters in a file upload request
	/// </summary>
    public class UploadFileRequest : ResumableUploadRequest
    {
        // 分段上传时的最大并发数，默认为1
        private int taskNum = 1;

        internal override string GetAction()
        {
            return "UploadFile";
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UploadFileRequest()
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public UploadFileRequest(string bucketName, string objectKey) :base(bucketName, objectKey)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uploadFile">To-be-uploaded local file</param>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public UploadFileRequest(string uploadFile,string bucketName, string objectKey) 
            : this(bucketName, objectKey)
        {
            this.UploadFile = uploadFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadFile">To-be-uploaded local file</param>
        /// <param name="partSize">Part size</param>
        public UploadFileRequest(string bucketName, string objectKey, string uploadFile, long partSize)
            :this(uploadFile, bucketName, objectKey)
        {
            this.UploadPartSize = partSize;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadFile">To-be-uploaded local file</param>
        /// <param name="partSize">Part size</param>
        /// <param name="taskNum">Number of upload requests</param>
        /// <param name="enableCheckpoint"></param>
        public UploadFileRequest(string bucketName, string objectKey, string uploadFile, long partSize, int taskNum,
                bool enableCheckpoint)
            : this(bucketName, objectKey, uploadFile, partSize, taskNum, enableCheckpoint, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadFile">To-be-uploaded local file</param>
        /// <param name="partSize">Part size</param>
        /// <param name="taskNum">Number of upload requests</param>
        /// <param name="enableCheckpoint">Whether to enabled the resumable mode</param>
        /// <param name="checkpointFile">File used to record progresses of resumable uploads</param>
        public UploadFileRequest(string bucketName, string objectKey, string uploadFile, long partSize, int taskNum,
                bool enableCheckpoint, string checkpointFile)
            : this(bucketName, objectKey)
        { 
            this.UploadPartSize = partSize;
            this.UploadFile = uploadFile;
            this.EnableCheckpoint = enableCheckpoint;
            this.CheckpointFile = checkpointFile;
            this.TaskNum = taskNum;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadFile">To-be-uploaded local file</param>
        /// <param name="partSize">Part size</param>
        /// <param name="taskNum">Number of upload requests</param>
        /// <param name="enableCheckpoint">Whether to enabled the resumable mode</param>
        /// <param name="checkpointFile">File used to record progresses of resumable uploads</param>
        /// <param name="enableCheckSum">Whether to verify the to-be-uploaded file upon non-initial uploads in resumable upload mode</param>
        public UploadFileRequest(string bucketName, string objectKey, string uploadFile, long partSize, int taskNum,
                bool enableCheckpoint, string checkpointFile, bool enableCheckSum)
            : this(bucketName, objectKey, uploadFile, partSize, taskNum, enableCheckpoint, checkpointFile)
        {   
            this.EnableCheckSum = enableCheckSum;
        }

        /// <summary>
        /// To-be-uploaded local file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public string UploadFile
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum number of parts that can be concurrently uploaded
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, whose default value is 1
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


    }
}

