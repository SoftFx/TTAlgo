using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public abstract class RepeatableActivity
    {
        public abstract Task Abort();
    }

    /// <summary>
    /// Warning: This class is not thread safe. Thread synchronization is up to caller.
    /// </summary>
    public class TriggeredActivity : RepeatableActivity
    {
        private Task currentTask;
        private Func<Task> activityFactory;
        private CancellationTokenSource cancelSignal;
        private bool triggered;

        public TriggeredActivity(Func<Task> activityFactory)
        {
            this.activityFactory = activityFactory;
        }

        public async void Trigger(bool cancelCurrent = false)
        {
            if (currentTask != null)
            {
                if (!triggered)
                {
                    triggered = true;
                    if (cancelCurrent) cancelSignal.Cancel();
                    await currentTask;
                }
                else // someone already wating for cancelation
                    return;
            }

            cancelSignal = new CancellationTokenSource();
            triggered = false;
            currentTask = activityFactory();
         }

        public async override Task Abort()
        {
            if (currentTask != null)
            {
                cancelSignal.Cancel();
                await currentTask;
            }
        }
    }
}
