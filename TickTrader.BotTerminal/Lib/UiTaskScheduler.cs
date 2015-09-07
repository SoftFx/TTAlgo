using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TickTrader.BotTerminal.Lib
{
    public class UiTaskScheduler : TaskScheduler
    {
        private readonly Dispatcher dispatcher;
        private readonly DispatcherPriority priority;

        public UiTaskScheduler(
            Dispatcher dispatcher, DispatcherPriority priority)
        {
            this.dispatcher = dispatcher;
            this.priority = priority;
        }

        protected override bool TryDequeue(Task task)
        {
            return base.TryDequeue(task);
        }

        protected override void QueueTask(Task task)
        {
            dispatcher.BeginInvoke(new Action(() => TryExecuteTask(task)), priority);
        }

        protected override bool TryExecuteTaskInline(
            Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotSupportedException();
        }
    }
}
