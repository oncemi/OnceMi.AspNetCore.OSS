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

namespace OBS.Model
{
    /// <summary>
    /// Content range of the object to be downloaded or copied
    /// </summary>
    public class ByteRange
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ByteRange()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        public ByteRange(long start, long end)
        {
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Start position, that is the sequence number of the byte from which the download or copy starts
        /// </summary>
        public long Start
        {
            get;
            set;
        }

        /// <summary>
        /// End position, that is the sequence number of the byte from which the download or copy ends
        /// </summary>
        public long End
        {
            get;
            set;
        }

    }
}


