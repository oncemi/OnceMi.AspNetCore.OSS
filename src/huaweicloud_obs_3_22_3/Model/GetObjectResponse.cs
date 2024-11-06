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
using System;
using System.IO;



namespace OBS.Model
{
    /// <summary>
    /// Response to an object download request
    /// </summary>
    public class GetObjectResponse : GetObjectMetadataResponse
    {

        private bool _disposed = false;
        private Stream _outputStream;

        internal override void HandleObsWebServiceRequest(ObsWebServiceRequest req)
        {
            GetObjectRequest request = req as GetObjectRequest;

            if (request != null && request.DownloadProgress != null && this.OutputStream != null && this.ContentLength > 0)
            {
                TransferStream stream = new TransferStream(this.OutputStream);

                TransferStreamManager mgr;
                if (request.ProgressType == ProgressTypeEnum.ByBytes)
                {
                    mgr = new TransferStreamByBytes(request.Sender, request.DownloadProgress,
                    this.ContentLength, 0, request.ProgressInterval);
                }
                else
                {
                    mgr = new ThreadSafeTransferStreamBySeconds(request.Sender, request.DownloadProgress,
                    this.ContentLength, 0, request.ProgressInterval);
                    stream.CloseStream += mgr.TransferEnd;
                }
                stream.BytesReaded += mgr.BytesTransfered;
                stream.StartRead += mgr.TransferStart;
                stream.BytesReset += mgr.TransferReset;
                this.OutputStream = stream;
            }

        }

        /// <summary>
        /// Object data stream 
        /// </summary>
        public Stream OutputStream
        {
            get
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return this._outputStream;
            }
            internal set
            {
                this._outputStream = value;
            }
        }

        /// <summary>
        /// Write the object content to a file.
        /// </summary>
        /// <param name="filePath">File path</param>
        public void WriteResponseStreamToFile(string filePath)
        {
            this.WriteResponseStreamToFile(filePath, false);
        }

        /// <summary>
        /// Write the object content to a file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="append">Write mode</param>
        public void WriteResponseStreamToFile(string filePath, bool append)
        {
            try
            {
                filePath = System.IO.Path.GetFullPath(filePath);
                FileInfo fi = new FileInfo(filePath);
                Directory.CreateDirectory(fi.DirectoryName);

                FileMode fm = FileMode.Create;
                if (append && File.Exists(filePath))
                {
                    fm = FileMode.Append;
                }
                using (Stream downloadStream = new FileStream(filePath, fm, FileAccess.Write, FileShare.Read, Constants.DefaultBufferSize))
                {
                    long current = 0;
                    byte[] buffer = new byte[Constants.DefaultBufferSize];
                    int bytesRead = 0;
                    while ((bytesRead = this.OutputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        downloadStream.Write(buffer, 0, bytesRead);
                        current += bytesRead;

                    }
                    if (current != this.ContentLength)
                    {
                        throw new ObsException(string.Format("The total bytes read {0} from response stream is not equal to the Content-Length {1}", current, this.ContentLength), ErrorType.Receiver, null);
                    }
                }
            }
            catch (ObsException ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(ex.Message, ex);
                }
                throw ex;
            }
            catch (Exception ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(ex.Message, ex);
                }
                ObsException exception = new ObsException(ex.Message, ex);
                exception.ErrorType = ErrorType.Receiver;
                throw exception;
            }
            finally
            {
                if (OutputStream != null)
                {
                    OutputStream.Close();
                    OutputStream = null;
                }
            }
        }
    }
}

