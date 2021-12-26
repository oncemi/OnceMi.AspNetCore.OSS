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
using OBS.Internal.Log;
using OBS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net;

namespace OBS
{
    public partial class ObsClient
    {

        #region UploadFile
        /// <summary>
        /// Upload a file in resumable mode.
        /// </summary>
        /// <param name="request">Parameters in a request for uploading a file</param>
        /// <returns>Response to the request for combining parts</returns>
        public CompleteMultipartUploadResponse UploadFile(UploadFileRequest request)
        {
            ParameterJudgment(request);
            if (request.EnableCheckpoint)
            {
                if (string.IsNullOrEmpty(request.CheckpointFile))
                {
                    request.CheckpointFile = request.UploadFile + ".uploadFile_record";
                }
            }
            if (string.IsNullOrEmpty(request.UploadFile) || !File.Exists(request.UploadFile))
            {
                if (File.Exists(request.CheckpointFile))
                {
                    File.Delete(request.CheckpointFile);
                }
                throw new ObsException("The UploadFile is not exist.", ErrorType.Sender, "NoSuchUploadFile", "");
            }

            return ResumableUploadExcute(request);
        }

        /// <summary>
        /// Upload data streams in resumable mode.
        /// </summary>
        /// <param name="request">Parameters in a request for uploading data streams</param>
        /// <returns>Response to the request for combining parts</returns>
        public CompleteMultipartUploadResponse UploadStream(UploadStreamRequest request)
        {
            ParameterJudgment(request);
            if (request.UploadStream == null)
            {
                throw new ObsException("The UploadStream is null.", ErrorType.Sender, "NullUploadStream", "");
            }

            if (!request.UploadStream.CanSeek)
            {
                throw new ObsException("The UploadStream is not seekable.", ErrorType.Sender, "NotSeekableUploadStream", "");
            }

            if (request.EnableCheckpoint)
            {
                if (string.IsNullOrEmpty(request.CheckpointFile))
                {
                    request.CheckpointFile = Environment.CurrentDirectory + "/" + request.ObjectKey + ".uploadFile_record";
                }
            }

            return ResumableUploadExcute(request);
        }

        /// <summary>
        /// Verify parameters. 
        /// </summary>
        /// <param name="request"></param>
        private void ParameterJudgment(ResumableUploadRequest request)
        {
            if (request == null)
            {
                throw new ObsException(Constants.NullRequestMessage, ErrorType.Sender, Constants.NullRequest, "");
            }
            if (string.IsNullOrEmpty(request.BucketName))
            {
                throw new ObsException(Constants.InvalidBucketNameMessage, ErrorType.Sender, Constants.InvalidBucketName, "");
            }
            if (string.IsNullOrEmpty(request.ObjectKey))
            {
                throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
            }
        }


