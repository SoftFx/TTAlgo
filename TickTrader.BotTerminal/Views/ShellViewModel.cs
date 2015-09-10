using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen
    {
        private TraderModel model = new TraderModel();

        public ShellViewModel()
        {
            SymbolList = new SymbolListViewModel(model.Symbols);
            PositionList = new PositionListViewModel(model.Account);
            OrderList = new OrderListViewModel(model.Account);
            CanConnect = true;

            UpdateCommandStates(ConnectionModel.States.Offline, model.Connection.State.Current);
            model.Connection.State.StateChanged += UpdateCommandStates;
        }

        private void UpdateCommandStates(ConnectionModel.States oldState, ConnectionModel.States connectionState)
        {
            CanConnect = connectionState != ConnectionModel.States.Deinitializing;
            CanDisconnect = connectionState == ConnectionModel.States.Online;
            CanCancel = connectionState == ConnectionModel.States.Initializing || connectionState == ConnectionModel.States.WaitingLogon;
            NotifyOfPropertyChange("CanConnect");
            NotifyOfPropertyChange("CanDisconnect");
            NotifyOfPropertyChange("CanCancel");
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }
        public bool CanCancel { get; private set; }

        public async Task Connect()
        {
            try
            {
                //SetBusyConnecting(true);
                await model.Connection.ChangeConnection("tp.st.soft-fx.eu", "1000", "123");
                model.Connection.StartConnecting();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            //SetBusyConnecting(false);
        }

        public async Task Disconnect()
        {
            try
            {
                //SetBusyConnecting(true);
                await model.Connection.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            //SetBusyConnecting(false);
        }

        public async Task Cancel()
        {
            try
            {
                await model.Connection.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                model.Connection.StartDisconnecting();
        }

        //private void SetBusyConnecting(bool val)
        //{
        //    CanConnect = !val;
        //    NotifyOfPropertyChange("CanConnect");
        //}

        public SymbolListViewModel SymbolList { get; private set; }
        public PositionListViewModel PositionList { get; private set; }
        public OrderListViewModel OrderList { get; private set; }
    }
}
