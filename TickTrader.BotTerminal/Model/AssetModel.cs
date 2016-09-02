using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    class AssetModel : PropertyChangedBase
    {
        private string currency;
        private double balance;
        private double tradeAmount;

        public AssetModel(double balance, string currency)
        {
            this.currency = currency;
            this.balance = balance;
        }

        public AssetModel(AssetInfo asset)
        {
            Currency = asset.Currency;
            Update(asset);
        }

        private void Update(AssetInfo asset)
        {
            Balance = asset.Balance;
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

        public double Balance
        {
            get { return balance; }
            private set
            {
                if (balance != value)
                {
                    balance = value;
                    NotifyOfPropertyChange(nameof(Balance));
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

        public Algo.Core.AssetEntity ToAlgoAsset()
        {
            return new Algo.Core.AssetEntity()
            {
                CurrencyCode = currency,
                Volume = balance
            };
        }
    }
}
