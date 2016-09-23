using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class PositionViewModel : PropertyChangedBase, IDisposable
    {
        private SymbolObserver _symbolObserver = new SymbolObserver();

        public PositionViewModel(PositionModel position, SymbolModel symbol = null)
        {
            Position = position;
            _symbolObserver.ObservableSymbol = symbol;
            NotifyOfPropertyChange(nameof(CurrentPrice));

            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;
        }

        public int PriceDigits { get; private set; }
        public int ProfitDigits { get; private set; }
        public PositionModel Position { get; private set; }

        public RateDirectionTracker CurrentPrice => Position.Side == TradeRecordSide.Buy ? _symbolObserver?.Bid : _symbolObserver?.Ask;

        public void Dispose()
        {
            _symbolObserver?.Dispose();
        }
    }
}
