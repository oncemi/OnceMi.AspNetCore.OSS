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
using OBS.Internal.Log;
using OBS.Model;
using System;
using System.IO;
using System.Net;

namespace OBS.Internal
{
    internal partial class HttpClient
    {
        internal HttpObsAsyncResult BeginPerformRequest(HttpRequest request, HttpContext context, 
            AsyncCallback callback, object state)
        {
            this.PrepareRequestAndContext(request, context);
            HttpObsAsyncResult result = new HttpObsAsyncResult(callback, state);
            result.HttpRequest = request;
            result.HttpContext = context;
            result.OriginPos = (request.Content != null && request.Content.CanSeek) ? request.Content.Position : -1L;
            result.RetryCount = 0;
            result.RequestStartDateTime = DateTime.Now;
            this.BeginDoRequest(result);
            return result;
        }

        internal HttpResponse EndPerformRequest(HttpObsAsyncResult result)
        {
            HttpResponse response = null;
            HttpRequest request = result.HttpRequest;
            HttpContext context = result.HttpContext;
            int maxErrorRetry = context.ObsConfig.MaxErrorRetry;
            long originPos = result.OriginPos;
            try
            {
                response = context.ObsConfig.AsyncSocketTimeout < 0 ? result.Get() : result.Get(context.ObsConfig.AsyncSocketTimeout);
                new MergeResponseHeaderHandler(this.GetIHeaders(context));
                int statusCode = Convert.ToInt32(response.StatusCode);
                new MergeResponseHeaderHandler(this.GetIHeaders(context)).Handle(response);

                if (LoggerMgr.IsDebugEnabled)
                {
                    LoggerMgr.Debug(string.Format("Response with statusCode {0} and headers {1}", statusCode, CommonUtil.ConvertHeadersToString(response.Headers)));
                }

                if (statusCode >= 300 && statusCode < 400 && statusCode != 304)
                {
                    if (response.Headers.ContainsKey(Constants.CommonHeaders.Location))
                    {
                        string location = response.Headers[Constants.CommonHeaders.Location];
                        if (!string.IsNullOrEmpty(location))
                        {
                            if (location.IndexOf("?") < 0)
                            {
                                location += "?" + CommonUtil.ConvertParamsToString(request.Params);
                            }
                            if (LoggerMgr.IsWarnEnabled)
                            {
                                LoggerMgr.Warn(string.Format("Redirect to {0}", location));
                            }
                            context.RedirectLocation = location;
                            result.RetryCount--;
                            if (ShouldRetry(request, null, result.RetryCount, maxErrorRetry))
                            {
                                PrepareRetry(request, response, result.RetryCount, originPos, true);
                                result.Reset();
                                this.BeginDoRequest(result);
                                return this.EndPerformRequest(result);
                            }
                            else if (result.RetryCount > maxErrorRetry)
                            {
                                throw ParseObsException(response, "Exceeded 3xx redirect limit", context);
                            }
                        }
                    }
                    throw ParseObsException(response, "Try to redirect, but location is null!", context);
                }
                else if ((statusCode >= 400 && statusCode < 500) || statusCode == 304)
                {
                    ObsException exception = ParseObsException(response, "Request error", context);
                    if (Constants.RequestTimeout.Equals(exception.ErrorCode))
                    {
                        if (ShouldRetry(request, null, result.RetryCount, maxErrorRetry))
                        {
                            if (LoggerMgr.IsWarnEnabled)
                            {
                                LoggerMgr.Warn("Retrying connection that failed with RequestTimeout error");
                            }
                            PrepareRetry(request, response, result.RetryCount, originPos, true);
                            result.Reset();
                            this.BeginDoRequest(result);
                            return this.EndPerformRequest(result);
                        }
                        else if (result.RetryCount > maxErrorRetry && LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Exceeded maximum number of retries for RequestTimeout errors");
                        }
                    }
                    throw exception;
                }
                else if (statusCode >= 500)
                {
                    if (ShouldRetry(request, null, result.RetryCount, maxErrorRetry))
                    {
                        PrepareRetry(request, response, result.RetryCount, originPos, true);
                        result.Reset();
                        this.BeginDoRequest(result);
                        return this.EndPerformRequest(result);
                    }
                    else if (result.RetryCount > maxErrorRetry && LoggerMgr.IsErrorEnabled)
                    {
                        LoggerMgr.Error("Encountered too many 5xx errors");
                    }
                    throw ParseObsException(response, "Request error", context);
                }

                foreach (HttpResponseHandler handler in context.Handlers)
                {
                    handler.Handle(response);
                }
                return response;
            }
            catch (Exception ex)
            {
                try
                {
                    if (ex is ObsException)
                    {
                        if (LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Rethrowing as a ObsException error in EndPerformRequest", ex);
                        }
                        throw ex;
                    }
                    else
                    {
                        if (ShouldRetry(request, ex, result.RetryCount, maxErrorRetry))
                        {
                            PrepareRetry(request, response, result.RetryCount, originPos, true);
                            result.Reset();
                            this.BeginDoRequest(result);
                            return this.EndPerformRequest(result);
                        }
                        else if (result.RetryCount > maxErrorRetry && LoggerMgr.IsWarnEnabled)
                        {
                            LoggerMgr.Warn("Too many errors excced the max error retry count", ex);
                        }
                        if (LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Rethrowing as a ObsException error in PerformRequest", ex);
                        }
                        throw ParseObsException(response, ex.Message, ex, result.HttpContext);
                    }
                }
                finally
                {
                    CommonUtil.CloseIDisposable(response);
                }
            }
        }

