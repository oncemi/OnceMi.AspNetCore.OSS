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
using System.Text;

namespace OBS.Model
{
    /// <summary>
    /// SSE-C encryption/decryption headers
    /// </summary>
    public class SseCHeader : SseHeader
    {
        /// <summary>
        /// SSE-C encryption/decryption algorithm
        /// </summary>
        public SseCAlgorithmEnum Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Key used in the SSE-C encryption/decryption mode. The key is used to encrypt and decrypt an object. The value is a Base64-encoded key.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which cannot be used with "Key".
        /// </para>
        /// </remarks>
        public string KeyBase64
        {
            get;
            set;
        }

        /// <summary>
        /// Key used in the SSE-C encryption/decryption mode. The key is used to encrypt and decrypt an object. The value is a key that is not encoded using Base64.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter, which cannot be used with "KeyBase64".
        /// </para>
        /// </remarks>
        public byte[] Key
        {
            get;
            set;
        }
    }
}


