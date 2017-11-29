using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Lib
{
    public class AsyncBuffer<T> : IAsyncEnumerator<T>
    {
        private static readonly Task<bool> TrueResult = Task.FromResult(true);
        private static readonly Task<bool> FalseResult = Task.FromResult(false);

        private object _lockObj = new object();
        private TaskCompletionSource<T> _reader;
        private TaskCompletionSource<bool> _writer;
        private T _currentVal;
        private T _waitingVal;
        private bool isClosed;
        private Exception _exception;
#if DEBUG
        private static int IdSeed = 1;
        private int id = System.Threading.Interlocked.Increment(ref IdSeed);

        private void Trace(string msg)
        {
            System.Diagnostics.Debug.WriteLine("AsyncBuffer[{0}] t={2} {1}", id, msg, System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
#endif
        public bool Write(T item)
        {
            return WriteAsync(item).Result;
        }

        public Task<bool> WriteAsync(T item)
        {
            lock (_lockObj)
            {
                if (isClosed)
                    return FalseResult;

                if (_reader != null)
                {
                    Trace("Write: Hit reader!");
                    Task.Factory.StartNew(r => ((TaskCompletionSource<T>)r).SetResult(item), _reader);
                    _reader = null;
                    return TrueResult;
                }
                else
                {
                    if (_writer != null)
                        throw new InvalidOperationException("Concurrent write!");

                    Trace("Write: Wait for read!");

                    _writer = new TaskCompletionSource<bool>();
                    _waitingVal = item;
                    return _writer.Task;
                }
            }
        }

        public void SetFailed(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");

            Close(ex);
        }

        public void Dispose()
        {
            Close();
        }

        private void Close(Exception ex = null)
        {
            lock (_lockObj)
            {
                if (!isClosed)
                {
                    _exception = ex;

                    isClosed = true;
                    if (_reader != null)
                    {
                        if (ex != null)
                            _reader.SetException(ex);
                        else
                            _reader.SetCanceled();
                        _reader = null;
                    }
                    if (_writer != null)
                    {
                        _writer.SetResult(false);
                        _writer = null;
                    }
                }
            }
        }

        #region IAsyncEnumerator<T>

        T IAsyncEnumerator<T>.Current => _currentVal;

        Task<bool> IAsyncEnumerator<T>.Next()
        {
            lock (_lockObj)
            {
                if (_writer != null)
                {
                    Trace("Read: Hit writer!");

                    Task.Factory.StartNew(w => ((TaskCompletionSource<bool>)w).SetResult(true), _writer);
                    _writer = null;
                    _currentVal = _waitingVal;
                    return TrueResult;
                }
                else if (isClosed)
                {
                    if (_exception != null)
                        throw _exception;
                    return FalseResult;
                }
                else
                {
                    if (_reader != null)
                        throw new InvalidOperationException("Concurrent read!");

                    Trace("Read: Wait for read!");

                    _reader = new TaskCompletionSource<T>();
                    return _reader.Task.ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                            return false;
                        _currentVal = t.Result;
                        return true;
                    });
                }
            }
        }

        #endregion
    }
}
