using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    class OrderViewModel : PropertyChangedBase, IDisposable
    {
        private SymbolModel symbol;

        public OrderViewModel(OrderModel order, SymbolModel symbol)
        {
            this.symbol = symbol;

            Order = order;
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;
        }

        public OrderModel Order { get; private set; }
        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }

        public RateDirectionTracker CurrentPrice => Order.OrderType != TradeRecordType.Position ?
                                                    Order.Side == TradeRecordSide.Buy ? symbol?.AskTracker : symbol?.BidTracker :
                                                    Order.Side == TradeRecordSide.Buy ? symbol?.BidTracker : symbol?.AskTracker;

        public void Dispose()
        {
        }
    }
}
