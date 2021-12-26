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

namespace OBS
{

    /// <summary>
    /// Configuration parameters of ObsClient
    /// </summary>
    public partial class ObsConfig
    {

        private string _endpoint;


        #region Constructor 
        /// <summary>
        /// Constructor
        /// </summary>
        public ObsConfig()
        {
            Initialize();
        }

        #endregion

        /// <summary>
        /// Initialization
        /// </summary>
        protected virtual void Initialize()
        {
        }


        [Obsolete]
        public bool PathStyle
        {
            get;
            set;
        }

        /// <summary>
        /// OBS endpoint
        /// </summary>
        public string Endpoint
        {
            set
            {
                this._endpoint = value;

                if (string.IsNullOrEmpty(this._endpoint))
                {
                    throw new ObsException("Endpoint is null", ErrorType.Sender, null);
                }

                this._endpoint = this._endpoint.Trim();

                if (!this._endpoint.StartsWith("http://") && !this._endpoint.StartsWith("https://"))
                {
                    this._endpoint = "https://" + this._endpoint;
                }
                int index;
                while ((index = this._endpoint.LastIndexOf("/")) == this._endpoint.Length - 1)
                {
                    this._endpoint = this._endpoint.Substring(0, index);
                }

                if (CommonUtil.IsIP(this._endpoint))
                {
                    this.PathStyle = true;
                }
                else
                {
                    this.PathStyle = false;
                }
            }

            get
            {
                return this._endpoint;
            }
        }

    }
}


