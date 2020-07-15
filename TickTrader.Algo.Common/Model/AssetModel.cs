using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class AssetModel : ObservableObject, IAssetModel2
    {
        private string currency;
        private decimal amount;
        private double tradeAmount;
        private decimal margin;
        private Currency currencyInfo;

        public AssetModel(double balance, string currency, IReadOnlyDictionary<string, CurrencyEntity> currencies)
        {
            this.currency = currency;
            this.amount = (decimal)balance;
            currencyInfo = currencies.GetOrDefault(currency) ?? Null.Currency;
        }

        public AssetModel(Domain.AssetInfo asset, IReadOnlyDictionary<string, CurrencyEntity> currencies)
        {
            Currency = asset.Currency;
            currencyInfo = currencies.GetOrDefault(currency) ?? Null.Currency;
            Update(asset);
        }

        private void Update(Domain.AssetInfo asset)
        {
            Amount = (decimal)asset.Balance;
        }

        public string Currency
        {
            get { return currency; }
            private set
            {
                if (currency != value)
                {
                    currency = value;
                    NotifyOfPropertyChange(nameof(Currency));
                }
            }
        }

        public decimal Amount
        {
            get { return amount; }
            private set
            {
                if (amount != value)
                {
                    amount = value;
                    NotifyOfPropertyChange(nameof(Amount));
                }
            }
        }

        public decimal Margin
        {
            get { return margin; }
            set
            {
                if (margin != value)
                {
                    margin = value;
                    NotifyOfPropertyChange(nameof(Margin));
                    NotifyOfPropertyChange(nameof(FreeAmount));
                    NotifyOfPropertyChange(nameof(LockedAmount));
                }
            }
        }

        public decimal FreeAmount => Amount - Margin;
        public decimal LockedAmount => Margin;

        public Domain.AssetInfo GetInfo()
        {
            return new Domain.AssetInfo((double)amount, currency);
        }

        public AssetEntity GetEntity()
        {
            return new AssetEntity((double)amount, currency);
        }
    }
}
