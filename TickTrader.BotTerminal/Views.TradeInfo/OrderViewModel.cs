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
        private AccountModel _account;
        private SymbolModel _symbol;

        public OrderViewModel(OrderModel order, SymbolModel symbol, AccountModel account)
        {
            _symbol = symbol;
            _account = account;

            Order = order;
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = account.BalanceDigits;

            SortedNumber = GetSortedNumber(order);

            if (_symbol != null) // server misconfiguration can cause unexisting symbols
            {
                _symbol.RateUpdated += UpdateDeviationPrice;
            }
        }

        public OrderModel Order { get; private set; }
        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }

        public RateDirectionTracker CurrentPrice => Order.OrderType != OrderType.Position ?
                                                    Order.Side == OrderSide.Buy ? _symbol?.AskTracker : _symbol?.BidTracker :
                                                    Order.Side == OrderSide.Buy ? _symbol?.BidTracker : _symbol?.AskTracker;

        public decimal? DeviationPrice => Order.Side == OrderSide.Buy ? (decimal?)CurrentPrice?.Rate - Order?.Price : Order?.Price - (decimal?)CurrentPrice?.Rate;

        public string SortedNumber { get; }

        public void Dispose()
        {
            if (_symbol != null)
            {
                _symbol.RateUpdated -= UpdateDeviationPrice;
            }
        }

        private void UpdateDeviationPrice(SymbolModel model)
        {
            NotifyOfPropertyChange(nameof(DeviationPrice));
        }

        private string GetSortedNumber(OrderModel position) => $"{position.Modified?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{position.Id}";
    }
}
