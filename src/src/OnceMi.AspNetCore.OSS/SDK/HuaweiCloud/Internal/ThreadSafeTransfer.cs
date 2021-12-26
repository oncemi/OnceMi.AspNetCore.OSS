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
using OBS.Model;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OBS.Internal
{

    internal class ThreadSafeTransferStreamByBytes : TransferStreamManager
    {

        protected readonly object _lock = new object();
        protected bool flag = false;

        public ThreadSafeTransferStreamByBytes(object sender, EventHandler<TransferStatus> handler, long totalBytes,
            long transferredBytes, double intervalByBytes) : base(sender, handler, totalBytes, transferredBytes)
        {
            this.interval = intervalByBytes;
        }

        public override void TransferStart()
        {
            if (!flag)
            {
                lock (_lock)
                {
                    flag = true;
                    base.TransferStart();
                }
            }
        }

        public override void TransferReset(long resetBytes)
        {
            Interlocked.Add(ref transferredBytes, -resetBytes);
        }

        public override void TransferEnd()
        {
            lock (_lock){
                base.TransferEnd();
            }
        }

        protected override void DoBytesTransfered(int bytes)
        {
            Interlocked.Add(ref transferredBytes, bytes);
            Interlocked.Add(ref newlyTransferredBytes, bytes);
            DateTime now = DateTime.Now;
            IList<BytesUnit> currentInstantaneousBytes = this.CreateCurrentInstantaneousBytes(bytes, now);
            this.lastInstantaneousBytes = currentInstantaneousBytes;

            long _newlyTransferredBytes = Interlocked.Read(ref newlyTransferredBytes);
            long _transferredBytes = Interlocked.Read(ref transferredBytes);
            if (_newlyTransferredBytes >= this.interval && (_transferredBytes < totalBytes || totalBytes == -1))
            {
                if (Interlocked.CompareExchange(ref newlyTransferredBytes, 0, _newlyTransferredBytes) == _newlyTransferredBytes)
                {
                    TransferStatus status = new TransferStatus(_newlyTransferredBytes,
                       _transferredBytes, totalBytes, (now - lastCheckpoint).TotalSeconds, (now - startCheckpoint).TotalSeconds);
                    status.SetInstantaneousBytes(currentInstantaneousBytes);
                    handler(sender, status);
                    lastCheckpoint = now;
                }
            }
        }
    }


    internal class ThreadSafeTransferStreamBySeconds : ThreadSafeTransferStreamByBytes
    {

        private Timer timer;

        public ThreadSafeTransferStreamBySeconds(object sender, EventHandler<TransferStatus> handler, long totalBytes,
            long transferredBytes, double intervalBySeconds) : base(sender, handler, totalBytes, transferredBytes, intervalBySeconds)
        {
            
        }

        public void DoRecord(object state)
        {
            DateTime now = DateTime.Now;
            long _transferredBytes = Interlocked.Read(ref transferredBytes);
            if (_transferredBytes < this.totalBytes)
            {
                long _newlyTransferredBytes = Interlocked.Read(ref newlyTransferredBytes);
                TransferStatus status = new TransferStatus(_newlyTransferredBytes,
                    _transferredBytes, totalBytes, interval, (now - startCheckpoint).TotalSeconds);
                handler(sender, status);
                // Reset
                Interlocked.Add(ref newlyTransferredBytes, -_newlyTransferredBytes);
                lastCheckpoint = now;
            }
        }

        public override void TransferStart()
        {
            if (!flag)
            {
                lock (_lock)
                {
                    flag = true;
                    startCheckpoint = DateTime.Now;
                    lastCheckpoint = DateTime.Now;
                    timer = new Timer(this.DoRecord, null, 0, Convert.ToInt32(this.interval * 1000));
                }
            }
        }

        public override void TransferEnd()
        {
            lock (_lock)
            {
                timer?.Dispose();
                DateTime now = DateTime.Now;
                TransferStatus status = new TransferStatus(Interlocked.Read(ref newlyTransferredBytes),
                              Interlocked.Read(ref transferredBytes), totalBytes, (now - lastCheckpoint).TotalSeconds, (now - startCheckpoint).TotalSeconds);
                handler(sender, status);
            }
        }

        protected override void DoBytesTransfered(int bytes)
        {

            Interlocked.Add(ref transferredBytes, bytes);
            Interlocked.Add(ref newlyTransferredBytes, bytes);
        }
    }
}
