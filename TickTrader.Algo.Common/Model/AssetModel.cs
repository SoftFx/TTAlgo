using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public class AssetModel : ObservableObject, TickTrader.BusinessLogic.IAssetModel
    {
        private string currency;
        private decimal amount;
        private double tradeAmount;
        private decimal margin;
        private Algo.Api.Currency currencyInfo;

        public AssetModel(double balance, string currency, IDictionary<string, CurrencyInfo> currencies)
        {
            this.currency = currency;
            this.amount = (decimal)balance;
            currencyInfo = currencies.ContainsKey(currency) ? (Currency)FdkToAlgo.Convert(currencies[currency]) : new Algo.Core.NullCurrency(currency);
        }

        public AssetModel(AssetInfo asset, IDictionary<string, CurrencyInfo> currencies)
        {
            Currency = asset.Currency;
            currencyInfo = currencies.ContainsKey(currency) ? (Currency)FdkToAlgo.Convert(currencies[currency]) : new Algo.Core.NullCurrency(currency);
            Update(asset);
        }

        private void Update(AssetInfo asset)
        {
            Amount = (decimal)asset.Balance;
            TradeAmount = asset.TradeAmount;
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

        public double TradeAmount
        {
            get { return tradeAmount; }
            private set
            {
                if (tradeAmount != value)
                {
                    tradeAmount = value;
                    NotifyOfPropertyChange(nameof(TradeAmount));
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

        public Algo.Core.AssetEntity ToAlgoAsset()
        {
            return new Algo.Core.AssetEntity((double)amount, currency);
        }
    }
}
