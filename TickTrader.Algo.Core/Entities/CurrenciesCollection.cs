using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public void Merge(IEnumerable<CurrencyEntity> currencies)
        {
            if (currencies != null)
            {
                fixture.InvalidateAll();

                foreach (var currency in currencies)
                {
                    var curr = fixture.GetOrDefault(currency.Name);
                    if (curr == null)
                    {
                        fixture.Add(currency);
                    }
                    else
                    {
                        curr.Update(currency.Info);
                    }
                }
            }
        }

        internal CurrencyEntity GetOrDefault(string currency)
        {
            var currInfo = fixture.GetOrDefault(currency);
            if (currInfo?.IsNull ?? true) // deleted currencies will be present after reconnect, but IsNull will be true
                return null;
            return currInfo;
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
            private List<CurrencyEntity> sortedCurrencies;

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

            private List<CurrencyEntity> SortedCurrencies
            {
                get
                {
                    if (sortedCurrencies == null)
                        sortedCurrencies = currencies.Values.Where(c => !c.IsNull).OrderBy(c => c.SortOrder).ToList();

                    return sortedCurrencies;
                }
            }

            public CurrencyEntity GetOrDefault(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                CurrencyEntity currInfo;
                currencies.TryGetValue(name, out currInfo);
                return currInfo;
            }

            public void Add(CurrencyEntity currency)
            {
                currencies.Add(currency.Name, currency);
                sortedCurrencies = null;
            }

            public void Clear()
            {
                currencies.Clear();
                sortedCurrencies = null;
            }

            public void InvalidateAll()
            {
                // currencies are not deleted from collection
                // deleted currencies will have IsNull set to true
                foreach (var curr in sortedCurrencies)
                {
                    curr.Update(null);
                }
                sortedCurrencies = null;
            }

            public IEnumerable<CurrencyEntity> GetValues()
            {
                return SortedCurrencies;
            }

            public IEnumerator<Currency> GetEnumerator()
            {
                return SortedCurrencies.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return SortedCurrencies.GetEnumerator();
            }
        }
    }
}