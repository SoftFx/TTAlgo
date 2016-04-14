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
            Caliburn.Micro.Execute.OnUIThread(syncAction);
        }
    }
}