        /// <summary>
        /// Execution method of the resumable upload
        /// </summary>
        /// <param name="resumableUploadRequest"></param>
        /// <returns></returns>
        private CompleteMultipartUploadResponse ResumableUploadExcute(ResumableUploadRequest resumableUploadRequest)
        {
            UploadCheckPoint uploadCheckPoint = new UploadCheckPoint();

            ResumableUploadTypeEnum uploadType;
            if (resumableUploadRequest is UploadFileRequest)
            {
                uploadType = ResumableUploadTypeEnum.UploadFile;
            }
            else
            {
                uploadType = ResumableUploadTypeEnum.UploadStream;
                uploadCheckPoint.UploadStream = (resumableUploadRequest as UploadStreamRequest).UploadStream;
            }


            if (resumableUploadRequest.EnableCheckpoint)
            {
                bool loadFileFlag = true;
                bool paraVerifyFlag = true;
                bool fileVerifyFlag = true;

                if (!File.Exists(resumableUploadRequest.CheckpointFile))
                {
                    loadFileFlag = false;
                }
                else
                {
                    try
                    {

                        uploadCheckPoint.Load(resumableUploadRequest.CheckpointFile);
                    }

                    catch (Exception ex)
                    {
                        LoggerMgr.Warn(string.Format("Load checkpoint file with path {0} error", resumableUploadRequest.CheckpointFile), ex);
                        loadFileFlag = false;
                    }
                }


                if (loadFileFlag)
                {

                    if (uploadType == ResumableUploadTypeEnum.UploadFile)
                    {
                        UploadFileRequest uploadFileRequest = resumableUploadRequest as UploadFileRequest;

                        if (!(uploadFileRequest.BucketName.Equals(uploadCheckPoint.BucketName)
                            && uploadFileRequest.ObjectKey.Equals(uploadCheckPoint.ObjectKey)
                            && uploadFileRequest.UploadFile.Equals(uploadCheckPoint.UploadFile)))
                        {
                            paraVerifyFlag = false;
                        }

                        else
                        {

                            fileVerifyFlag = uploadCheckPoint.IsValid(uploadFileRequest.UploadFile, null, uploadFileRequest.EnableCheckSum, uploadType);
                        }
                    }

                    else
                    {
                        UploadStreamRequest uploadStreamRequest = resumableUploadRequest as UploadStreamRequest;

                        if (!(uploadStreamRequest.BucketName.Equals(uploadCheckPoint.BucketName)
                            && uploadStreamRequest.ObjectKey.Equals(uploadCheckPoint.ObjectKey)))
                        {
                            paraVerifyFlag = false;
                        }

                        else
                        {

                            fileVerifyFlag = uploadCheckPoint.IsValid(null, uploadStreamRequest.UploadStream, uploadStreamRequest.EnableCheckSum, uploadType);
                        }
                    }
                }


                if (!loadFileFlag || !paraVerifyFlag || !fileVerifyFlag)
                {

                    if (loadFileFlag && !string.IsNullOrEmpty(uploadCheckPoint.BucketName) && !string.IsNullOrEmpty(uploadCheckPoint.ObjectKey) &&
                        !string.IsNullOrEmpty(uploadCheckPoint.UploadId))
                    {

                        AbortMultipartUpload(uploadCheckPoint);
                    }


                    if (File.Exists(resumableUploadRequest.CheckpointFile))
                    {
                        File.Delete(resumableUploadRequest.CheckpointFile);
                    }


                    ResumableUploadPrepare(resumableUploadRequest, uploadCheckPoint);
                }
            }

            else
            {

                ResumableUploadPrepare(resumableUploadRequest, uploadCheckPoint);
            }

            TransferStreamManager mgr = null;
            try
            {

                IList<PartResultUpload> partResultsUpload = ResumableUploadBegin(resumableUploadRequest, uploadCheckPoint, out mgr, uploadType);



                foreach (PartResultUpload result in partResultsUpload)
                {

                    if (result.IsFailed && result.Exception != null)
                    {

                        if (!resumableUploadRequest.EnableCheckpoint)
                        {
                            AbortMultipartUpload(uploadCheckPoint);
                        }

                        else if (uploadCheckPoint.IsUploadAbort)
                        {
                            AbortMultipartUpload(uploadCheckPoint);


                            if (File.Exists(resumableUploadRequest.CheckpointFile))
                            {
                                File.Delete(resumableUploadRequest.CheckpointFile);
                            }
                        }

                        throw result.Exception;
                    }
                }
            }
            finally
            {
                if (mgr is ThreadSafeTransferStreamByBytes)
                {
                    mgr.TransferEnd();
                }
            }



            CompleteMultipartUploadRequest completeMultipartUploadRequest = new CompleteMultipartUploadRequest
            {
                BucketName = resumableUploadRequest.BucketName,
                ObjectKey = resumableUploadRequest.ObjectKey,
                UploadId = uploadCheckPoint.UploadId,
                PartETags = uploadCheckPoint.PartEtags
            };
            try
            {
                CompleteMultipartUploadResponse completeMultipartUploadResponse = CompleteMultipartUpload(completeMultipartUploadRequest);
                if (resumableUploadRequest.UploadEventHandler != null)
                {
                    ResumableUploadEvent e = new ResumableUploadEvent();
                    e.EventType = ResumableUploadEventTypeEnum.CompleteMultipartUploadSucceed;
                    e.UploadId = uploadCheckPoint.UploadId;
                    e.ETag = completeMultipartUploadResponse.ETag;
                    resumableUploadRequest.UploadEventHandler(this, e);
                }
                if (resumableUploadRequest.EnableCheckpoint)
                {

                    if (File.Exists(resumableUploadRequest.CheckpointFile))
                    {
                        File.Delete(resumableUploadRequest.CheckpointFile);
                    }
                }
                return completeMultipartUploadResponse;
            }

            catch (ObsException ex)
            {

                if (!resumableUploadRequest.EnableCheckpoint)
                {
                    AbortMultipartUpload(uploadCheckPoint);
                }

                else
                {

                    if (ex.StatusCode >= HttpStatusCode.BadRequest && ex.StatusCode < HttpStatusCode.InternalServerError)
                    {
                        AbortMultipartUpload(uploadCheckPoint);


                        if (File.Exists(resumableUploadRequest.CheckpointFile))
                        {
                            File.Delete(resumableUploadRequest.CheckpointFile);
                        }
                    }
                }

                if (resumableUploadRequest.UploadEventHandler != null)
                {
                    ResumableUploadEvent e = new ResumableUploadEvent();
                    e.EventType = ResumableUploadEventTypeEnum.CompleteMultipartUploadFailed;
                    e.UploadId = uploadCheckPoint.UploadId;
                    resumableUploadRequest.UploadEventHandler(this, e);
                }


                throw ex;
            }
        }

        /// <summary>
        /// Prepare for uploading a file.
        /// </summary>
        /// <param name="resumableUploadRequest"></param>
        /// <param name="uploadCheckPoint"></param>
        private void ResumableUploadPrepare(ResumableUploadRequest resumableUploadRequest, UploadCheckPoint uploadCheckPoint)
        {

            uploadCheckPoint.BucketName = resumableUploadRequest.BucketName;
            uploadCheckPoint.ObjectKey = resumableUploadRequest.ObjectKey;
            long originPosition = 0;
            if (resumableUploadRequest is UploadFileRequest)
            {
                UploadFileRequest uploadFileRequest = resumableUploadRequest as UploadFileRequest;
                uploadCheckPoint.UploadFile = uploadFileRequest.UploadFile;
                uploadCheckPoint.FileStatus = FileStatus.getFileStatus(uploadFileRequest.UploadFile, null, uploadFileRequest.EnableCheckSum, ResumableUploadTypeEnum.UploadFile);
            }
            else
            {
                UploadStreamRequest uploadStreamRequest = resumableUploadRequest as UploadStreamRequest;
                uploadCheckPoint.UploadStream = uploadStreamRequest.UploadStream;
                uploadCheckPoint.FileStatus = FileStatus.getFileStatus(null, uploadStreamRequest.UploadStream, uploadStreamRequest.EnableCheckSum, ResumableUploadTypeEnum.UploadStream);
                originPosition = uploadCheckPoint.UploadStream.Position;
            }

            uploadCheckPoint.UploadParts = SplitUploadFile(uploadCheckPoint.FileStatus.Size, resumableUploadRequest.UploadPartSize, originPosition);
            uploadCheckPoint.PartEtags = new List<PartETag>();


            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest()
            {
                BucketName = resumableUploadRequest.BucketName,
                ObjectKey = resumableUploadRequest.ObjectKey,
                Metadata = resumableUploadRequest.Metadata,
                CannedAcl = resumableUploadRequest.CannedAcl,
                StorageClass = resumableUploadRequest.StorageClass,
                WebsiteRedirectLocation = resumableUploadRequest.WebsiteRedirectLocation,
                Expires = resumableUploadRequest.Expires,
                ContentType = resumableUploadRequest.ContentType,
                SuccessRedirectLocation = resumableUploadRequest.SuccessRedirectLocation,
                SseHeader = resumableUploadRequest.SseHeader
            };

            foreach (var entry in resumableUploadRequest.ExtensionPermissionMap)
            {
                initiateRequest.ExtensionPermissionMap.Add(entry.Key, entry.Value);
            }
            InitiateMultipartUploadResponse initiateResponse;
            try
            {
                initiateResponse = InitiateMultipartUpload(initiateRequest);
            }
            catch (ObsException ex)
            {
                if (resumableUploadRequest.UploadEventHandler != null)
                {
                    ResumableUploadEvent e = new ResumableUploadEvent();
                    e.EventType = ResumableUploadEventTypeEnum.InitiateMultipartUploadFailed;
                    resumableUploadRequest.UploadEventHandler(this, e);
                }
                throw ex;
            }


            uploadCheckPoint.UploadId = initiateResponse.UploadId;


            if (resumableUploadRequest.EnableCheckpoint)
            {
                try
                {
                    uploadCheckPoint.Record(resumableUploadRequest.CheckpointFile);
                }

                catch (Exception ex)
                {

                    AbortMultipartUpload(uploadCheckPoint);


                    ObsException exception = new ObsException(ex.Message, ex);
                    exception.ErrorType = ErrorType.Sender;
                    throw exception;
                }
            }
            if (resumableUploadRequest.UploadEventHandler != null)
            {
                ResumableUploadEvent e = new ResumableUploadEvent();
                e.UploadId = uploadCheckPoint.UploadId;
                e.EventType = ResumableUploadEventTypeEnum.InitiateMultipartUploadSucceed;
                resumableUploadRequest.UploadEventHandler(this, e);
            }
        }

