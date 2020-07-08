using System;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.RpcTest
{
    class NullSyncContext : ISynchronizationContext
    {
        public void Invoke(Action action)
        {
            action();
        }

        public void Invoke<T>(Action<T> action, T arg)
        {
            action(arg);
        }

        public void Send(Action action)
        {
            action();
        }
    }
}
