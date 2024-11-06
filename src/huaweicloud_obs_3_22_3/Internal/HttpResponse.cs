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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace OBS.Internal
{
    public class HttpResponse : IDisposable
    {

        private IDictionary<string, string> _headers;

        private HttpWebResponse _response;
        private HttpWebRequest _request;

        private bool _disposed;

        public HttpResponse(HttpWebResponse httpWebResponse)
        {
            this._response = httpWebResponse;
        }

        public HttpResponse(WebException failure, HttpWebRequest httpWebRequest)
        {
            HttpWebResponse httpWebResponse = failure.Response as HttpWebResponse;
            this.Failure = failure;
            this._response = httpWebResponse;
            this._request = httpWebRequest;
        }

        public HttpWebResponse HttpWebResponse
        {
            get
            {
                return this._response;
            }
        }

        public void Abort()
        {
            if (this._request != null)
            {
                this._request.Abort();
            }
        }

        public IDictionary<string, string> Headers
        {
            get { return _headers ?? (_headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)); }
            internal set
            {
                this._headers = value;
            }
        }

        public Stream Content
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return (this._response != null) ? this._response.GetResponseStream() : null;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this._response.StatusCode;
            }
        }

        public Exception Failure { get; set; }

        internal string RequestUrl
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
                if (_response != null)
                {
                    _response.Close();
                    _response = null;
                }
                if (_request != null)
                {
                    _request = null;
                }
                _disposed = true;
            }
        }
    }
}
