using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
        }

        public static Task<bool> WaitAsync(this Task t, int timeoutMs)
        {
            return Task.WhenAny(t, Task.Delay(timeoutMs))
                .ContinueWith(wt => wt.Result == t);
        }
    }
}
