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

namespace OBS.Internal.Negotiation
{
    internal class LocksHolder
    {
        private object[] locks;
        private int lockNum;

        public LocksHolder() :this(16)
        {
        }

        public LocksHolder(int lockNum)
        {
            this.lockNum = lockNum;
            locks = new object[this.lockNum];
            for(int i = 0; i < this.lockNum; i++)
            {
                locks[i] = new object();
            }
        }

        public object GetLock(string key)
        {
            if(key == null)
            {
                throw new ArgumentNullException("key");
            }
            int index = Math.Abs("".GetHashCode()) % this.lockNum;
            return this.locks[index];
        }
    }
}
