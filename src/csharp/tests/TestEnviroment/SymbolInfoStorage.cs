using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public sealed class SymbolInfoStorage
    {
        private readonly TestRateGenerator _rateGenerator;

        private readonly List<(string, string)> _symbolCurr = new()
        {
            ("EUR", "USD"),
            ("EUR", "AUD"),
            ("AUD", "USD"),
            ("AUD", "CAD"),
            ("USD", "JPY"),
            ("BTC", "USD"),
        };

        public static SymbolInfoStorage Instance { get; } = new SymbolInfoStorage();

        public Dictionary<string, SymbolInfo> Symbols;

        public Dictionary<string, double> Bid, Ask;


        private SymbolInfoStorage()
        {
            _rateGenerator = TestRateGenerator.Instance;

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
        }

        public void AllSymbolsRateUpdate()
        {
            foreach (var symbol in Symbols.Values)
                symbol.UpdateRate(_rateGenerator.BuildNewQuote(symbol.Name));

            Bid = Symbols.ToDictionary(k => k.Key, v => v.Value.Bid);
            Ask = Symbols.ToDictionary(k => k.Key, v => v.Value.Ask);
        }
    }
}
