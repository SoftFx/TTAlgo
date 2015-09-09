using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class AccountModel
    {
        public enum States { Offline, Initializing, Online }
        public enum Events { Connected, CacheInitialized, Diconnected }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private ObservableDictionary<string, PositionModel> positions = new ObservableDictionary<string, PositionModel>();
        private ObservableDictionary<long, OrderModel> orders = new ObservableDictionary<long, OrderModel>();
        private ConnectionModel connection;
        private ActionBlock<Action> uiUpdater;

        public AccountModel(ConnectionModel connection)
        {
            this.connection = connection;
            this.Positions = positions.AsReadonly();
            this.uiUpdater = DataflowHelper.CreateUiActionBlock<Action>(a => a(), 100, 100, CancellationToken.None);

            stateControl.AddTransition(States.Offline, Events.Connected, States.Initializing);
            stateControl.AddTransition(States.Initializing, Events.Diconnected, States.Offline);
            stateControl.AddTransition(States.Initializing, Events.CacheInitialized, States.Online);
            stateControl.AddTransition(States.Online, Events.Diconnected, States.Offline);

            connection.Initialized += () =>
                {
                    connection.TradeProxy.CacheInitialized += TradeProxy_CacheInitialized;
                    connection.TradeProxy.AccountInfo += TradeProxy_AccountInfo;
                    connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                    connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                };

            connection.Deinitialized += () =>
            {
                connection.TradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                connection.TradeProxy.AccountInfo -= TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
            };

            connection.Connected += () => stateControl.PushEvent(Events.Connected);
            connection.Disconnected += s => stateControl.PushEventAndAsyncWait(Events.Diconnected, States.Offline);
        }

        void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
        }

        void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
        }

        void TradeProxy_AccountInfo(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
        }

        void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            stateControl.PushEvent(Events.CacheInitialized);
        }

        public ReadonlyDictionaryObserver<string, PositionModel> Positions { get; private set; }
    }

    internal class PositionModel
    {
    }

    internal class OrderModel
    {
    }
}
