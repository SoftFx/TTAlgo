using Machinarium.State;
using System;

namespace TickTrader.BotTerminal.Lib
{
    public class DispatcherStateMachineSync : IStateMachineSync
    {
        public void Synchronized(Action syncAction)
        {
            App.Current?.Dispatcher.Invoke(syncAction);
        }

        public T Synchronized<T>(Func<T> syncAction)
        {
            return App.Current.Dispatcher.Invoke(syncAction);
        }
    }
}