        /// <summary>
        /// Calculate the part list based on the file size and part size.
        /// </summary>
        /// <param name="fileLength"></param>
        /// <param name="partSize"></param>
        /// <returns></returns>
        private List<UploadPart> SplitUploadFile(long fileLength, long partSize, long originPosition)
        {
            List<UploadPart> parts = new List<UploadPart>();

            int partNumber = Convert.ToInt32(fileLength / partSize);

            if (partNumber >= 10000)
            {
                partSize = fileLength % 10000 == 0 ? fileLength / 10000 : fileLength / 10000 + 1;
                partNumber = Convert.ToInt32(fileLength / partSize);
            }

            if (fileLength % partSize > 0)
                partNumber++;

            if (partNumber == 0)
            {
                parts.Add(new UploadPart()
                {
                    PartNumber = 1,
                    Offset = 0,
                    Size = 0,
                    IsCompleted = false
                });
            }
            else
            {
                for (int i = 0; i < partNumber; i++)
                {
                    parts.Add(new UploadPart()
                    {
                        PartNumber = i + 1,
                        Offset = i * partSize + originPosition,
                        Size = partSize,
                        IsCompleted = false
                    });
                }
                if (fileLength % partSize > 0)
                    parts[parts.Count - 1].Size = fileLength % partSize;
            }

            return parts;
        }

        /// <summary>
        /// Results of the part upload
        /// </summary>
        internal class PartResultUpload
        {
            public bool IsFailed { get; set; }

            public ObsException Exception { get; set; }
        }

        internal class UploadPartExcuteParam
        {
            internal ResumableUploadTypeEnum uploadType;
            internal UploadPart uploadPart;
            internal UploadCheckPoint uploadCheckPoint;
            internal PartResultUpload partResultUpload;
            internal ManualResetEvent executeEvent;
            internal string checkpointFile;
            internal bool enableCheckpoint;
            internal TransferStreamManager mgr;
            internal EventHandler<ResumableUploadEvent> eventHandler;
        }

