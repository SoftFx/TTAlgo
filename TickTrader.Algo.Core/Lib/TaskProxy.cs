using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class TaskProxy<T> : CrossDomainObject
    {
        private TaskCompletionSource<T> taskSrc = new TaskCompletionSource<T>();

        public TaskProxy()
        {
        }

        public void SetCompleted(T result)
        {
            taskSrc.SetResult(result);
        }

        public void SetException(Exception ex)
        {
            taskSrc.SetException(ex);
        }

        public Task<T> LocalTask { get { return taskSrc.Task; } }
    }

    public static class TaskProxyExtention
    {
        public static void Attach<T>(this TaskProxy<T> proxy, Task<T> task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    proxy.SetException(t.Exception);
                else
                    proxy.SetCompleted(t.Result);
            });
        }
    }
}
