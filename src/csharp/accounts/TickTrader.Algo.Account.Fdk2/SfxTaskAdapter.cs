using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public static class SfxTaskAdapter
    {
        #region Helpers

        internal static void SetCompleted(object state)
        {
            SetCompleted<object>(state, null);
        }

        internal static void SetCompleted<T>(object state, T result)
        {
            if (state is RequestResultSource<T>)
            {
                var src = (RequestResultSource<T>)state;
                src.SetCompleted(result);
            }
            else if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetResult(result);
            }
        }

        internal static void SetFailed(object state, Exception ex)
        {
            SetFailed<object>(state, ex);
        }

        internal static void SetFailed<T>(object state, Exception ex)
        {

            if (state is RequestResultSource<T>)
            {
                var src = (RequestResultSource<T>)state;
                src.SetFailed(Convert(ex));
            }
            else if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetException(Convert(ex));
            }
        }

        internal static void TrySetCompleted(object state) => TrySetCompleted<object>(state, null);

        internal static bool TrySetCompleted<T>(object state, T result)
        {
            if (state is RequestResultSource<T> requestResSrc)
                return requestResSrc.TrySetCompleted(result);
            if (state is TaskCompletionSource<T> taskSrc)
                return taskSrc.TrySetResult(result);

            return false;
        }

        internal static bool TrySetFailed(object state, Exception ex) => TrySetFailed<object>(state, ex);

        internal static bool TrySetFailed<T>(object state, Exception ex)
        {
            if (state is RequestResultSource<T> requestResSrc)
                return requestResSrc.TrySetFailed(Convert(ex));
            else if (state is TaskCompletionSource<T> taskSrc)
                return taskSrc.TrySetException(Convert(ex));

            return false;
        }

        internal static Exception Convert(Exception ex)
        {
            if (ex is RejectException)
                return new InteropException(ex.Message, ConnectionErrorInfo.Types.ErrorCode.RejectedByServer);
            if (ex is FDK2.TimeoutException)
                return new InteropException(ex.Message, ConnectionErrorInfo.Types.ErrorCode.Timeout);
            return ex;
        }

        internal static Task<T> WithTimeout<T>(this TaskCompletionSource<T> taskSrc)
        {
            var timeoutCancelSrc = new CancellationTokenSource();
            Task.WhenAny(taskSrc.Task, Task.Delay(5 * 60 * 1000, timeoutCancelSrc.Token))
                .ContinueWith(t =>
                {
                    try
                    {
                        if (t.Result == taskSrc.Task)
                        {
                            timeoutCancelSrc.Cancel();
                        }
                        else
                        {
                            taskSrc.SetException(new InteropException("Request timed out.", ConnectionErrorInfo.Types.ErrorCode.Timeout));
                        }
                    }
                    finally
                    {
                        timeoutCancelSrc.Dispose();
                    }
                });
            return taskSrc.Task;
        }


        internal class RequestResultSource<T> : TaskCompletionSource<T>
        {
            private string _requestName;
            private long _creationTime;
            private long _reportTime;


            public RequestResultSource(string requestName)
            {
                _requestName = requestName;
                _creationTime = DateTime.Now.Ticks;
            }


            public void SetCompleted(T result)
            {
                //System.Diagnostics.Debug.WriteLine("RRS - report: " + Thread.CurrentThread.ManagedThreadId);
                _reportTime = DateTime.Now.Ticks;
                SetResult(result);
            }

            public void SetFailed(Exception ex)
            {
                _reportTime = DateTime.Now.Ticks;
                SetException(ex);
            }

            public bool TrySetCompleted(T result)
            {
                Interlocked.CompareExchange(ref _reportTime, DateTime.UtcNow.Ticks, 0);
                return TrySetResult(result);
            }

            public bool TrySetFailed(Exception ex)
            {
                Interlocked.CompareExchange(ref _reportTime, DateTime.UtcNow.Ticks, 0);
                return TrySetException(ex);
            }

            public string MeasureRequestTime()
            {
                //System.Diagnostics.Debug.WriteLine("RRS - result: " + Thread.CurrentThread.ManagedThreadId);
                var currentTime = DateTime.Now.Ticks;
                var sfxPing = (_reportTime - _creationTime) * 1e-4;
                var totalPing = (currentTime - _creationTime) * 1e-4;
                return $"Measured {_requestName}: sfxPing = {sfxPing} ms; totalPing = {totalPing} ms";
            }
        }

        #endregion
    }
}
