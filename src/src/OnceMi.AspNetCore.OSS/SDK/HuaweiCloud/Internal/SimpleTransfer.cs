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
using System.IO;
using System.Text;

namespace OBS.Internal
{
    internal class TransferStream : Stream
    {
        internal delegate void BytesTransferred(int bytes);
        internal delegate void BytesAction(long bytes);
        internal delegate void EventDelegate();

        internal event BytesTransferred BytesReaded;
        internal event BytesTransferred BytesWrited;
        internal event BytesAction BytesReset;
        internal event EventDelegate StartWrite;
        internal event EventDelegate StartRead;
        internal event EventDelegate CloseStream;

        protected bool readFlag = false;
        protected bool writeFlag = false;
        protected long readedBytes = 0;

        internal Stream OriginStream { get; set; }

        public override bool CanRead
        {
            get
            {
                return this.OriginStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.OriginStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.OriginStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.OriginStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.OriginStream.Position;
            }
            set
            {
                this.writeFlag = false;
                this.readFlag = false;
                this.OriginStream.Position = value;
            }
        }

        public TransferStream(Stream originStream)
        {
            this.OriginStream = originStream;
        }

        public override void Flush()
        {
            this.OriginStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.writeFlag = false;
            this.readFlag = false;
            return this.OriginStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.OriginStream.SetLength(value);
        }

        public void ResetReadProgress()
        {
            BytesReset?.Invoke(this.readedBytes);
            readedBytes = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.readFlag)
            {
                this.readFlag = true;
                StartRead?.Invoke();
            }
            int bytes = this.OriginStream.Read(buffer, offset, count);
            readedBytes += bytes;
            BytesReaded?.Invoke(bytes);
            return bytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this.writeFlag)
            {
                this.writeFlag = true;
                StartWrite?.Invoke();
            }
            this.OriginStream.Write(buffer, offset, count);
            BytesWrited?.Invoke(count);
        }

        public override void Close()
        {
            try
            {
                this.OriginStream.Close();
            }
            finally
            {
                this.CloseStream?.Invoke();
            }
        }

    }

    internal class BytesUnit
    {
        public DateTime DateTime
        {
            get;
            set;
        }

        public long Bytes
        {
            set;
            get;
        }
    }

    internal abstract class TransferStreamManager
    {
        protected object sender;
        protected EventHandler<TransferStatus> handler;
        protected long totalBytes;
        protected long transferredBytes;
        protected long newlyTransferredBytes;
        protected DateTime startCheckpoint;
        protected DateTime lastCheckpoint;
        protected double interval;
        protected volatile IList<BytesUnit> lastInstantaneousBytes;

        public TransferStreamManager(object sender, EventHandler<TransferStatus> handler, long totalBytes,
            long transferredBytes)
        {
            this.sender = sender;
            this.handler = handler;
            this.totalBytes = totalBytes;
            this.transferredBytes = transferredBytes < 0 ? 0 : transferredBytes;
            startCheckpoint = DateTime.Now;
            lastCheckpoint = DateTime.Now;
        }

        public virtual void TransferStart()
        {
            this.startCheckpoint = DateTime.Now;
            this.lastCheckpoint = DateTime.Now;
            this.lastInstantaneousBytes = new List<BytesUnit>();
        }

        public virtual void TransferReset(long resetBytes)
        {
            this.startCheckpoint = DateTime.Now;
            this.lastCheckpoint = DateTime.Now;
            this.lastInstantaneousBytes = new List<BytesUnit>();
            this.newlyTransferredBytes = 0;
            this.transferredBytes -= resetBytes;
        }

        protected IList<BytesUnit> CreateCurrentInstantaneousBytes(long bytes, DateTime now)
        {
            IList<BytesUnit> currentInstantaneousBytes = new List<BytesUnit>();
            IList<BytesUnit> _lastInstantaneousBytes = this.lastInstantaneousBytes;
            if (_lastInstantaneousBytes != null)
            {
                foreach (BytesUnit item in _lastInstantaneousBytes)
                {
                    if ((now - item.DateTime).TotalMilliseconds < 1000)
                    {
                        currentInstantaneousBytes.Add(item);
                    }
                }
            }
            BytesUnit unit = new BytesUnit();
            unit.DateTime = now;
            unit.Bytes = bytes;
            currentInstantaneousBytes.Add(unit);
            return currentInstantaneousBytes;
        }

        public virtual void TransferEnd()
        {
            if(handler == null)
            {
                return;
            }
            DateTime now = DateTime.Now;
            TransferStatus status = new TransferStatus(newlyTransferredBytes,
                          transferredBytes, totalBytes, (now - lastCheckpoint).TotalSeconds, (now - startCheckpoint).TotalSeconds);
            status.SetInstantaneousBytes(this.CreateCurrentInstantaneousBytes(newlyTransferredBytes, now));
            handler(sender, status);
        }

        public void BytesTransfered(int bytes)
        {
            if (handler == null)
            {
                return;
            }

            if (bytes > 0)
            {
                this.DoBytesTransfered(bytes);
            }
        }

        protected abstract void DoBytesTransfered(int bytes);

    }

    internal class TransferStreamByBytes : TransferStreamManager
    {
        public TransferStreamByBytes(object sender, EventHandler<TransferStatus> handler, long totalBytes,
            long transferredBytes, double intervalByBytes) : base(sender, handler, totalBytes, transferredBytes)
        {
            this.interval = intervalByBytes;
        }

        protected override void DoBytesTransfered(int bytes)
        {
            transferredBytes += bytes;
            newlyTransferredBytes += bytes;
            DateTime now = DateTime.Now;
            IList<BytesUnit> currentInstantaneousBytes = this.CreateCurrentInstantaneousBytes(bytes, now);
            this.lastInstantaneousBytes = currentInstantaneousBytes;
            if (newlyTransferredBytes >= this.interval || transferredBytes == totalBytes)
            {
                TransferStatus status = new TransferStatus(newlyTransferredBytes,
                   transferredBytes, totalBytes, (now - lastCheckpoint).TotalSeconds, (now - startCheckpoint).TotalSeconds);
                status.SetInstantaneousBytes(currentInstantaneousBytes);
                handler(sender, status);
                // Reset
                newlyTransferredBytes = 0;
                lastCheckpoint = now;
            }
        }
    }

}
