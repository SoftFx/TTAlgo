using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    class OrderViewModel : PropertyChangedBase, IDisposable
    {
        private static IndificationNumberGenerator _numberGenerator = new IndificationNumberGenerator();
        private SymbolModel symbol;

        public OrderViewModel(OrderModel order, SymbolModel symbol)
        {
            this.symbol = symbol;

            Order = order;
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;

            order.EssentialParametersChanged += o =>
            {
                NotifyOfPropertyChange(nameof(Price));
            };

            SortedNumber = GetSortedNumber(order);

            this.symbol.RateUpdated += UpdateDeviationPrice;
        }

        public OrderModel Order { get; private set; }
        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }
        public decimal? Price => Order.OrderType == OrderType.StopLimit || Order.OrderType == OrderType.Stop ? Order.StopPrice : Order.LimitPrice;

        public RateDirectionTracker CurrentPrice => Order.OrderType != OrderType.Position ?
                                                    Order.Side == OrderSide.Buy ? symbol?.AskTracker : symbol?.BidTracker :
                                                    Order.Side == OrderSide.Buy ? symbol?.BidTracker : symbol?.AskTracker;

        public decimal? DeviationPrice
        {
            get
            {
                double sPrice = (Order.Side == OrderSide.Buy ? symbol.CurrentAsk : symbol.CurrentBid) ?? 0;
                return Order.Side == OrderSide.Buy ? (decimal)sPrice - Price : Price - (decimal)sPrice;
            }
        }

        public string SortedNumber { get; }

        public void Dispose()
        {
        }

        private void UpdateDeviationPrice(SymbolModel model)
        {
            NotifyOfPropertyChange(nameof(DeviationPrice));
        }

        private string GetSortedNumber(OrderModel position)
        {
            return $"{position.Modified?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{position.Id}";
        }
    }
}
