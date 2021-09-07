using System;
using System.Threading;

namespace TickTrader.Algo.Async.Actors
{
    public class SyncContextAdapter : SynchronizationContext
    {
        private readonly IMsgDispatcher _msgDispatcher;


        public SyncContextAdapter(IMsgDispatcher msgDispatcher)
        {
            _msgDispatcher = msgDispatcher;
        }


        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException("Should not Send() in Actor famework!");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _msgDispatcher.PostMessage(new CallbackMsg(d, state));
        }

        public void Enter()
        {
            SetSynchronizationContext(this);
        }

        public void Exit()
        {
            SetSynchronizationContext(null);
        }
    }
}
