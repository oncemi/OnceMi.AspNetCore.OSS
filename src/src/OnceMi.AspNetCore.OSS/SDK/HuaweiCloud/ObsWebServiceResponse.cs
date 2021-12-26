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
using System.Collections.Generic;
using System.Net;

namespace OBS
{
    /// <summary>
    /// Base class of service responses 
    /// </summary>
    public class ObsWebServiceResponse : IDisposable
    {
        private bool _disposed = false;

        private IDictionary<string, string> _headers;
        
        internal virtual void HandleObsWebServiceRequest(ObsWebServiceRequest request)
        {

        }

        /// <summary>
        /// Request ID returned by the OBS server, which uniquely identities a request
        /// </summary>
        public string RequestId
        {
            get;
            internal set;
        }

        /// <summary>
        ///  Response headers
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get
            {
                return this._headers ?? (this._headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
            internal set
            {
                this._headers = value;
            }
        }

        /// <summary>
        /// Content length of the HTTP response
        /// </summary>
        public virtual long ContentLength
        {
            get;
            internal set;
        }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get;
            internal set;
        }

        public HttpResponse OriginalResponse
        {
            get;
            set;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (OriginalResponse.Content != null && ContentLength > 0)
                {
                    CommonUtil.CloseIDisposable(OriginalResponse);
                }
                _disposed = true;
            }
        }
    }
}


