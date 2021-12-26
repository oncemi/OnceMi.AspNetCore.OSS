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
using System.Net;
using OBS.Internal;

namespace OBS
{
    /// <summary>
    /// Configuration parameters of ObsClient
    /// </summary>
    public partial class ObsConfig
    {

        private AuthTypeEnum authType = Constants.DefaultAuthType;
        private int maxErrorRetry = Constants.DefaultMaxErrorRetry;
        private int connectTimeout = Constants.DefaultConnectTimeout;
        private int bufferSize = Constants.DefaultBufferSize;
        private int connectionLimit = Constants.DefaultConnectionLimit;
        private int maxIdleTime = Constants.DefaultMaxIdleTime;
        private int readWriteTimeout = Constants.DefaultReadWriteTimeout;
        private int receiveBufferSize = Constants.DefaultBufferSize;
        private int asyncSocketTimeout = Constants.DefaultAsyncSocketTimeout;
        private bool keepAlive = Constants.DefaultKeepAlive;
        private bool authTypeNegotiation = Constants.DefaultAuthTypeNegotiation;

        /// <summary>
        /// Whether to verify the certificate
        /// </summary>
        public bool ValidateCertificate
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to negotiate the authentication mode. The default value is "true."
        /// </summary>
        public bool AuthTypeNegotiation
        {
            get
            {
                return this.authTypeNegotiation;
            }
            set
            {
                this.authTypeNegotiation = value;
            }
        }

        /// <summary>
        /// Authentication mode used for accessing OBS. When protocol negotiation is enabled, this parameter is ineffective.
        /// </summary>
        public AuthTypeEnum AuthType
        {
            get { return this.authType; }
            set { this.authType = value; }
        }

        /// <summary>
        /// Maximum number of retry attempts upon a request failure. The default value is 3.
        /// </summary>
        public int MaxErrorRetry
        {
            get { return this.maxErrorRetry; }
            set { this.maxErrorRetry = value < 0 ? 0 : value; }
        }

        /// <summary>
        /// Size of the socket reception buffer
        /// </summary>
        public int ReceiveBufferSize
        {
            get { return this.receiveBufferSize; }
            set { this.receiveBufferSize = value <= 0 ? Constants.DefaultBufferSize : value; }
        }

        /// <summary>
        /// Read/write cache size during an object upload
        /// </summary>
        public int BufferSize
        {
            get { return this.bufferSize; }
            set { this.bufferSize = value <= 0 ? Constants.DefaultBufferSize : value; }
        }

        /// <summary>
        /// HTTPS protocol type
        /// </summary>
        public SecurityProtocolType? SecurityProtocolType
        {
            get;
            set;
        }


        /// <summary>
        /// Request timeout interval. The unit is millisecond.
        /// </summary>
        public int Timeout
        {
            get { return this.connectTimeout; }
            set
            {
                this.connectTimeout = value <= 0 ? Constants.DefaultConnectTimeout : value;
            }
        }

        /// <summary>
        /// Asynchronous request timeout interval. The unit is millisecond.
        /// </summary>
        public int AsyncSocketTimeout
        {
            get { return this.asyncSocketTimeout; }
            set
            {
                this.asyncSocketTimeout = value <= 0 ? Constants.DefaultAsyncSocketTimeout : value;
            }
        }

        /// <summary>
        /// Whether to use persistent connections. The default value is "true."
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return this.keepAlive;
            }
            set
            {
                this.keepAlive = value;
            }
        }


        /// <summary>
        /// Proxy address
        /// </summary>
        public string ProxyHost
        {
            get;
            set;
        }


        /// <summary>
        /// Proxy port
        /// </summary>
        public int ProxyPort
        {
            get;
            set;
        }

        /// <summary>
        /// Username used for connecting to the proxy server
        /// </summary>
        public string ProxyUserName
        {
            get;
            set;
        }

        /// <summary>
        /// Password used for connecting to the proxy server
        /// </summary>
        public string ProxyPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Domain to which the proxy belongs
        /// </summary>
        public string ProxyDomain
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum idle time for obtaining connections from the connection pool. The unit is millisecond.
        /// The default value is 30000.
        /// </summary>
        public int MaxIdleTime
        {
            get { return this.maxIdleTime; }
            set
            {
                if (value <= 0)
                {
                    this.maxIdleTime = Constants.DefaultMaxIdleTime;
                }
                else
                {

                    this.maxIdleTime = value;
                }
            }
        }

        /// <summary>
        /// Maximum number of concurrently opened HTTP connections
        /// The default value is 1000.
        /// </summary>
        public int ConnectionLimit
        {
            get { return this.connectionLimit; }
            set
            {
                this.connectionLimit = value <= 0 ? Constants.DefaultConnectionLimit : value;
            }
        }


        /// <summary>
        /// Data read/write timeout interval. The unit is millisecond.
        /// The default value is 60000.
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return this.readWriteTimeout; }
            set
            {
                this.readWriteTimeout = value <= 0 ? Constants.DefaultReadWriteTimeout : value;
            }
        }

    }
}


