using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class MockClient : IMarketDataProvider, IFeedSubscription, IQuoteDistributorSource
    {
        private VarDictionary<string, SymbolModel> _symbols = new VarDictionary<string, SymbolModel>();
        private VarDictionary<string, CurrencyEntity> _currencies = new VarDictionary<string, CurrencyEntity>();

        public MockClient()
        {
            Acc = new AccountModel(_currencies, _symbols);
            Distributor = new QuoteDistributor(this);
        }

        public AccountModel Acc { get; }

        public void Init(AccountEntity accInfo, IEnumerable<SymbolEntity> symbols, IEnumerable<CurrencyEntity> currencies)
        {
            _symbols.Clear();
            _currencies.Clear();

            foreach (var c in currencies)
                _currencies.Add(c.Name, c);

            foreach (var s in symbols)
                _symbols.Add(s.Name, new SymbolModel(s, _currencies));

            Acc.Init(accInfo, new OrderEntity[0], new PositionEntity[0], new AssetEntity[0]);
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
            NewQuote?.Invoke(quote);
            Distributor.UpdateRate(quote);
        }

        #region IMarketDataProvider
        public IVarSet<string, SymbolModel> Symbols => _symbols;
        public IVarSet<string, CurrencyEntity> Currencies => _currencies;
        public QuoteDistributor Distributor { get; }
        #endregion

        #region IFeedSubscription
        public event Action<QuoteEntity> NewQuote;
        void IFeedSubscription.Add(string symbol, int depth) => throw new NotImplementedException();
        void IFeedSubscription.Remove(string symbol) => throw new NotImplementedException();
        void IDisposable.Dispose() { }
        #endregion

        #region IQuoteDistributorSource
        void IQuoteDistributorSource.ModifySubscription(string symbol, int depth) { }
        #endregion
    }
}
