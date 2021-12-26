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
using System.Collections.Generic;
using System.Globalization;

namespace OBS.Internal.Auth
{
    internal abstract class Signer
    {

        internal void DoAuth(HttpRequest request, HttpContext context, IHeaders iheaders)
        {
            if (request.Headers.ContainsKey(Constants.CommonHeaders.Authorization))
            {
                request.Headers.Remove(Constants.CommonHeaders.Authorization);
            }

            if (request.Headers.ContainsKey(Constants.CommonHeaders.Date))
            {
                request.Headers.Remove(Constants.CommonHeaders.Date);
            }

            //date
            if (!request.Headers.ContainsKey(iheaders.DateHeader()))
            {
                request.Headers.Add(Constants.CommonHeaders.Date, DateTime.UtcNow.ToString(Constants.RFC822DateFormat, Constants.CultureInfo));
            }

            //host
            string endpoint = string.IsNullOrEmpty(context.RedirectLocation) ? request.Endpoint : context.RedirectLocation;
            request.Headers[Constants.CommonHeaders.Host] = request.GetHost(endpoint);

            // anonymous user
            if (string.IsNullOrEmpty(context.SecurityProvider.Ak) || string.IsNullOrEmpty(context.SecurityProvider.Sk))
            {
                return;
            }

            if (!string.IsNullOrEmpty(context.SecurityProvider.Token) && !request.Headers.ContainsKey(iheaders.SecurityTokenHeader()))
            {
                request.Headers.Add(iheaders.SecurityTokenHeader(), context.SecurityProvider.Token.Trim());
            }
            this._DoAuth(request, context, iheaders);
        }

        internal abstract IDictionary<string,string> GetSignature(HttpRequest request, HttpContext context, IHeaders iheaders);

        protected abstract void _DoAuth(HttpRequest request, HttpContext context, IHeaders iheaders);
    }
}
