using Machinarium.Qnil;
using System.Collections.Generic;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class MockClient : IMarketDataProvider
    {
        private VarDictionary<string, SymbolInfo> _symbols = new VarDictionary<string, SymbolInfo>();
        private VarDictionary<string, CurrencyInfo> _currencies = new VarDictionary<string, CurrencyInfo>();

        public MockClient()
        {
            Acc = new AccountModel(_currencies, _symbols);
            Distributor = new QuoteDistributor();
        }

        public AccountModel Acc { get; }

        public void Init(Domain.AccountInfo accInfo, IEnumerable<ISymbolInfo> symbols, IEnumerable<CurrencyInfo> currencies)
        {
            _symbols.Clear();
            _currencies.Clear();

            foreach (var c in currencies)
                _currencies.Add(c.Name, c);

            foreach (var s in symbols)
                _symbols.Add(s.Name, new SymbolInfo(s));

            Acc.Init(accInfo, new Domain.OrderInfo[0], new Domain.PositionInfo[0], new Domain.AssetInfo[0]);
            Acc.StartCalculator(this);
        }

        public void Deinit()
        {
            Acc.Deinit();
        }

        public void Clear()
        {
            _symbols.Clear();
            _currencies.Clear();
            Acc.Clear();
        }

        public void OnRateUpdate(QuoteInfo quote)
        {
            _symbols.GetOrDefault(quote.Symbol)?.UpdateRate(quote);
            Distributor.UpdateRate(quote);
            //Acc?.Market?.UpdateRate(quote);
        }

        #region IMarketDataProvider
        public IVarSet<string, SymbolInfo> Symbols => _symbols;
        public IVarSet<string, CurrencyInfo> Currencies => _currencies;
        public QuoteDistributor Distributor { get; }
        #endregion
    }
}
