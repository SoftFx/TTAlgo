using System;
using System.Threading;

namespace TickTrader.Algo.CoreV1
{
    internal class SynchronizationContextAdapter : SynchronizationContext
    {
        public Action<SendOrPostCallback, object> OnAsyncAction { get; set; }

        public override void Post(SendOrPostCallback d, object state)
        {
            OnAsyncAction(d, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException();
        }
    }
}
