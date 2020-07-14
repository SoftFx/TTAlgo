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
        private AccountModel account;
        private SymbolModel symbol;

        public OrderViewModel(OrderModel order, SymbolModel symbol, AccountModel account)
        {
            this.symbol = symbol;
            this.account = account;

            Order = order;
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = account.BalanceDigits;

            order.EssentialParametersChanged += o =>
            {
                NotifyOfPropertyChange(nameof(Price));
            };

            SortedNumber = GetSortedNumber(order);

            if (this.symbol != null) // server misconfiguration can cause unexisting symbols
            {
                this.symbol.RateUpdated += UpdateDeviationPrice;
            }
        }

        public OrderModel Order { get; private set; }
        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }
        public decimal? Price => Order.Price;

        public RateDirectionTracker CurrentPrice => Order.OrderType != Algo.Domain.OrderInfo.Types.Type.Position ?
                                                    Order.Side == Algo.Domain.OrderInfo.Types.Side.Buy ? symbol?.AskTracker : symbol?.BidTracker :
                                                    Order.Side == Algo.Domain.OrderInfo.Types.Side.Buy ? symbol?.BidTracker : symbol?.AskTracker;

        public decimal? DeviationPrice => Order.Side == Algo.Domain.OrderInfo.Types.Side.Buy ? (decimal?)CurrentPrice?.Rate - Price : Price - (decimal?)CurrentPrice?.Rate;


        public string SortedNumber { get; }

        public void Dispose()
        {
        }

        private void UpdateDeviationPrice(SymbolModel model)
        {
            NotifyOfPropertyChange(nameof(DeviationPrice));
        }

        private string GetSortedNumber(OrderModel position) => $"{position.Modified?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{position.Id}";
    }
}
