﻿using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public static class SymbolFactory
    {
        private static readonly SymbolInfo _prototype = new()
        {
            TradeAllowed = true,
            Digits = 5,
            Security = string.Empty,
            Description = "test",
            LotSize = 100000,
            MinTradeVolume = 0.01,
            MaxTradeVolume = 1000,
            TradeVolumeStep = 0.01,

            Slippage = new SlippageInfo
            {
                DefaultValue = 0.08,
                Type = SlippageInfo.Types.Type.Percent,
            },

            Commission = new CommissonInfo
            {
                Commission = 0.0018,
                LimitsCommission = 0.0018,
                ValueType = CommissonInfo.Types.ValueType.Percentage,
            },

            Margin = new MarginInfo
            {
                Mode = MarginInfo.Types.CalculationMode.Forex,
                Factor = 1,
                Hedged = 0.5,
                StopOrderReduction = 1,
                HiddenLimitOrderReduction = 1,
            },

            Swap = new SwapInfo
            {
                Enabled = true,
                Type = SwapInfo.Types.Type.Points,
                SizeLong = -4.1,
                SizeShort = 0.36,
                TripleSwapDay = 3,
            }
        };


        public static ISymbolInfoWithRate BuildSymbol(string baseCurr, string counterCurr)
        {
            var symbol = _prototype.Clone();

            symbol.Name = $"{baseCurr}{counterCurr}";
            symbol.BaseCurrency = baseCurr;
            symbol.CounterCurrency = counterCurr;

            return symbol;
        }
    }
}
