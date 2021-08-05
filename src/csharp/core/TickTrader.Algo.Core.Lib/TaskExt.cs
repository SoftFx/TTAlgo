using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class TaskExt
    {
        public static void Forget(this Task task)
        {
        }

        public static Task<bool> WaitAsync(this Task t, int timeoutMs)
        {
            return Task.WhenAny(t, Task.Delay(timeoutMs))
                .ContinueWith(wt => wt.Result == t);
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetCanceled(), tcs);
            return tcs.Task;
        }

        public static Task AddCancelation(this Task awaitable, CancellationToken cancellationToken)
        {
            return Task.WhenAny(awaitable, cancellationToken.WhenCanceled())
                .ContinueWith(t =>
                {
                    if (awaitable.IsCompleted)
                        awaitable.Wait(); // re-throw exceptions
                });
        }

        public static Task OnException(this Task awaitable, Action<Exception> action)
        {
            return awaitable.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    action(t.Exception);
                else if (t.IsCanceled)
                    action(new TaskCanceledException());
            });
        }


        public static void Schedule(int timeMs, Action action) => Schedule(timeMs, action, CancellationToken.None);

        public static void Schedule(int timeMs, Action action, CancellationToken cancelToken)
        {
            Task.Delay(timeMs, cancelToken).ContinueWith(_ => action(), TaskContinuationOptions.NotOnCanceled);
        }

        public static Task WrapSyncException(Func<Task> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        public static Task<T> WrapSyncException<T>(Func<Task<T>> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return Task.FromException<T>(ex);
            }
        }
    }
}
