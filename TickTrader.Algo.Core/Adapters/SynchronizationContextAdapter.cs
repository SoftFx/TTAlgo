using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
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
