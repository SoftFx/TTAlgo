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

        public ManagedActivity(Func<CancellationToken, Task> activityFactory)
        {
            this.ActivityFactory = activityFactory;
        }

        protected Func<CancellationToken, Task> ActivityFactory { get; private set; }
        public Task Task { get; protected set; }

        public async Task Stop()
        {
            if (Task != null)
            {
                cancelSignal.Cancel();
                await Task;
                Task = null;
            }
        }

        public void Abrot()
        {
            cancelSignal.Cancel();
        }
    }

    /// <summary>
    /// Warning: This class is not thread safe. Thread synchronization is up to caller.
    /// </summary>
    public class RepeatableActivity : ManagedActivity
    {
        public RepeatableActivity(Func<CancellationToken, Task> activityFactory)
            : base(activityFactory)
        {
        }

        public void Invoke()
        {
            cancelSignal = new CancellationTokenSource();
            Task = ActivityFactory(cancelSignal.Token);
        }
    }

    /// <summary>
    /// Warning: This class is not thread safe. Thread synchronization is up to caller.
    /// </summary>
    public class TriggeredActivity : ManagedActivity
    {
        private bool triggered;

        public TriggeredActivity(Func<CancellationToken, Task> activityFactory)
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
            Task = ActivityFactory(cancelSignal.Token);
         }   
    }
}
