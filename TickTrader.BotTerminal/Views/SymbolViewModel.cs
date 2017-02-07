using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    class SymbolViewModel : PropertyChangedBase
    {
        public enum States { Collapsed, Expanded, ExpandedWithLevel2 }

        private SymbolModel model;
        private bool isSelected;
        private States currentState;
        private iOrderUi _orderUi;
        private DateTime _quoteTime;
        private IFeedSubscription subscription;

        public SymbolViewModel(SymbolModel model, iOrderUi orderUi)
        {
            this.model = model;
            subscription = model.Subscribe();
            subscription.NewQuote += OnRateUpdate;
            this._orderUi = orderUi;

            this.Bid = model.BidTracker;
            this.Ask = model.AskTracker;

            Bid.Precision = model.Descriptor.Precision;
            Ask.Precision = model.Descriptor.Precision;

            this.DetailsPanel = new SymbolDetailsViewModel(Ask, Bid);
            this.Level2Panel = new SymbolLevel2ViewModel();
            if (model.Descriptor.Features.IsColorSupported)
                Color = model.Descriptor.Color;

            this.DetailsPanel.OnBuyClick = () => _orderUi.OpenMarkerOrder(model.Name);
            this.DetailsPanel.OnSellClick = () => _orderUi.OpenMarkerOrder(model.Name);
        }

        public string SymbolName { get { return model.Name; } }
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

        private void OnRateUpdate(Quote tick)
        {
            if (tick.HasBid)
                Bid.Rate = tick.Bid;

            if (tick.HasAsk)
                Ask.Rate = tick.Ask;

            QuoteTime = tick.CreatingTime;
        }

        public void OpenOrder()
        {
            _orderUi.OpenMarkerOrder(model.Name);
        }

        public void OpenChart()
        {
            NewChartRequested(this.model.Descriptor.Name);
        }

        public event Action<string> NewChartRequested = delegate { };

        public void Close()
        {
            subscription.Dispose();
        }
    }
}