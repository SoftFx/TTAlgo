using ActorSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model
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

        internal static Exception Convert(Exception ex)
        {
            if (ex is RejectException)
                return new InteropException(ex.Message, ConnectionErrorCodes.RejectedByServer);
            if (ex is FDK2.TimeoutException)
                return new InteropException(ex.Message, ConnectionErrorCodes.Timeout);
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
                            taskSrc.SetException(new InteropException("Request timed out.", ConnectionErrorCodes.Timeout));
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
