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
using System.Collections.Generic;
using System.Text;

namespace OBS.Model
{
    /// <summary>
    /// Data transmission status
    /// </summary>
    public class TransferStatus : EventArgs
    {
        private long _newlyTransferredBytes;
        private long _transferredBytes;
        private long _totalBytes;
        private double _intervalSeconds;
        private double _totalSeconds;
        private IList<BytesUnit> _instantaneousBytes;

        internal TransferStatus(long newlyTransferredBytes, long transferredBytes, long totalBytes,
            double intervalSeconds, double totalSeconds)
        {
            this._newlyTransferredBytes = newlyTransferredBytes;
            this._transferredBytes = transferredBytes;
            this._totalBytes = totalBytes;
            this._intervalSeconds = intervalSeconds;
            this._totalSeconds = totalSeconds;
        }

        internal void SetInstantaneousBytes(IList<BytesUnit> instantaneousBytes)
        {
            this._instantaneousBytes = instantaneousBytes;
        }

        /// <summary>
        /// Instantaneous rate
        /// </summary>
        public double InstantaneousSpeed
        {
            get {
                if(this._instantaneousBytes != null)
                {
                    long instantaneousSpeed = 0;
                    foreach (BytesUnit item in this._instantaneousBytes)
                    {
                        instantaneousSpeed += item.Bytes;
                    }
                    return instantaneousSpeed;
                }
                return this._newlyTransferredBytes / this._intervalSeconds;
            }
        }

        /// <summary>
        /// Average rate
        /// </summary>
        public double AverageSpeed
        {
           get { return this._transferredBytes / this._totalSeconds; }
        }

        /// <summary>
        /// Transmission progress
        /// </summary>
        public int TransferPercentage
        {
            get {
                if(this._totalBytes < 0)
                {
                    return -1;
                }else if(this._totalBytes == 0)
                {
                    return 100;
                }
                return (int)((this._transferredBytes * 100) / this._totalBytes);
            }
        }

        /// <summary>
        /// Number of bytes transmitted since last progress refresh
        /// </summary>
        public long NewlyTransferredBytes
        {
            get { return this._newlyTransferredBytes; }
        }

        /// <summary>
        /// Number of bytes that have been transmitted
        /// </summary>
        public long TransferredBytes
        {
            get { return this._transferredBytes; }
        }

        /// <summary>
        /// Number of bytes being transmitted
        /// </summary>
        public long TotalBytes
        {
            get { return this._totalBytes; }
        }

    }
}