        private void UploadPartExcute(object state)
        {
            UploadPartExcuteParam param = state as UploadPartExcuteParam;

            UploadPart uploadPart = param.uploadPart;

            PartResultUpload partResultUpload = new PartResultUpload();

            UploadCheckPoint uploadCheckPoint = param.uploadCheckPoint;

            try
            {
                if (!uploadCheckPoint.IsUploadAbort)
                {
                    UploadPartRequest uploadPartRequest = new UploadPartRequest()
                    {
                        BucketName = uploadCheckPoint.BucketName,
                        ObjectKey = uploadCheckPoint.ObjectKey,
                        UploadId = uploadCheckPoint.UploadId,
                        PartNumber = uploadPart.PartNumber,
                        PartSize = uploadPart.Size,
                        AutoClose = false
                    };


                    UploadPartResponse uploadPartResponse = null;

                    if (param.uploadType == ResumableUploadTypeEnum.UploadFile)
                    {
                        if (param.mgr != null)
                        {
                            using (TransferStream stream = new TransferStream(new FileStream(uploadCheckPoint.UploadFile, FileMode.Open, FileAccess.Read)))
                            {
                                stream.Seek(uploadPart.Offset, SeekOrigin.Begin);
                                stream.BytesReaded += param.mgr.BytesTransfered;
                                stream.StartRead += param.mgr.TransferStart;
                                stream.BytesReset += param.mgr.TransferReset;
                                uploadPartRequest.InputStream = stream;
                                uploadPartResponse = UploadPart(uploadPartRequest);
                            }
                        }
                        else
                        {
                            uploadPartRequest.Offset = uploadPart.Offset;
                            uploadPartRequest.FilePath = uploadCheckPoint.UploadFile;
                            uploadPartResponse = UploadPart(uploadPartRequest);
                        }
                    }

                    else
                    {
                        if (param.mgr != null)
                        {
                            TransferStream stream = new TransferStream(uploadCheckPoint.UploadStream);
                            stream.Seek(uploadPart.Offset, SeekOrigin.Begin);
                            stream.BytesReaded += param.mgr.BytesTransfered;
                            stream.StartRead += param.mgr.TransferStart;
                            uploadPartRequest.InputStream = stream;
                            uploadPartResponse = UploadPart(uploadPartRequest);
                        }
                        else
                        {
                            uploadCheckPoint.UploadStream.Seek(uploadPart.Offset, SeekOrigin.Begin);
                            uploadPartRequest.Offset = uploadPart.Offset;
                            uploadPartRequest.InputStream = uploadCheckPoint.UploadStream;
                            uploadPartResponse = UploadPart(uploadPartRequest);
                        }
                    }

                    PartETag partEtag = new PartETag(uploadPartResponse.PartNumber, uploadPartResponse.ETag);

                    partResultUpload.IsFailed = false;
                    uploadPart.IsCompleted = true;
                    lock (param.uploadCheckPoint.uploadlock)
                    {
                        uploadCheckPoint.PartEtags.Add(partEtag);
                        if (param.enableCheckpoint)
                        {
                            uploadCheckPoint.Record(param.checkpointFile);
                        }
                    }

                    if (param.eventHandler != null)
                    {
                        ResumableUploadEvent e = new ResumableUploadEvent();
                        e.EventType = ResumableUploadEventTypeEnum.UploadPartSucceed;
                        e.UploadId = uploadCheckPoint.UploadId;
                        e.PartNumber = uploadPart.PartNumber;
                        e.ETag = partEtag.ETag;
                        param.eventHandler(this, e);
                    }

                    if (LoggerMgr.IsDebugEnabled)
                    {
                        LoggerMgr.Debug(string.Format("PartNumber {0} is done, PartSize {1}, Offset {2}", uploadPart.PartNumber,
                            uploadPart.Size, uploadPart.Offset));
                    }

                }
                else
                {
                    partResultUpload.IsFailed = true;
                }
            }
            catch (ObsException ex)
            {

                if (ex.StatusCode >= HttpStatusCode.BadRequest && ex.StatusCode < HttpStatusCode.InternalServerError)
                {
                    uploadCheckPoint.IsUploadAbort = true;
                }
                partResultUpload.IsFailed = true;
                partResultUpload.Exception = ex;

                if (param.eventHandler != null)
                {
                    ResumableUploadEvent e = new ResumableUploadEvent();
                    e.EventType = ResumableUploadEventTypeEnum.UploadPartFailed;
                    e.UploadId = uploadCheckPoint.UploadId;
                    e.PartNumber = uploadPart.PartNumber;
                    param.eventHandler(this, e);
                }

            }
            catch (Exception ex)
            {
                partResultUpload.IsFailed = true;
                ObsException exception = new ObsException(ex.Message, ex);
                exception.ErrorType = ErrorType.Sender;
                partResultUpload.Exception = exception;
                if (param.eventHandler != null)
                {
                    ResumableUploadEvent e = new ResumableUploadEvent();
                    e.EventType = ResumableUploadEventTypeEnum.UploadPartFailed;
                    e.UploadId = uploadCheckPoint.UploadId;
                    e.PartNumber = uploadPart.PartNumber;
                    param.eventHandler(this, e);
                }
            }
            finally
            {
                param.partResultUpload = partResultUpload;
                param.executeEvent.Set();
            }

        }

        /// <summary>
        /// Concurrently upload loads in multi-thread mode.
        /// </summary>
        private IList<PartResultUpload> ResumableUploadBegin(ResumableUploadRequest resumableUploadRequest, UploadCheckPoint uploadCheckPoint, out TransferStreamManager mgr, ResumableUploadTypeEnum uploadType)
        {

            IList<PartResultUpload> partResultsUpload = new List<PartResultUpload>();
            IList<UploadPart> uploadParts = new List<UploadPart>();
            long transferredBytes = 0;
            foreach (var uploadPart in uploadCheckPoint.UploadParts)
            {
                if (uploadPart.IsCompleted)
                {
                    transferredBytes += uploadPart.Size;
                    PartResultUpload result = new PartResultUpload();
                    result.IsFailed = false;
                    partResultsUpload.Add(result);
                }
                else
                {
                    uploadParts.Add(uploadPart);
                }
            }

            if (uploadParts.Count < 1)
            {
                mgr = null;
                return partResultsUpload;
            }

            if (resumableUploadRequest.UploadProgress != null)
            {

                if (uploadType == ResumableUploadTypeEnum.UploadFile)
                {
                    if (resumableUploadRequest.ProgressType == ProgressTypeEnum.ByBytes)
                    {
                        mgr = new ThreadSafeTransferStreamByBytes(this, resumableUploadRequest.UploadProgress,
                        uploadCheckPoint.FileStatus.Size, transferredBytes, resumableUploadRequest.ProgressInterval);
                    }
                    else
                    {
                        mgr = new ThreadSafeTransferStreamBySeconds(this, resumableUploadRequest.UploadProgress,
                        uploadCheckPoint.FileStatus.Size, transferredBytes, resumableUploadRequest.ProgressInterval);
                    }
                }

                else
                {
                    if (resumableUploadRequest.ProgressType == ProgressTypeEnum.ByBytes)
                    {

                        mgr = new TransferStreamByBytes(resumableUploadRequest.Sender, resumableUploadRequest.UploadProgress,
                        uploadCheckPoint.FileStatus.Size, transferredBytes, resumableUploadRequest.ProgressInterval);
                    }
                    else
                    {
                        mgr = new ThreadSafeTransferStreamBySeconds(resumableUploadRequest.Sender, resumableUploadRequest.UploadProgress,
                        uploadCheckPoint.FileStatus.Size, transferredBytes, resumableUploadRequest.ProgressInterval);
                    }
                }
            }
            else
            {
                mgr = null;
            }

            int taskNum = 1;
            if (uploadType == ResumableUploadTypeEnum.UploadFile)
            {
                taskNum = Math.Min((resumableUploadRequest as UploadFileRequest).TaskNum, uploadParts.Count);
            }

            ManualResetEvent[] events = new ManualResetEvent[taskNum];
            UploadPartExcuteParam[] executeParams = new UploadPartExcuteParam[taskNum];
            for (int i = 0; i < taskNum; i++)
            {
                UploadPartExcuteParam param = new UploadPartExcuteParam();
                param.uploadType = uploadType;
                param.uploadPart = uploadParts[i];
                param.uploadCheckPoint = uploadCheckPoint;
                param.executeEvent = new ManualResetEvent(false);
                param.enableCheckpoint = resumableUploadRequest.EnableCheckpoint;
                param.checkpointFile = resumableUploadRequest.CheckpointFile;
                param.eventHandler = resumableUploadRequest.UploadEventHandler;
                param.mgr = mgr;
                events[i] = param.executeEvent;
                executeParams[i] = param;
                ThreadPool.QueueUserWorkItem(UploadPartExcute, param);
            }

            try
            {

                while (taskNum < uploadParts.Count)
                {
                    if (uploadCheckPoint.IsUploadAbort)
                    {
                        break;
                    }
                    int finished = WaitHandle.WaitAny(events);
                    UploadPartExcuteParam finishedParam = executeParams[finished];
                    partResultsUpload.Add(finishedParam.partResultUpload);
                    finishedParam.partResultUpload = null;
                    finishedParam.uploadPart = uploadParts[taskNum++];
                    finishedParam.executeEvent.Reset();
                    ThreadPool.QueueUserWorkItem(UploadPartExcute, finishedParam);
                }
            }
            finally
            {
                WaitHandle.WaitAll(events);
                for (int i = 0; i < events.Length; i++)
                {
                    UploadPartExcuteParam finishedParam = executeParams[i];
                    partResultsUpload.Add(finishedParam.partResultUpload);
                    events[i].Close();
                }

            }
            return partResultsUpload;
        }

