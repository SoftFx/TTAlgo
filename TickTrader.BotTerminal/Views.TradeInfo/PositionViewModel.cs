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
    class PositionViewModel : PropertyChangedBase, IDisposable
    {
        public PositionViewModel(PositionModel position, SymbolModel symbol = null)
        {
            Symbol = symbol;
            Position = position;

            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;
        }

        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }
        public PositionModel Position { get; private set; }
        public SymbolModel Symbol { get; private set; }

        public RateDirectionTracker CurrentPrice => Position.Side == TradeRecordSide.Buy ? Symbol?.BidTracker : Symbol?.AskTracker;

        public void Dispose()
        {
        }
    }
}
