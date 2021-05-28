using Machinarium.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public class DispatcherStateMachineSync : IStateMachineSync
    {
        public void Synchronized(Action syncAction)
        {
            App.Current.Dispatcher.Invoke(syncAction);
        }

        public T Synchronized<T>(Func<T> syncAction)
        {
            return App.Current.Dispatcher.Invoke(syncAction);
        }
    }
}
