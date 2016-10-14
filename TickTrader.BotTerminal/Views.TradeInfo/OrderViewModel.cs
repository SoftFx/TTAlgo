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
        private SymbolObserver _symbolObserver = new SymbolObserver();

        public OrderViewModel(OrderModel order, SymbolModel symbol)
        {
            _symbolObserver.ObservableSymbol = symbol;

            Order = order;
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;
        }

        public OrderModel Order { get; private set; }
        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }

        public RateDirectionTracker CurrentPrice => Order.OrderType != TradeRecordType.Position ?
                                                    Order.Side == TradeRecordSide.Buy ? _symbolObserver?.Ask : _symbolObserver?.Bid :
                                                    Order.Side == TradeRecordSide.Buy ? _symbolObserver?.Bid : _symbolObserver?.Ask;

        public void Dispose()
        {
            _symbolObserver?.Dispose();
        }
    }
}