        private void BeginDoRequest(HttpObsAsyncResult asyncResult)
        {
            HttpRequest httpRequest = asyncResult.HttpRequest;
            HttpContext context = asyncResult.HttpContext;
            if (!context.SkipAuth)
            {
                this.GetSigner(context).DoAuth(httpRequest, context, this.GetIHeaders(context));
            }

            if (!context.ObsConfig.KeepAlive && !httpRequest.Headers.ContainsKey(Constants.CommonHeaders.Connection))
            {
                httpRequest.Headers.Add(Constants.CommonHeaders.Connection, "Close");
            }

            HttpWebRequest request = HttpWebRequestFactory.BuildWebRequest(httpRequest, context);
            asyncResult.HttpWebRequest = request;
            asyncResult.HttpStartDateTime = DateTime.Now;


            if (httpRequest.Method == HttpVerb.PUT ||
                httpRequest.Method == HttpVerb.POST || httpRequest.Method == HttpVerb.DELETE)
            {
                this.BeginSetContent(asyncResult);
            }
            else
            {
                asyncResult.Continue(this.EndGetResponse);
            }
        }

        private void BeginSetContent(HttpObsAsyncResult asyncResult)
        {
            HttpWebRequest webRequest = asyncResult.HttpWebRequest;
            HttpRequest httpRequest = asyncResult.HttpRequest;

            long userSetContentLength = -1;
            if (httpRequest.Headers.ContainsKey(Constants.CommonHeaders.ContentLength))
            {
                userSetContentLength = long.Parse(httpRequest.Headers[Constants.CommonHeaders.ContentLength]);
            }

            if (userSetContentLength >= 0)
            {
                webRequest.ContentLength = userSetContentLength;
                if (webRequest.ContentLength > Constants.DefaultStreamBufferThreshold)
                {
                    webRequest.AllowWriteStreamBuffering = false;
                }
            }
            else
            {
                webRequest.SendChunked = true;
                webRequest.AllowWriteStreamBuffering = false;
            }

            webRequest.BeginGetRequestStream(this.EndGetRequestStream, asyncResult);
        }

