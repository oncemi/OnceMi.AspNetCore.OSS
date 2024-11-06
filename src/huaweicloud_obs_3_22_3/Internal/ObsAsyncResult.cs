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
using System.Threading;

namespace OBS.Internal
{
    internal class ObsAsyncResult<V> : IAsyncResult, IDisposable
    {
        protected readonly object _state;

        protected readonly ManualResetEvent _event;

        protected AsyncCallback _callback;

        protected Exception _exception;

        protected bool _isCompleted = false;

        protected bool _disposed = false;

        protected V _result;

        public ObsAsyncResult(AsyncCallback callback, object state)
        {
            this._callback = callback;
            this._state = state;
            this._event = new ManualResetEvent(false);
        }

        public bool IsCompleted
        {
            get { return this._isCompleted; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this._isCompleted)
                {
                    this._event.Set();
                }
                return this._event;
            }
        }

        public AsyncCallback AsyncCallback
        {
            get { return this._callback; }
        }

        public object AsyncState
        {
            get { return this._state; }
        }

        public bool CompletedSynchronously
        {
            get;
            set;
        }

        public V Get()
        {
            if (!this._isCompleted)
            {
                this._event.WaitOne();
            }

            if (this._exception != null)
            {
                throw this._exception;
            }
            return this._result;
        }

        public virtual V Get(int millisecondsTimeout)
        {
            if (!this._isCompleted)
            {
                if (!this._event.WaitOne(millisecondsTimeout))
                {
                    throw new TimeoutException();
                }
            }

            if (this._exception != null)
            {
                throw this._exception;
            }
            return this._result;
        }

        public virtual void Reset(AsyncCallback callback)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("IAsyncResult is disposed");
            }
            this._isCompleted = false;
            this._event.Reset();
            this._result = default(V);
            this._exception = null;
            if(callback != null)
            {
                this._callback = callback;
            }
        }

        private void Notify()
        {
            this._isCompleted = true;
            if (!_disposed)
            {
                this._event.Set();
            }
            this._callback?.Invoke(this);
        }

        public void Set(V result)
        {
            this._result = result;
            this.Notify();
        }

        public void Set(Exception ex)
        {
            this._exception = ex;
            this.Notify();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing && this._event != null)
                {
                    this._event.Close();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
