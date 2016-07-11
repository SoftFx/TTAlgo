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
            SellTracker = sellRateTracker;
            BuyTracker = buyRateTracker;
        }

        public RateDirectionTracker SellTracker { get; private set; }
        public RateDirectionTracker BuyTracker { get; private set; }

        public System.Action OnSellClick { get; set; }
        public System.Action OnBuyClick { get; set; }

        public void Sell()
        {
            OnSellClick();
        }

        public void Buy()
        {
            OnBuyClick();
        }
    }
}
