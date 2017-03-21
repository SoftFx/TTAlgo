using Machinarium.Qnil;
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
        T Invoke<T>(Func<T> syncFunc);
    }

    public interface ISymbolManager : IDynamicDictionarySource<string, SymbolModel>
    {
        IFeedSubscription SubscribeAll();
    }
}
