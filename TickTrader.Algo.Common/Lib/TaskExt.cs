using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class TaskExt
    {
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

        public static Task<bool> WaitAsync(this Task t, int timeoutMs)
        {
            return Task.WhenAny(t, Task.Delay(timeoutMs))
                .ContinueWith(wt => wt.Result == t);
        }
    }
}
