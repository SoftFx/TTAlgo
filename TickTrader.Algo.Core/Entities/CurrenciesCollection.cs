using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class CurrenciesCollection : IEnumerable<CurrencyEntity>
    {
        private CurrencyFixture fixture = new CurrencyFixture();

        internal CurrencyList CurrencyListImp => fixture;

        public void Add(CurrencyEntity currency)
        {
            fixture.Add(currency);
        }

        public void Init(IEnumerable<CurrencyEntity> currencies)
        {
            fixture.Clear();

            if (currencies != null)
            {
                foreach (var currency in currencies)
                    fixture.Add(currency);
            }
        }

        public IEnumerator<CurrencyEntity> GetEnumerator()
        {
            return fixture.GetValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fixture.GetValues().GetEnumerator();
        }

        private class CurrencyFixture : CurrencyList
        {
            private Dictionary<string, CurrencyEntity> currencies = new Dictionary<string, CurrencyEntity>();

            public Currency this[string currencyCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(currencyCode))
                        return Null.Currency;

                    CurrencyEntity currency;
                    if (!currencies.TryGetValue(currencyCode, out currency))
                        return new NullCurrency(currencyCode);
                    return currency;
                }
            }

            public void Add(CurrencyEntity currency)
            {
                currencies.Add(currency.Name, currency);
            }

            public void Clear()
            {
                currencies.Clear();
            }

            public IEnumerable<CurrencyEntity> GetValues()
            {
                return currencies.Values;
            }

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