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

namespace OBS
{
    /// <summary>
    /// Base class of OBS exceptions
    /// </summary>
    public abstract class ServiceException : Exception
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public ServiceException()
            : base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        public ServiceException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="innerException">Exception caused by the error</param>
        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="innerException">Exception caused by the error</param>
        /// <param name="statusCode">HTTP status code</param>
        public ServiceException(string message, Exception innerException, HttpStatusCode statusCode)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerException">Exception caused by the error</param>
        public ServiceException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        public ServiceException(string message, ErrorType errorType, string errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.ErrorType = errorType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        public ServiceException(string message, ErrorType errorType, string errorCode, string requestId)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.ErrorType = errorType;
            this.RequestId = requestId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ServiceException(string message, ErrorType errorType, string errorCode, string requestId, HttpStatusCode statusCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.ErrorType = errorType;
            this.RequestId = requestId;
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error information</param>
        /// <param name="innerException">Exception caused by the error</param>
        /// <param name="errorType">Error type</param>
        /// <param name="errorCode">Error code on the OBS server</param>
        /// <param name="requestId">Request ID returned by the OBS server</param>
        /// <param name="statusCode">HTTP status code</param>
        public ServiceException(string message, Exception innerException, ErrorType errorType, string errorCode, string requestId, HttpStatusCode statusCode)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.ErrorType = errorType;
            this.RequestId = requestId;
            this.StatusCode = statusCode;
        }


        /// <summary>
        /// Error description returned by the OBS server
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Error type
        /// </summary>
        public ErrorType ErrorType
        {
            get;
            set;
        }

        /// <summary>
        /// Error code returned by the OBS server
        /// </summary>
        public string ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Request ID returned by the OBS server
        /// </summary>
        public string RequestId
        {
            get;
            set;
        }

        /// <summary>
        /// HTTP status code in the response
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }
    }
}


