using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AssetEntity : Api.Asset
    {
        public AssetEntity(double balance, string currency, Dictionary<string, Currency> currencies)
        {
            this.Currency = currency;
            this.Volume = balance;
            this.CurrencyInfo = currencies.ContainsKey(currency) ? currencies[currency] : new NullCurrency(currency);
        }

        public AssetEntity(double balance, string currency, Currency currencyInfo)
        {
            this.Currency = currency;
            this.Volume = balance;
            this.CurrencyInfo = currencyInfo;
        }

        public string Currency { get; set; }
        public Currency CurrencyInfo { get; }
        public double Volume { get; set; }
    }
}