        private void EndGetRequestStream(IAsyncResult ar)
        {
            HttpObsAsyncResult asyncResult = ar.AsyncState as HttpObsAsyncResult;
            HttpWebRequest webRequest = asyncResult.HttpWebRequest;
            ObsConfig obsConfig = asyncResult.HttpContext.ObsConfig;
            Stream data = asyncResult.HttpRequest.Content;
            if (data == null)
            {
                data = new MemoryStream();
            }
            try
            {
                using (Stream requestStream = webRequest.EndGetRequestStream(ar))
                {
                    ObsCallback callback = delegate ()
                    {
                        asyncResult.IsTimeout = false;
                    };
                    if (!webRequest.SendChunked)
                    {
                        CommonUtil.WriteTo(data, requestStream, webRequest.ContentLength, obsConfig.BufferSize, callback);
                    }
                    else
                    {
                        CommonUtil.WriteTo(data, requestStream, obsConfig.BufferSize, callback);
                    }
                }
                asyncResult.Continue(this.EndGetResponse);
            }
            catch (Exception e)
            {
                asyncResult.Abort(e);
            }
        }

        private void EndGetResponse(IAsyncResult ar)
        {
            HttpObsAsyncResult asyncResult = ar.AsyncState as HttpObsAsyncResult;
            asyncResult.IsTimeout = false;
            try
            {
                HttpResponse httpResponse = new HttpResponse(asyncResult.HttpWebRequest.EndGetResponse(ar) as HttpWebResponse);
                asyncResult.Set(httpResponse);
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response == null)
                {
                    asyncResult.Abort(ex);
                }
                else
                {
                    asyncResult.Set(new HttpResponse(ex, asyncResult.HttpWebRequest));
                }
            }
            catch (Exception ex)
            {
                asyncResult.Abort(ex);
            }
            finally
            {
                if (LoggerMgr.IsInfoEnabled)
                {
                    LoggerMgr.Info(string.Format("Send http request end, cost {0} ms", (DateTime.Now.Ticks - asyncResult.HttpStartDateTime.Ticks) / 10000));
                }
            }
        }


    }

    internal class HttpObsAsyncResult : ObsAsyncResult<HttpResponse>
    {
        public HttpObsAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
            this.IsTimeout = true;
        }

        public object AdditionalState { get; set; }

        public HttpRequest HttpRequest { get; set; }

        public HttpContext HttpContext { get; set; }

        public HttpWebRequest HttpWebRequest { get; set; }

        public long OriginPos { get; set; }

        public int RetryCount { get; set; }

        public bool IsTimeout { get; set; }

        public DateTime HttpStartDateTime { get; set; }

        public DateTime RequestStartDateTime { get; set; }

        public void Reset()
        {
            this.Reset(null);
            this.RetryCount++;
        }

        public override void Reset(AsyncCallback callback)
        {
            base.Reset(callback);
            this.HttpWebRequest = null;
        }

        public override HttpResponse Get(int millisecondsTimeout)
        {
            if (!this._isCompleted)
            {
                while (!this._event.WaitOne(millisecondsTimeout))
                {
                    if (IsTimeout)
                    {
                        throw new TimeoutException("Socket timeout");
                    }
                    IsTimeout = true;
                }
            }

            if (this._exception != null)
            {
                throw this._exception;
            }
            return this._result;
        }

        public void Abort()
        {
            if (this.HttpWebRequest != null)
            {
                try
                {
                    this.HttpWebRequest.Abort();
                }
                catch (Exception ex)
                {
                    LoggerMgr.Error(ex.Message, ex);
                }
            }
        }

        public void Abort(Exception ex)
        {
            this.Abort();
            if(ex != null)
            {
                this.Set(ex);
            }
        }

        public void Continue(AsyncCallback callback)
        {
            try
            {
                this.HttpWebRequest.BeginGetResponse(callback, this);
            }
            catch (Exception ex)
            {
                this.Abort(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                base.Dispose(disposing);
                this.AdditionalState = null;
                CommonUtil.CloseIDisposable(this.HttpRequest);
            }
        }
    }


}
