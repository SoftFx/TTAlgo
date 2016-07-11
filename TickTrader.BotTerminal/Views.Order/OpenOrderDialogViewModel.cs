using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class OpenOrderDialogViewModel: Conductor<IOpenOrderDialogPage>.Collection.OneActive, IDisposable
    {        
        private MarketOrderPageViewModel marketOrderPage;
        private PendingOrderPageViewModel pendingOrderPage;
        private SymbolModel selectedSymbol;

        private TraderModel trade;

        public OpenOrderDialogViewModel(TraderModel trade, FeedModel feed, string preselectedSymbol)
        {
            this.trade = trade;

            Symbols = feed.Symbols.OrderBy((k, v) => k).Chain().AsObservable();

            marketOrderPage = new MarketOrderPageViewModel(this);
            pendingOrderPage = new PendingOrderPageViewModel(this);

            Items.Add(marketOrderPage);
            Items.Add(pendingOrderPage);

            UpdateState(trade.Connection.State.Current);
            trade.Connection.State.StateChanged += State_StateChanged;

            if (preselectedSymbol != null)
                SelectedSymbol = Symbols.FirstOrDefault(s => s.Name == preselectedSymbol);

            if (SelectedSymbol == null)
                SelectedSymbol = Symbols.FirstOrDefault();
        }

        #region Bindable Properties

        public bool IsEnabled { get; private set; }
        public IObservableListSource<SymbolModel> Symbols { get; private set; }
        public List<decimal> PredefinedAmounts { get; private set; }

        public SymbolModel SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                this.selectedSymbol = value;
                NotifyOfPropertyChange(nameof(SelectedSymbol));
                PredefinedAmounts = selectedSymbol.PredefinedAmounts;
                NotifyOfPropertyChange(nameof(PredefinedAmounts));
            }
        }

        #endregion

        public void Dispose()
        {
            trade.Connection.State.StateChanged -= State_StateChanged;
            Symbols.Dispose();
        }

        private void State_StateChanged(ConnectionModel.States from, ConnectionModel.States to)
        {
            UpdateState(to);
        }

        private void UpdateState(ConnectionModel.States state)
        {
            IsEnabled = state == ConnectionModel.States.Online;
        }
    }

    internal interface IOpenOrderDialogPage { }
}
