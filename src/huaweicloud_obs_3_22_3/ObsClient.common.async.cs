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
using System.IO;
using System.Net;
using System.Threading;
using OBS.Internal;
using OBS.Internal.Log;
using OBS.Internal.Negotiation;

namespace OBS
{

    public partial class ObsClient
    {

        internal GetApiVersionResponse GetApiVersionAsync(GetApiVersionRequest request)
        {
            return this.EndDoRequest<GetApiVersionRequest, GetApiVersionResponse>(this.BeginDoRequest<GetApiVersionRequest>(request, null, null));
        }

        internal IAsyncResult BeginDoRequest<T>(T request, AsyncCallback callback, object state) where T : ObsWebServiceRequest
        {
            return this.BeginDoRequest(request, null, callback, state);
        }

        internal IAsyncResult BeginDoRequest<T>(T request, DoValidateDelegate doValidateDelegate,
             AsyncCallback callback, object state) where T : ObsWebServiceRequest
        {
            HttpContext context = this.BeforeRequest(request, doValidateDelegate, true);
            try
            {
                HttpObsAsyncResult result = this.httpClient.BeginPerformRequest(this.PrepareHttpRequest(request, context), context, callback, state);
                
                result.AdditionalState = new object[] { request, context };
                return result;
            }
            catch (ObsException ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(string.Format("{0} exception code: {1}, with message: {2}", request.GetAction(), ex.ErrorCode, ex.Message));

                }
                throw ex;
            }
            catch (Exception ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(string.Format("{0} exception with message: {1}", request.GetAction(), ex.Message));
                }
                throw new ObsException(ex.Message, ex);
            }
        }

        internal K EndDoRequest<T, K>(IAsyncResult ar)
             where T : ObsWebServiceRequest
            where K : ObsWebServiceResponse
        {
            return this.EndDoRequest<T, K>(ar, true);
        }
        internal K EndDoRequest<T, K>(IAsyncResult ar, bool autoClose)
            where T : ObsWebServiceRequest
            where K : ObsWebServiceResponse
        {
            if (ar == null)
            {
                throw new ObsException(Constants.NullRequestMessage, ErrorType.Sender, Constants.NullRequest, "");
            }
            HttpObsAsyncResult result = ar as HttpObsAsyncResult;
            
            if(result == null)
            {
                throw new ObsException(Constants.NullRequestMessage, ErrorType.Sender, Constants.NullRequest, "");
            }
            object[] additionalState = result.AdditionalState as object[];

            T request = additionalState[0] as T;
            HttpContext context = additionalState[1] as HttpContext;
            
            try
            {
                
                HttpResponse httpResponse = this.httpClient.EndPerformRequest(result);
                return this.PrepareResponse<T, K>(request, context, result.HttpRequest, httpResponse);
            }
            catch (ObsException ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(string.Format("{0} exception code: {1}, with message: {2}", request.GetAction(), ex.ErrorCode, ex.Message));

                }
                throw ex;
            }
            catch (Exception ex)
            {
                if (LoggerMgr.IsErrorEnabled)
                {
                    LoggerMgr.Error(string.Format("{0} exception with message: {1}", request.GetAction(), ex.Message));
                }
                throw new ObsException(ex.Message, ex);
            }
            finally
            {
                if (autoClose)
                {
                    if (request != null)
                    {
                        request.Sender = null;
                    }

                    CommonUtil.CloseIDisposable(result.HttpRequest);
                }

                if (LoggerMgr.IsInfoEnabled)
                {

                    LoggerMgr.Info(string.Format("{0} end, cost {1} ms", request.GetAction(), (DateTime.Now - result.RequestStartDateTime).TotalMilliseconds));
                }
            }
        }
    }

}
