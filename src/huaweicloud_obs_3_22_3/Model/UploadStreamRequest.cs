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
	/// Parameters in a data stream upload request
	/// </summary>
    public class UploadStreamRequest : ResumableUploadRequest
    {
        //UplaodStream*1/2�1/2�"֧�ֶ��̲߳����ϴ"����taskNum����

        private Stream uploadStream;

        internal override string GetAction()
        {
            return "UploadStream";
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UploadStreamRequest()
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public UploadStreamRequest(string bucketName, string objectKey) :base(bucketName, objectKey)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uploadStream">Data stream to be uploaded, which must be queryable</param>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        public UploadStreamRequest(Stream uploadStream, string bucketName, string objectKey) 
            : this(bucketName, objectKey)
        {
            this.UploadStream = uploadStream;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadStream">Data stream to be uploaded, which must be queryable</param>
        /// <param name="partSize">Part size</param>
        public UploadStreamRequest(string bucketName, string objectKey, Stream uploadStream, long partSize)
            :this(uploadStream, bucketName, objectKey)
        {
            this.UploadPartSize = partSize;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadStream">Data stream to be uploaded, which must be queryable</param>
        /// <param name="partSize">Part size</param>
        /// <param name="enableCheckpoint"></param>
        public UploadStreamRequest(string bucketName, string objectKey, Stream uploadStream, long partSize, bool enableCheckpoint)
            : this(bucketName, objectKey, uploadStream, partSize, enableCheckpoint, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="uploadStream">Data stream to be uploaded, which must be queryable</param>
        /// <param name="partSize">Part size</param>
        /// <param name="enableCheckpoint">Whether to use the resumable mode</param>
        /// <param name="checkpointFile">File used to record progresses of resumable uploads</param>
        public UploadStreamRequest(string bucketName, string objectKey, Stream uploadStream, long partSize, bool enableCheckpoint, string checkpointFile)
            : this(bucketName, objectKey)
        { 
            this.UploadPartSize = partSize;
            this.UploadStream = uploadStream;
            this.EnableCheckpoint = enableCheckpoint;
            this.CheckpointFile = checkpointFile;
        }

        /// <summary>
        /// File used to record the upload progress
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter. The default value is the directory under which the current program runs.
        /// </para>
        /// </remarks>
        public override string CheckpointFile
        {
            get;
            set;
        }


        /// <summary>
        /// Data stream to be uploaded, which must be queryable
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public Stream UploadStream
        {
            get { return this.uploadStream; }
            set { this.uploadStream = value; }
        }


    }
}



