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
    class PositionViewModel : PropertyChangedBase, IDisposable
    {
        private static IndificationNumberGenerator _numberGenerator = new IndificationNumberGenerator();

        public PositionViewModel(PositionModel position)
        {
            Position = position;

            PriceDigits = position?.SymbolModel?.PriceDigits ?? 5;
            ProfitDigits = position?.SymbolModel?.QuoteCurrencyDigits ?? 2;
            SortedNumber = GetSortedNumber(position);
        }

        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }
        public PositionModel Position { get; private set; }
        public RateDirectionTracker CurrentPrice => Position.Side == OrderSide.Buy ? Position?.SymbolModel?.BidTracker : Position?.SymbolModel?.AskTracker;
        public string SortedNumber { get; }

        public void Dispose()
        {
        }

        private string GetSortedNumber(PositionModel position)
        {
            return $"{position.Modified?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{_numberGenerator.GetNumber(position.Modified.Value)}";
        }
    }
}
