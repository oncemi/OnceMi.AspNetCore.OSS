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
using System.Net;
using OBS.Model;
using System.Text;

namespace OBS
{
    /// <summary>
    /// The OBS service is abnormal.
    /// </summary>
    public class ObsException : ServiceException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        public ObsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="innerException">Exception caused by the error</param>
        public ObsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerException">Exception caused by the error</param>
        public ObsException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        public ObsException(string message, ErrorType errorType, string errorCode)
            : base(message, errorType, errorCode)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        public ObsException(string message, ErrorType errorType, string errorCode, string requestId)
            : base(message, errorType, errorCode, requestId)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ObsException(string message, ErrorType errorType, string errorCode, string requestId, HttpStatusCode statusCode)
            : base(message, errorType, errorCode, requestId, statusCode)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="innerException">Exception caused by the error</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ObsException(string message, Exception innerException, ErrorType errorType, string errorCode, string requestId, HttpStatusCode statusCode)
            : base(message, innerException, errorType, errorCode, requestId, statusCode)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error description returned by the OBS server</param>
        /// <param name="innerException">Exception caused by the error</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="obsId2"> Special symbol</param>
        public ObsException(string message, Exception innerException, ErrorType errorType, string errorCode, string requestId, HttpStatusCode statusCode, string obsId2)
            : base(message, innerException, errorType, errorCode, requestId, statusCode)
        {
            this.ObsId2 = obsId2;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="errorMessage">Error description returned by the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ObsException(string message, ErrorType errorType, string errorCode, string errorMessage, string requestId, HttpStatusCode statusCode)
            : base(message, errorType, errorCode, requestId, statusCode)
        {
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="innerException">Exception caused by the error</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="errorMessage">Error description returned by the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ObsException(string message, Exception innerException, ErrorType errorType, string errorCode, string errorMessage, string requestId, HttpStatusCode statusCode)
            : base(message, innerException, errorType, errorCode, requestId, statusCode)
        {
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Special symbol returned by the OBS server, used for locating faults
        /// </summary>
        [Obsolete]
        public string ObsId2 { get; set; }

        /// <summary>
        /// Server ID
        /// </summary>
        public string HostId
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Message)
            .Append(", StatusCode:").Append(Convert.ToInt32(this.StatusCode))
                .Append(", ErrorCode:").Append(this.ErrorCode)
                .Append(", ErrorMessage:").Append(this.ErrorMessage)
                .Append(", RequestId:").Append(this.RequestId)
                .Append(", HostId:").Append(this.HostId);
            return sb.ToString();
        }
    }
}


