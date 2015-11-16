using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SymbolDetailsViewModel : PropertyChangedBase
    {
        public SymbolDetailsViewModel(RateDirectionTracker buyRateTracker, RateDirectionTracker sellRateTracker)
        {
            Sell = sellRateTracker;
            Buy = buyRateTracker;
        }

        public RateDirectionTracker Sell { get; private set; }
        public RateDirectionTracker Buy { get; private set; }
    }
}
