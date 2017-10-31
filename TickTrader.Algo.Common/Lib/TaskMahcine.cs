using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Common.Lib
{
    public static class TaskMahcine
    {
        public static ActionBlock<Task> Create()
        {
            return new ActionBlock<Task>(t => t.RunSynchronously());
        }

        public static Task EnqueueTask(this ActionBlock<Task> machine, Action taskDef)
        {
            var task = new Task(taskDef);
            machine.Post(task);
            return task;
        }

        public static Task<T> EnqueueTask<T>(this ActionBlock<Task> machine, Func<T> taskDef)
        {
            var task = new Task<T>(taskDef);
            machine.Post(task);
            return task;
        }

        public static Task<bool> AddTimeout(this Task t, int timeoutMs)
        {
            return Task.WhenAny(t, Task.Delay(timeoutMs))
                .ContinueWith(wt => wt == t);
        }
    }
}
