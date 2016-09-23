using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class AssetViewModel
    {
        public AssetViewModel(AssetModel asset, CurrencyInfo info)
        {
            Asset = asset;
            CurrencyDigits = info?.Precision ?? 2;
        }

        public AssetModel Asset { get; private set; }
        public int CurrencyDigits { get; private set; }
    }
}
