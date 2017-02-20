using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class AssetModel : ObservableObject, TickTrader.BusinessLogic.IAssetModel
    {
        private string currency;
        private decimal amount;
        private double tradeAmount;
        private decimal margin;

        public AssetModel(double balance, string currency)
        {
            this.currency = currency;
            this.amount = (decimal)balance;
        }

        public AssetModel(AssetInfo asset)
        {
            Currency = asset.Currency;
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
            return new Algo.Core.AssetEntity()
            {
                Currency = currency,
                Volume = (double)amount
            };
        }
    }
}
