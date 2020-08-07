using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.RpcTest
{
    class NullSyncContext : ISyncContext
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

        public T Invoke<T>(Func<T> syncFunc)
        {
            return syncFunc();
        }

        public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
        {
            return syncFunc(args);
        }
    }
}