        /// <summary>
        /// Abort a multipart upload.
        /// </summary>
        private void AbortMultipartUpload(UploadCheckPoint uploadCheckPoint)
        {
            try
            {
                AbortMultipartUploadRequest abortRequest = new AbortMultipartUploadRequest
                {
                    BucketName = uploadCheckPoint.BucketName,
                    ObjectKey = uploadCheckPoint.ObjectKey,
                    UploadId = uploadCheckPoint.UploadId,
                };
                this.AbortMultipartUpload(abortRequest);
            }
            catch (ObsException ex)
            {
                LoggerMgr.Warn("Abort multipart upload failed", ex);
            }
        }
        #endregion


        #region DownloadFile
        /// <summary>
        /// Download a file in resumable mode.
        /// </summary>
        /// <param name="request">Parameters in a file download request</param>
        /// <returns>Response to a request for obtaining object metadata</returns>
        public GetObjectMetadataResponse DownloadFile(DownloadFileRequest request)
        {
            ParameterJudgement(request);
            return DownloadFileExcute(request);
        }

        /// <summary>
        /// Verify the parameters of the to-be-downloaded file.
        /// </summary>
        /// <param name="request"></param>
        private void ParameterJudgement(DownloadFileRequest request)
        {
            if (request == null)
            {
                throw new ObsException(Constants.NullRequestMessage, ErrorType.Sender, Constants.NullRequest, "");
            }
            if (string.IsNullOrEmpty(request.BucketName))
            {
                throw new ObsException(Constants.InvalidBucketNameMessage, ErrorType.Sender, Constants.InvalidBucketName, "");
            }
            if (string.IsNullOrEmpty(request.ObjectKey))
            {
                throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
            }
            if (string.IsNullOrEmpty(request.DownloadFile))
            {
                request.DownloadFile = Environment.CurrentDirectory + "/" + request.ObjectKey;
            }
            if (request.EnableCheckpoint)
            {
                if (string.IsNullOrEmpty(request.CheckpointFile))
                {
                    request.CheckpointFile = request.DownloadFile + "downloadFile_record";
                }
            }
        }

