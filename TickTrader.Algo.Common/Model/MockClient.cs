using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;

namespace TickTrader.Algo.Common.Model
{
    public class MockClient : IMarketDataProvider
    {
        private VarDictionary<string, SymbolModel> _symbols = new VarDictionary<string, SymbolModel>();
        private VarDictionary<string, CurrencyEntity> _currencies = new VarDictionary<string, CurrencyEntity>();

        public MockClient()
        {
            Acc = new AccountModel(_currencies, _symbols);
            Distributor = new QuoteDistributor();
        }

        public AccountModel Acc { get; }

        public void Init(Domain.AccountInfo accInfo, IEnumerable<Domain.SymbolInfo> symbols, IEnumerable<CurrencyEntity> currencies)
        {
            _symbols.Clear();
            _currencies.Clear();

            foreach (var c in currencies)
                _currencies.Add(c.Name, c);

            foreach (var s in symbols)
                _symbols.Add(s.Name, new SymbolModel(s, _currencies));

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

        public void OnRateUpdate(QuoteEntity quote)
        {
            _symbols.GetOrDefault(quote.Symbol)?.OnNewTick(quote);
            Distributor.UpdateRate(quote);
            Acc?.Market?.UpdateRate(quote);
        }

        #region IMarketDataProvider
        public IVarSet<string, SymbolModel> Symbols => _symbols;
        public IVarSet<string, CurrencyEntity> Currencies => _currencies;
        public QuoteDistributor Distributor { get; }
        #endregion
    }
}
