using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotAgent.Infrastructure
{
    public class SyncAdapter : ISyncContext
    {
        private object _sync;

        public SyncAdapter(object syncObj)
        {
            _sync = syncObj;
        }

        public void Invoke(Action syncAction)
        {
            lock (_sync) syncAction();
        }

        public void Invoke<T>(Action<T> syncAction, T args)
        {
            lock (_sync) syncAction(args);
        }

        public T Invoke<T>(Func<T> syncFunc)
        {
            lock (_sync) return syncFunc();
        }

        public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
        {
            lock (_sync) return syncFunc(args);
        }
    }
}
