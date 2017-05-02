using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.Infrastructure
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

        public T Invoke<T>(Func<T> syncFunc)
        {
            lock (_sync) return syncFunc();
        }
    }
}
