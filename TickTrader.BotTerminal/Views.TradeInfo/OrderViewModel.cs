using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class OrderViewModel : PropertyChangedBase, IDisposable
    {
        private OrderModel _order;
        private SymbolObserver _symbolObserver = new SymbolObserver();

        public OrderViewModel(OrderModel order)
        {
            Order = order;
        }
        public OrderViewModel(OrderModel order, SymbolModel symbol) : this(order)
        {
            _symbolObserver.ObservableSymbol = symbol;
            NotifyOfPropertyChange(nameof(CurrentPrice));
        }

        public OrderModel Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    NotifyOfPropertyChange(nameof(Order));
                }
            }
        }
        public RateDirectionTracker CurrentPrice => Order.OrderType != TradeRecordType.Position ?
                                                    Order.Side == TradeRecordSide.Buy ? _symbolObserver?.Ask : _symbolObserver?.Bid :
                                                    Order.Side == TradeRecordSide.Buy ? _symbolObserver?.Bid : _symbolObserver?.Ask;

        public void Dispose()
        {
            _symbolObserver?.Dispose();
        }
    }
}
