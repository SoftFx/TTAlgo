using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SymbolModel
    {
        public enum States { Offline, Online, UpdatingSubscription }

        private StateMachine<States> stateControl = new StateMachine<States>();

        public SymbolModel()
        {
            //stateControl.AddTransition(States.Offline, () => isConnected, States.Online);
            //stateControl.AddTransition(States.Online,  () => !isConnected, States.Offline,);
            //stateControl.AddTransition(States.UpdatingSubscription, () => !isConnected, States.Offline);
            //stateControl.AddTransition(States.Online, States.UpdatingSubscription, () => isModified);
        }
    }
}
