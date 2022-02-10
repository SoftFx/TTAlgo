using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public sealed class SymbolInfoStorage
    {
        private readonly List<(string, string)> _symbolCurr = new()
        {
            ("EUR", "USD"),
            ("EUR", "AUD"),
            ("AUD", "USD"),
            ("AUD", "CAD"),
            ("USD", "JPY"),
            ("BTC", "USD"),
        };

        public Dictionary<string, SymbolInfo> Symbols;

        public Dictionary<string, double> Bid, Ask;


        public SymbolInfoStorage()
        {
            Symbols = _symbolCurr.ToDictionary(k => $"{k.Item1}{k.Item2}", v => SymbolFactory.BuildSymbol(v.Item1, v.Item2));

            var smb = Symbols["EURUSD"];
            smb.Slippage.DefaultValue = 0.06;
            smb.Margin.StopOrderReduction = 0.6;
            smb.Margin.HiddenLimitOrderReduction = 0.8;

            smb = Symbols["EURAUD"];
            smb.Slippage.DefaultValue = 0;
            smb.Swap.SizeLong = -4.51;
            smb.Swap.SizeShort = -1.54;

            smb = Symbols["AUDUSD"];
            smb.Swap.SizeLong = -0.98;
            smb.Swap.SizeShort = -0.44;

            AllSymbolsRateUpdate();
        }

        public void AllSymbolsRateUpdate()
        {
            foreach (var symbol in Symbols.Values)
                symbol.BuildNewQuote();

            Bid = Symbols.ToDictionary(k => k.Key, v => v.Value.Bid ?? double.NaN);
            Ask = Symbols.ToDictionary(k => k.Key, v => v.Value.Ask ?? double.NaN);
        }

        public void ResetAllRateUpdate()
        {
            foreach (var symbol in Symbols.Values)
                symbol.UpdateRate(null);

            Bid = null;
            Ask = null;
        }
    }
}
