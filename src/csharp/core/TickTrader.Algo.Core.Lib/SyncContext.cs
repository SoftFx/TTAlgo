using System;

namespace TickTrader.Algo.Core.Lib
{
    public interface ISyncContext
    {
        void Invoke(Action syncAction);
        void Invoke<T>(Action<T> syncAction, T args);
        T Invoke<T>(Func<T> syncFunc);
        TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args);
        void Send(Action asyncAction);
    }
}
