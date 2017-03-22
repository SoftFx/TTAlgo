using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    public class DispatcherSync : ISyncContext
    {
        public void Invoke(Action syncAction)
        {
            App.Current.Dispatcher.Invoke(syncAction);
        }

        public T Invoke<T>(Func<T> syncFunc)
        {
            return App.Current.Dispatcher.Invoke<T>(syncFunc);
        }
    }
}
