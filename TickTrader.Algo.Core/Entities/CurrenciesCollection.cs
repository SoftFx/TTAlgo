using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class CurrenciesCollection
    {
        private CurrencyFixture fixture = new CurrencyFixture();

        internal CurrencyList CurrencyListImp => fixture;

        public void Add(CurrencyEntity currency)
        {
            fixture.InnerCollection.Add(currency.Name, currency);
        }

        public void Init(IEnumerable<CurrencyEntity> currencies)
        {
            fixture.InnerCollection.Clear();

            if (currencies != null)
            {
                foreach (var currency in currencies)
                    fixture.InnerCollection.Add(currency.Name, currency);
            }
        }

        private class CurrencyFixture : CurrencyList
        {
            private Dictionary<string, Currency> currencies = new Dictionary<string, Currency>();

            public Currency this[string currencyCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(currencyCode))
                        return Null.Currency;

                    Currency currency;
                    if (!currencies.TryGetValue(currencyCode, out currency))
                        return new NullCurrency(currencyCode);
                    return currency;
                }
            }

            public Dictionary<string, Currency> InnerCollection { get { return currencies; } }

            public IEnumerator<Currency> GetEnumerator()
            {
                return currencies.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return currencies.Values.GetEnumerator();
            }
        }
    }
}