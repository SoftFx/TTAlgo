using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class AssetViewModel
    {
        public AssetViewModel(AssetModel asset, CurrencyEntity info)
        {
            Asset = asset;
            CurrencyDigits = info?.Precision ?? 2;
        }

        public AssetModel Asset { get; private set; }
        public int CurrencyDigits { get; private set; }
    }
}
