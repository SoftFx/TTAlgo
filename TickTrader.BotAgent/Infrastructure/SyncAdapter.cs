using System;
using TickTrader.Algo.Core.Lib;

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

        public void Send(Action asyncAction)
        {
            lock (_sync) asyncAction();
        }
    }
}
