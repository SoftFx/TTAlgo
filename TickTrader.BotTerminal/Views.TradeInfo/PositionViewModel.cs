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
        private PositionModel _position;
        private SymbolObserver _symbolObserver = new SymbolObserver();

        public PositionViewModel(PositionModel position)
        {
            Position = position;
        }
        public PositionViewModel(PositionModel position, SymbolModel symbol) : this(position)
        {
            _symbolObserver.ObservableSymbol = symbol;
            NotifyOfPropertyChange(nameof(CurrentPrice));
        }

        public PositionModel Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    NotifyOfPropertyChange(nameof(Position));
                }
            }
        }
        public RateDirectionTracker CurrentPrice => Position.Side == TradeRecordSide.Buy ? _symbolObserver?.Bid : _symbolObserver?.Ask;

        public void Dispose()
        {
            _symbolObserver?.Dispose();
        }
    }
}
