using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public abstract class ManagedActivity
    {
        protected CancellationTokenSource cancelSignal;

        public ManagedActivity(Func<Task> activityFactory)
        {
            this.ActivityFactory = activityFactory;
        }

        protected Func<Task> ActivityFactory { get; private set; }
        public Task Task { get; protected set; }

        public async Task Abort()
        {
            if (Task != null)
            {
                cancelSignal.Cancel();
                await Task;
                Task = null;
            }
        }
    }

    /// <summary>
    /// Warning: This class is not thread safe. Thread synchronization is up to caller.
    /// </summary>
    public class RepeatableActivity : ManagedActivity
    {
        public RepeatableActivity(Func<Task> activityFactory)
            : base(activityFactory)
        {
        }

        public void Invoke()
        {
            cancelSignal = new CancellationTokenSource();
            Task = ActivityFactory();
        }
    }

    /// <summary>
    /// Warning: This class is not thread safe. Thread synchronization is up to caller.
    /// </summary>
    public class TriggeredActivity : ManagedActivity
    {
        private bool triggered;

        public TriggeredActivity(Func<Task> activityFactory)
            : base(activityFactory)
        {
        }

        public async void Trigger(bool cancelCurrent = false)
        {
            if (Task != null)
            {
                if (!triggered)
                {
                    triggered = true;
                    if (cancelCurrent) cancelSignal.Cancel();
                    await Task;
                }
                else // someone already wating for cancelation
                    return;
            }

            cancelSignal = new CancellationTokenSource();
            triggered = false;
            Task = ActivityFactory();
         }   
    }
}
