using Machinarium.Qnil;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public interface ISyncContext
    {
        void Invoke(Action syncAction);
        void Invoke<T>(Action<T> syncAction, T args);
        T Invoke<T>(Func<T> syncFunc);
        TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args);
    }

    public interface ISymbolManager : IDynamicDictionarySource<string, SymbolModel>
    {
        IFeedSubscription SubscribeAll();
    }
}
