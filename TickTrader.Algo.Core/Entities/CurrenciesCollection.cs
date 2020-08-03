using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class CurrenciesCollection : Api.CurrencyList /*IEnumerable<CurrencyEntity>*/
    {
        private Dictionary<string, CurrencyAccessor> _currencies = new Dictionary<string, CurrencyAccessor>();

        //private CurrencyFixture _enities = new CurrencyFixture();

        public Currency this[string currencyCode]
        {
            get
            {
                if (string.IsNullOrEmpty(currencyCode))
                    return Null.Currency;

                CurrencyAccessor currency;
                if (!_currencies.TryGetValue(currencyCode, out currency))
                    return new NullCurrency(currencyCode);
                return currency;
            }
        }

        //internal CurrencyList CurrencyListImp => _enities;

        public IEnumerable<CurrencyAccessor> Values => _currencies.Values;

        //public void Add(CurrencyEntity currency)
        //{
        //    _enities.Add(currency);
        //}

        public void Init(IEnumerable<CurrencyInfo> currencies)
        {
            _currencies.Clear();

            if (currencies != null)
            {
                foreach (var currency in currencies)
                    _currencies.Add(currency.Name, new CurrencyAccessor(currency));
            }
        }

        public void Merge(IEnumerable<CurrencyInfo> currencies)
        {
            if (currencies != null)
            {
                InvalidateAll();

                foreach (var currency in currencies)
                {
                    var curr = _currencies.GetOrDefault(currency.Name);
                    if (curr == null)
                    {
                        _currencies.Add(currency.Name, new CurrencyAccessor(currency));
                    }
                    else
                    {
                        curr.Update(currency);
                    }
                }
            }
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

        private List<CurrencyAccessor> sortedCurrencies;

        private List<CurrencyAccessor> SortedCurrencies
        {
            get
            {
                if (sortedCurrencies == null)
                    sortedCurrencies = _currencies.Values.Where(c => !c.IsNull).OrderBy(c => c.Info.SortOrder).ToList();

                return sortedCurrencies;
            }
        }

        internal CurrencyAccessor GetOrDefault(string currency)
        {
            var currInfo = _currencies.GetOrDefault(currency);
            if (currInfo?.IsNull ?? true) // deleted currencies will be present after reconnect, but IsNull will be true
                return null;
            return currInfo;
        }

        public IEnumerator<Currency> GetEnumerator()
        {
            return SortedCurrencies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public IEnumerator<CurrencyEntity> GetEnumerator()
        //{
        //    return _enities.GetValues().GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return _enities.GetValues().GetEnumerator();
        //}

        //private class CurrencyFixture : CurrencyList
        //{
        //    private Dictionary<string, CurrencyEntity> currencies = new Dictionary<string, CurrencyEntity>();
        //    private List<CurrencyEntity> sortedCurrencies;

        //    public Currency this[string currencyCode]
        //    {
        //        get
        //        {
        //            if (string.IsNullOrEmpty(currencyCode))
        //                return Null.Currency;

        //            CurrencyEntity currency;
        //            if (!currencies.TryGetValue(currencyCode, out currency))
        //                return new NullCurrency(currencyCode);
        //            return currency;
        //        }
        //    }

        //    private List<CurrencyEntity> SortedCurrencies
        //    {
        //        get
        //        {
        //            if (sortedCurrencies == null)
        //                sortedCurrencies = currencies.Values.Where(c => !c.IsNull).OrderBy(c => c.SortOrder).ToList();

        //            return sortedCurrencies;
        //        }
        //    }

        //    public CurrencyEntity GetOrDefault(string name)
        //    {
        //        if (string.IsNullOrEmpty(name))
        //            return null;

        //        CurrencyEntity currInfo;
        //        currencies.TryGetValue(name, out currInfo);
        //        return currInfo;
        //    }

        //    public void Add(CurrencyEntity currency)
        //    {
        //        currencies.Add(currency.Name, currency);
        //        sortedCurrencies = null;
        //    }

        //    public void Clear()
        //    {
        //        currencies.Clear();
        //        sortedCurrencies = null;
        //    }

        //    public void InvalidateAll()
        //    {
        //        // currencies are not deleted from collection
        //        // deleted currencies will have IsNull set to true
        //        foreach (var curr in sortedCurrencies)
        //        {
        //            curr.Update(null);
        //        }
        //        sortedCurrencies = null;
        //    }

        //    public IEnumerable<CurrencyEntity> GetValues()
        //    {
        //        return SortedCurrencies;
        //    }

        //    public IEnumerator<Currency> GetEnumerator()
        //    {
        //        return SortedCurrencies.GetEnumerator();
        //    }

        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        return SortedCurrencies.GetEnumerator();
        //    }
        //}
    }
}