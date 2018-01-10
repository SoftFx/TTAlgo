using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class OpenOrderDialogViewModel: Conductor<IOpenOrderDialogPage>.Collection.OneActive, IDisposable
    {        
        private MarketOrderPageViewModel marketOrderPage;
        private PendingOrderPageViewModel pendingOrderPage;
        private SymbolModel selectedSymbol;

        private TraderClientModel clientModel;

        //public event System.Action DepthChanged;

        public OpenOrderDialogViewModel(TraderClientModel clientModel, string preselectedSymbol)
        {
            this.clientModel = clientModel;

            Symbols = clientModel.Symbols.Select((k,v) => (SymbolModel)v).OrderBy((k, v) => k).Chain().AsObservable();

            marketOrderPage = new MarketOrderPageViewModel(this);
            pendingOrderPage = new PendingOrderPageViewModel(this);

            Items.Add(marketOrderPage);
            Items.Add(pendingOrderPage);

            UpdateState(clientModel.Connection.State);
            clientModel.Connection.StateChanged += State_StateChanged;

            if (preselectedSymbol != null)
                SelectedSymbol = Symbols.FirstOrDefault(s => s.Name == preselectedSymbol);

            if (SelectedSymbol == null)
                SelectedSymbol = Symbols.FirstOrDefault();
        }

        #region Bindable Properties

        public bool IsEnabled { get; private set; }
        public IObservableListSource<SymbolModel> Symbols { get; private set; }
        public List<decimal> PredefinedAmounts { get; private set; }

        public RateDirectionTracker Bid { get; private set; }
        public RateDirectionTracker Ask { get; private set; }

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

        public int Depth
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        public void Dispose()
        {
            clientModel.Connection.StateChanged -= State_StateChanged;
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

        public void OnRateUpdate(Quote tick)
        {
            if (tick.HasAsk)
                Ask.Rate = tick.Ask;

            if (tick.HasBid)
                Ask.Rate = tick.Bid;
        }
    }

    internal interface IOpenOrderDialogPage { }
}
