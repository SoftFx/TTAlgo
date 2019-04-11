using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Common;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class SymbolViewModel : PropertyChangedBase
    {
        public enum States { Collapsed, Expanded, ExpandedWithLevel2 }

        private SymbolModel _model;
        private bool isSelected;
        private States currentState;
        private IShell _shell;
        private DateTime _quoteTime;
        private IFeedSubscription subscription;

        public SymbolViewModel(SymbolModel model, QuoteDistributor distributor, IShell shell)
        {
            _model = model;
            _shell = shell;
            subscription = distributor.Subscribe(model.Name);
            subscription.NewQuote += OnRateUpdate;

            Bid = model.BidTracker;
            Ask = model.AskTracker;

            OnInfoUpdated(_model);
            _model.InfoUpdated += OnInfoUpdated;

            DetailsPanel = new SymbolDetailsViewModel(Ask, Bid);
            Level2Panel = new SymbolLevel2ViewModel();

            Color = model.Descriptor.Color;

            if (_shell != null)
            {
                this.DetailsPanel.OnBuyClick = () => _shell.OrderCommands.OpenMarkerOrder(model.Name);
                this.DetailsPanel.OnSellClick = () => _shell.OrderCommands.OpenMarkerOrder(model.Name);
            }
        }

        public string SymbolName { get { return _model.Name; } }
        public string Group { get { return "Forex"; } }
        public int Color { get; private set; }

        public RateDirectionTracker Bid { get; private set; }
        public RateDirectionTracker Ask { get; private set; }
        public DateTime QuoteTime
        {
            get { return _quoteTime; }
            private set
            {
                if (_quoteTime != value)
                {
                    _quoteTime = value;
                    NotifyOfPropertyChange(nameof(QuoteTime));
                }
            }
        }
        public int Depth { get; private set; }
        public SymbolDetailsViewModel DetailsPanel { get; private set; }
        public SymbolLevel2ViewModel Level2Panel { get; private set; }

        #region State Management

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    NotifyOfPropertyChange("IsSelected");
                    if (value)
                        State = States.Expanded;
                    else
                        State = States.Collapsed;
                }
            }
        }

        public States State
        {
            get { return currentState; }
            set
            {
                this.currentState = value;
                NotifyOfPropertyChange("State");
                NotifyOfPropertyChange("IsExpanded");
                NotifyOfPropertyChange("IsLevel2Visible");
            }
        }

        public bool IsExpanded { get { return (State == States.Expanded || State == States.ExpandedWithLevel2); } }
        public bool IsLevel2Visible { get { return State == States.ExpandedWithLevel2; } }

        public void TriggerState()
        {
            if (!isSelected)
                return;

            if (State == States.Collapsed)
                State = States.Expanded;
            else if (State == States.Expanded)
                State = States.ExpandedWithLevel2;
            else if (State == States.ExpandedWithLevel2)
                State = States.Collapsed;
        }

        #endregion

        private void OnRateUpdate(QuoteEntity tick)
        {
            QuoteTime = tick.CreatingTime;
        }

        private void OnInfoUpdated(SymbolModel symbol)
        {
            Bid.Precision = symbol.Descriptor.Precision;
            Ask.Precision = symbol.Descriptor.Precision;
        }

        public void OpenOrder()
        {
            _shell.OrderCommands.OpenMarkerOrder(_model.Name);
        }

        public void OpenChart()
        {
            _shell.OpenChart(SymbolName);
        }

        public void Close()
        {
            subscription.Dispose();
        }
    }
}