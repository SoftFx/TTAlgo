using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal static class AlgoConverter
    {
        public static SymbolEntity Convert(SymbolInfo info)
        {
            return new SymbolEntity(info.Name)
            {
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinAmount = info.MinTradeVolume,
                MaxAmount = info.MaxTradeVolume,
                BaseCurrencyCode = info.Currency,
                CounterCurrencyCode = info.SettlementCurrency,
            };
        }
    }
}
