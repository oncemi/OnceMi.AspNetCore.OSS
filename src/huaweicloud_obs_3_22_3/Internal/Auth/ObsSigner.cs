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

namespace OBS.Internal.Auth
{
    internal class ObsSigner : AbstractSigner
    {

        private static ObsSigner instance = new ObsSigner();

        private ObsSigner()
        {

        }

        public static Signer GetInstance()
        {
            return instance;
        }

        protected override string GetAuthPrefix()
        {
            return "OBS";
        }
    }
}