        /// <summary>
        /// Execution method of the resumable download
        /// </summary>
        private GetObjectMetadataResponse DownloadFileExcute(DownloadFileRequest downloadFileRequest)
        {
            DownloadCheckPoint downloadCheckPoint = new DownloadCheckPoint();

            GetObjectMetadataResponse response = null;


            if (downloadFileRequest.EnableCheckpoint)
            {
                bool loadFileFlag = true;
                bool paraVerifyFlag = true;
                bool fileVerifyFlag = true;
                ObsException obsException = null;
                try
                {

                    downloadCheckPoint.Load(downloadFileRequest.CheckpointFile);
                }

                catch (Exception)
                {
                    loadFileFlag = false;
                }


                if (loadFileFlag)
                {

                    if (!(downloadFileRequest.BucketName.Equals(downloadCheckPoint.BucketName)
                    && downloadFileRequest.ObjectKey.Equals(downloadCheckPoint.ObjectKey)
                    && downloadFileRequest.DownloadFile.Equals(downloadCheckPoint.DownloadFile)))
                    {
                        paraVerifyFlag = false;
                    }

                    if (string.IsNullOrEmpty(downloadFileRequest.VersionId))
                    {
                        if (!string.IsNullOrEmpty(downloadCheckPoint.VersionId))
                        {
                            paraVerifyFlag = false;
                        }
                    }
                    else if (!downloadFileRequest.VersionId.Equals(downloadCheckPoint.VersionId))
                    {
                        paraVerifyFlag = false;
                    }


                    if (paraVerifyFlag)
                    {
                        try
                        {

                            fileVerifyFlag = downloadCheckPoint.IsValid(downloadFileRequest.TempDownloadFile, this);
                        }
                        catch (ObsException ex)
                        {
                            int statusCode = Convert.ToInt32(ex.StatusCode);
                            if (statusCode >= 400 && statusCode < 500)
                            {
                                fileVerifyFlag = false;
                                obsException = ex;
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                }


                if (!loadFileFlag || !paraVerifyFlag || !fileVerifyFlag)
                {

                    if (downloadCheckPoint.TmpFileStatus != null)
                    {
                        if (File.Exists(downloadCheckPoint.TmpFileStatus.TmpFilePath))
                        {
                            File.Delete(downloadCheckPoint.TmpFileStatus.TmpFilePath);
                        }
                    }


                    if (File.Exists(downloadFileRequest.CheckpointFile))
                    {
                        File.Delete(downloadFileRequest.CheckpointFile);
                    }

                    if (obsException != null)
                    {
                        throw obsException;
                    }


                    response = DownloadFilePrepare(downloadFileRequest, downloadCheckPoint);
                }
            }
            else
            {

                response = DownloadFilePrepare(downloadFileRequest, downloadCheckPoint);
            }

            TransferStreamManager mgr = null;
            try
            {

                IList<PartResultDown> partResultsDowns = DownloadFileBegin(downloadFileRequest, downloadCheckPoint, out mgr);


                foreach (PartResultDown result in partResultsDowns)
                {

                    if (result.IsFailed && result.Exception != null)
                    {

                        if (!downloadFileRequest.EnableCheckpoint)
                        {
                            if (File.Exists(downloadCheckPoint.TmpFileStatus.TmpFilePath))
                            {
                                File.Delete(downloadCheckPoint.TmpFileStatus.TmpFilePath);
                            }
                        }

                        else if (downloadCheckPoint.IsDownloadAbort)
                        {
                            if (File.Exists(downloadCheckPoint.TmpFileStatus.TmpFilePath))
                            {
                                File.Delete(downloadCheckPoint.TmpFileStatus.TmpFilePath);
                            }
                            if (File.Exists(downloadFileRequest.CheckpointFile))
                            {
                                File.Delete(downloadFileRequest.CheckpointFile);
                            }
                        }

                        throw result.Exception;
                    }
                }
            }
            finally
            {
                mgr?.TransferEnd();
            }


            try
            {

                Rename(downloadFileRequest.TempDownloadFile, downloadFileRequest.DownloadFile);
            }
            catch (Exception ex)
            {

                if (File.Exists(downloadFileRequest.CheckpointFile))
                {
                    File.Delete(downloadFileRequest.CheckpointFile);
                }
                ObsException exception = new ObsException(ex.Message, ex);
                exception.ErrorType = ErrorType.Sender;
                throw exception;
            }


            if (downloadFileRequest.EnableCheckpoint)
            {
                if (File.Exists(downloadFileRequest.CheckpointFile))
                {
                    File.Delete(downloadFileRequest.CheckpointFile);
                }
            }


            return response == null ? this.GetObjectMetadata(downloadFileRequest, downloadCheckPoint) : response;
        }

        private GetObjectMetadataResponse GetObjectMetadata(DownloadFileRequest downloadFileRequest, DownloadCheckPoint downloadCheckPoint)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest();
            request.BucketName = downloadCheckPoint.BucketName;
            request.ObjectKey = downloadCheckPoint.ObjectKey;
            request.VersionId = downloadCheckPoint.VersionId;
            request.SseCHeader = downloadFileRequest.SseCHeader;
            return this.GetObjectMetadata(request);
        }

        /// <summary>
        /// Prepare for downloading a file.
        /// </summary>
        /// <param name="downloadFileRequest"></param>
        /// <param name="downloadCheckPoint"></param>
        private GetObjectMetadataResponse DownloadFilePrepare(DownloadFileRequest downloadFileRequest, DownloadCheckPoint downloadCheckPoint)
        {
            downloadCheckPoint.BucketName = downloadFileRequest.BucketName;
            downloadCheckPoint.ObjectKey = downloadFileRequest.ObjectKey;
            downloadCheckPoint.VersionId = downloadFileRequest.VersionId;
            downloadCheckPoint.DownloadFile = downloadFileRequest.DownloadFile;
            GetObjectMetadataResponse response = this.GetObjectMetadata(downloadFileRequest, downloadCheckPoint);
            downloadCheckPoint.ObjectStatus = new ObjectStatus()
            {
                Size = response.ContentLength,
                LastModified = response.LastModified,
                Etag = response.ETag
            };
            downloadCheckPoint.DownloadParts = SplitObject(downloadCheckPoint.ObjectStatus.Size, downloadFileRequest.DownloadPartSize);

            try
            {

                using (FileStream fs = new FileStream(downloadFileRequest.TempDownloadFile, FileMode.Create))
                {
                    fs.SetLength(downloadCheckPoint.ObjectStatus.Size);
                }
            }
            catch (Exception ex)
            {
                ObsException exception = new ObsException(ex.Message, ex);
                exception.ErrorType = ErrorType.Sender;
                throw exception;
            }


            downloadCheckPoint.TmpFileStatus = new TmpFileStatus()
            {
                TmpFilePath = downloadFileRequest.TempDownloadFile,
                Size = downloadCheckPoint.ObjectStatus.Size,
                LastModified = File.GetLastWriteTime(downloadFileRequest.TempDownloadFile),
            };


            if (downloadFileRequest.EnableCheckpoint)
            {
                try
                {
                    downloadCheckPoint.Record(downloadFileRequest.CheckpointFile);
                }

                catch (Exception ex)
                {

                    if (downloadCheckPoint.TmpFileStatus != null)
                    {
                        if (File.Exists(downloadCheckPoint.TmpFileStatus.TmpFilePath))
                        {
                            File.Delete(downloadCheckPoint.TmpFileStatus.TmpFilePath);
                        }
                    }

                    ObsException exception = new ObsException(ex.Message, ex);
                    exception.ErrorType = ErrorType.Sender;
                    throw exception;
                }
            }
            return response;
        }

        /// <summary>
        /// Calculate the part list based on the file size and part size.
        /// </summary>
        private List<DownloadPart> SplitObject(long objectSize, long partSize)
        {
            List<DownloadPart> parts = new List<DownloadPart>();

            int partNumber = Convert.ToInt32(objectSize / partSize);

            if (partNumber >= 10000)
            {
                partSize = objectSize % 10000 == 0 ? objectSize / 10000 : objectSize / 10000 + 1;
                partNumber = Convert.ToInt32(objectSize / partSize);
            }

            if (objectSize % partSize > 0)
                partNumber++;

            for (int i = 0; i < partNumber; i++)
            {
                parts.Add(new DownloadPart()
                {
                    PartNumber = (i + 1),
                    Start = i * partSize,
                    End = (i + 1) * partSize - 1,
                    IsCompleted = false
                });
            }
            if (objectSize % partSize > 0)
                parts[parts.Count - 1].End = objectSize - 1;

            return parts;
        }

        /// <summary>
        /// Result of the part download
        /// </summary>
        internal class PartResultDown
        {
            /// <summary>
            /// Whether a part fails to be downloaded
            /// </summary>
            public bool IsFailed { get; set; }

            /// <summary>
            /// An part download exception
            /// </summary>
            public ObsException Exception { get; set; }

        }

        internal class DownloadPartExcuteParam
        {
            internal DownloadPart downloadPart;
            internal DownloadCheckPoint downloadCheckPoint;
            internal PartResultDown partResultDown;
            internal ManualResetEvent executeEvent;
            internal DownloadFileRequest downloadFileRequest;
            internal EventHandler<ResumableDownloadEvent> eventHandler;
            internal TransferStreamManager mgr;
        }

        /// <summary>
        /// Perform a partial download.
        /// </summary>
        private void DownloadPartExcute(object state)
        {
            DownloadPartExcuteParam param = state as DownloadPartExcuteParam;
            DownloadCheckPoint downloadCheckPoint = param.downloadCheckPoint;
            DownloadPart downloadPart = param.downloadPart;
            DownloadFileRequest downloadFileRequest = param.downloadFileRequest;

            PartResultDown partResultDown = new PartResultDown();

            try
            {
                if (!downloadCheckPoint.IsDownloadAbort)
                {

                    GetObjectRequest getObjectRequest = new GetObjectRequest()
                    {
                        BucketName = downloadCheckPoint.BucketName,
                        ObjectKey = downloadCheckPoint.ObjectKey,
                        ByteRange = new ByteRange(downloadPart.Start, downloadPart.End),
                        SseCHeader = downloadFileRequest.SseCHeader,
                        VersionId = downloadCheckPoint.VersionId
                    };
                    getObjectRequest.IfMatch = downloadFileRequest.IfMatch;
                    getObjectRequest.IfNoneMatch = downloadFileRequest.IfNoneMatch;
                    getObjectRequest.IfModifiedSince = downloadFileRequest.IfModifiedSince;
                    getObjectRequest.IfUnmodifiedSince = downloadFileRequest.IfUnmodifiedSince;


                    GetObjectResponse getObjectResponse = this.GetObject(getObjectRequest);

                    if (getObjectResponse.OutputStream == null || getObjectResponse.ContentLength == 0)
                    {
                        throw new ObsException("response body is null");
                    }

                    if (getObjectResponse.OutputStream != null && getObjectResponse.ContentLength > 0)
                    {
                        Stream content = null;
                        try
                        {
                            if (param.mgr != null)
                            {
                                TransferStream stream = new TransferStream(getObjectResponse.OutputStream);
                                stream.BytesReaded += param.mgr.BytesTransfered;
                                stream.StartRead += param.mgr.TransferStart;
                                stream.BytesReset += param.mgr.TransferReset;
                                content = stream;
                            }
                            else
                            {
                                content = getObjectResponse.OutputStream;
                            }

                            if (getObjectResponse.ContentLength != downloadPart.End - downloadPart.Start + 1)
                            {
                                throw new ObsException("The length of the response returned is not the same as expected.");
                            }

                            using (FileStream output = new FileStream(downloadFileRequest.TempDownloadFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite,
                                Constants.DefaultBufferSize))
                            {
                                output.Seek(downloadPart.Start, SeekOrigin.Begin);

                                byte[] buffer = new byte[Constants.DefaultBufferSize];

                                int bytesRead = 0;

                                while ((bytesRead = content.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    output.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                        finally
                        {
                            if (content != null)
                            {
                                content.Close();
                            }
                        }
                    }

                    LoggerMgr.Debug($"No {downloadPart.PartNumber} part ContentLength is {getObjectResponse.ContentLength} and Part size is ：{downloadPart.End - downloadPart.Start}");
                    partResultDown.IsFailed = false;
                    downloadPart.IsCompleted = true;

                    if (downloadFileRequest.EnableCheckpoint)
                    {
                        lock (downloadCheckPoint.downloadlock)
                        {

                            downloadCheckPoint.UpdateTmpFile(downloadFileRequest.TempDownloadFile);

                            downloadCheckPoint.Record(downloadFileRequest.CheckpointFile);
                        }
                    }

                    if (param.eventHandler != null)
                    {
                        ResumableDownloadEvent e = new ResumableDownloadEvent();
                        e.EventType = ResumableDownloadEventTypeEnum.DownloadPartSucceed;
                        e.PartNumber = downloadPart.PartNumber;
                        param.eventHandler(this, e);
                    }
                }
                else
                {
                    partResultDown.IsFailed = false;
                }
            }
            catch (ObsException ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(string.Format("DownloadPartExcute exception code: {0}, with message: {1}", ex.ErrorCode, ex.Message), ex);
                }

                if (ex.StatusCode >= HttpStatusCode.BadRequest && ex.StatusCode < HttpStatusCode.InternalServerError)
                {
                    downloadCheckPoint.IsDownloadAbort = true;
                }
                partResultDown.IsFailed = true;
                partResultDown.Exception = ex;
                if (param.eventHandler != null)
                {
                    ResumableDownloadEvent e = new ResumableDownloadEvent();
                    e.EventType = ResumableDownloadEventTypeEnum.DownloadPartFailed;
                    e.PartNumber = downloadPart.PartNumber;
                    param.eventHandler(this, e);
                }
            }
            catch (Exception ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error("Error in DownloadPartExcute", ex);
                }

                partResultDown.IsFailed = true;
                ObsException exception = new ObsException(ex.Message, ex);
                exception.ErrorType = ErrorType.Sender;
                partResultDown.Exception = exception;
                if (param.eventHandler != null)
                {
                    ResumableDownloadEvent e = new ResumableDownloadEvent();
                    e.EventType = ResumableDownloadEventTypeEnum.DownloadPartFailed;
                    e.PartNumber = downloadPart.PartNumber;
                    param.eventHandler(this, e);
                }
            }
            finally
            {
                if (LoggerMgr.IsDebugEnabled)
                {
                    LoggerMgr.Debug($"No {downloadPart.PartNumber} part finally download {(partResultDown.IsFailed ? "Failed" : "Succeed")}, Start at {downloadPart.Start}, End at {downloadPart.End}");
                }

                param.partResultDown = partResultDown;
                param.executeEvent.Set();
            }
        }


        /// <summary>
        /// Concurrently download parts in multi-thread mode. 
        /// </summary>
        private IList<PartResultDown> DownloadFileBegin(DownloadFileRequest downloadFileRequest, DownloadCheckPoint downloadCheckPoint,
            out TransferStreamManager mgr)
        {

            IList<PartResultDown> partResultsDowns = new List<PartResultDown>();
            IList<DownloadPart> downloadParts = new List<DownloadPart>();
            long transferredBytes = 0;
            foreach (var partResultDown in downloadCheckPoint.DownloadParts)
            {
                if (partResultDown.IsCompleted)
                {
                    PartResultDown result = new PartResultDown();
                    result.IsFailed = false;
                    partResultsDowns.Add(result);
                    transferredBytes += (partResultDown.End - partResultDown.Start) + 1;
                }
                else
                {
                    downloadParts.Add(partResultDown);
                }
            }

            if (downloadParts.Count < 1)
            {
                mgr = null;
                return partResultsDowns;
            }

            if (downloadFileRequest.DownloadProgress != null)
            {
                if (downloadFileRequest.ProgressType == ProgressTypeEnum.ByBytes)
                {
                    mgr = new ThreadSafeTransferStreamByBytes(this, downloadFileRequest.DownloadProgress,
                   downloadCheckPoint.ObjectStatus.Size, transferredBytes, downloadFileRequest.ProgressInterval);
                }
                else
                {
                    mgr = new ThreadSafeTransferStreamBySeconds(this, downloadFileRequest.DownloadProgress,
                    downloadCheckPoint.ObjectStatus.Size, transferredBytes, downloadFileRequest.ProgressInterval);
                }
            }
            else
            {
                mgr = null;
            }

            int taskNum = Math.Min(downloadFileRequest.TaskNum, downloadParts.Count);
            ManualResetEvent[] events = new ManualResetEvent[taskNum];
            DownloadPartExcuteParam[] executeParams = new DownloadPartExcuteParam[taskNum];
            for (int i = 0; i < taskNum; i++)
            {
                DownloadPartExcuteParam param = new DownloadPartExcuteParam();
                param.downloadPart = downloadParts[i];
                param.downloadCheckPoint = downloadCheckPoint;
                param.executeEvent = new ManualResetEvent(false);
                param.downloadFileRequest = downloadFileRequest;
                param.eventHandler = downloadFileRequest.DownloadEventHandler;
                param.mgr = mgr;
                events[i] = param.executeEvent;
                executeParams[i] = param;
                ThreadPool.QueueUserWorkItem(DownloadPartExcute, param);
            }

            try
            {

                while (taskNum < downloadParts.Count)
                {
                    if (downloadCheckPoint.IsDownloadAbort)
                    {
                        break;
                    }
                    int finished = WaitHandle.WaitAny(events);
                    DownloadPartExcuteParam finishedParam = executeParams[finished];
                    partResultsDowns.Add(finishedParam.partResultDown);
                    finishedParam.partResultDown = null;
                    finishedParam.downloadPart = downloadParts[taskNum++];
                    finishedParam.executeEvent.Reset();
                    ThreadPool.QueueUserWorkItem(DownloadPartExcute, finishedParam);
                }
            }
            finally
            {
                WaitHandle.WaitAll(events);
                for (int i = 0; i < events.Length; i++)
                {
                    DownloadPartExcuteParam finishedParam = executeParams[i];
                    partResultsDowns.Add(finishedParam.partResultDown);
                    events[i].Close();
                }

            }

            return partResultsDowns;
        }


        /// <summary>
        /// Rename the temporary file. 
        /// </summary>
        /// <param name="tempDownloadFilePath"></param>
        /// <param name="downloadFilePath"></param>
        private void Rename(string tempDownloadFilePath, string downloadFilePath)
        {
            if (File.Exists(downloadFilePath))
            {
                File.Delete(downloadFilePath);
            }
            if (!File.Exists(tempDownloadFilePath))
            {
                throw new FileNotFoundException("tempDownloadFile '" + tempDownloadFilePath + "' does not exist");
            }
            try
            {
                File.Move(tempDownloadFilePath, downloadFilePath);
            }

            catch (Exception)
            {
                byte[] buffer = new byte[Constants.DefaultBufferSize];
                int bytesRead = 0;

                try
                {
                    using (FileStream tempDownloadStream = new FileStream(tempDownloadFilePath, FileMode.Open))
                    {
                        while ((bytesRead = tempDownloadStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            using (FileStream downloadStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, Constants.DefaultBufferSize))
                            {
                                downloadStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ObsException exception = new ObsException(ex.Message, ex);
                    exception.ErrorType = ErrorType.Sender;
                    throw exception;
                }
                finally
                {

                    File.Delete(tempDownloadFilePath);
                }
            }
        }
        #endregion

    }
}
