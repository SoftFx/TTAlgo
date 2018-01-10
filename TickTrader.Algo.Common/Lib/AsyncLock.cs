using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public class AsyncLock
    {
        private Queue<TaskCompletionSource<IDisposable>> _waiters = new Queue<TaskCompletionSource<IDisposable>>();
        private bool _isTaken;

        public Task<IDisposable> GetLock()
        {
            return Enter(CancellationToken.None);
        }

        public Task<IDisposable> GetLock(CancellationToken cancelToken)
        {
            return Enter(cancelToken);
        }

        public Task InvokeExlusive(Action action)
        {
            return InvokeExlusive(action, CancellationToken.None);
        }

        //public async Task InvokeExlusive(Action action, CancellationToken cancelToken)
        //{
        //    using (await GetLock(cancelToken))
        //        action();
        //}

        public Task InvokeExlusive(Action action, CancellationToken cancelToken)
        {
            Action<Task<IDisposable>> invoker = t =>
            {
                try
                {
                    action();
                }
                finally
                {
                    Exit();
                }
            };

            return GetLock(cancelToken).ContinueWith(invoker, TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<T> InvokeExlusive<T>(Func<T> action)
        {
            return InvokeExlusive<T>(action, CancellationToken.None);
        }

        public Task<T> InvokeExlusive<T>(Func<T> action, CancellationToken cancelToken)
        {
            Func<Task<IDisposable>, T> invoker = t =>
            {
                try
                {
                    return action();
                }
                finally
                {
                    Exit();
                }
            };

            return GetLock(cancelToken).ContinueWith(invoker, TaskContinuationOptions.ExecuteSynchronously);
        }

        private Task<IDisposable> Enter(CancellationToken cancelToken)
        {
            var src = new TaskCompletionSource<IDisposable>();

            if (cancelToken.CanBeCanceled)
                cancelToken.Register(() => src.TrySetCanceled());

            lock (_waiters)
            {
                if (_isTaken)
                {
                    _waiters.Enqueue(src);
                }
                else
                {
                    _isTaken = true;
                    Task.Factory.StartNew(() => src.TrySetResult(new LockToken(this)));
                }
            }

            return src.Task;
        }

        private void Exit()
        {
            lock (_waiters)
            {
                if (_isTaken)
                    throw new InvalidOperationException("Cannot release lock: it is not taken!");

                while (_waiters.Count > 0)
                {
                    var next = _waiters.Dequeue();
                    if (next.TrySetResult(new LockToken(this)))
                        return;
                }

                _isTaken = false;
            }
        }

        private class LockToken : IDisposable
        {
            private AsyncLock _lock;

            public LockToken(AsyncLock lockObj)
            {
                _lock = lockObj;
            }

            public void Dispose()
            {
                _lock.Exit();
            }
        }
    }
}
