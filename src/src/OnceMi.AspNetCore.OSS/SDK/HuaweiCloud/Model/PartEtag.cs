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

namespace OBS.Model
{
    /// <summary>
    /// Part information
    /// </summary>
    public class PartETag : IComparable<PartETag>
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public PartETag()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="partNumber">Part number</param>
        /// <param name="etag">Part ETag</param>
        public PartETag(int partNumber, string etag)
        {
            this.PartNumber = partNumber;
            this.ETag = etag;
        }

        /// <summary>
        /// Compare with another part.
        /// </summary>
        /// <param name="other">Information of the other part</param>
        /// <returns>If the value is true, the two part numbers are the same.</returns>
        public int CompareTo(PartETag other)
        {
            if (other == null)
            {
                return 1;
            }
            return this.PartNumber.CompareTo(other.PartNumber);
        }

        /// <summary>
        /// Part numberc
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        ///  </para> 
        /// </remarks>
        public int PartNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Part ETag
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        ///  </para> 
        /// </remarks>
        public string ETag
        {
            get;
            set;
        }

    }
}


