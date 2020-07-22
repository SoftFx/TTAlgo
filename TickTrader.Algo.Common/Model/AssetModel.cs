using System;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class AssetModel : IAssetModel2
    {
        private decimal _margin;

        public AssetModel(double balance, string currency)
        {
            Currency = currency;
            Amount = (decimal)balance;
        }

        public AssetModel(Domain.AssetInfo asset)
        {
            Currency = asset.Currency;
            Update(asset);
        }

        private void Update(Domain.AssetInfo asset)
        {
            Amount = (decimal)asset.Balance;
        }

        public string Currency { get; private set; }

        public decimal Amount { get; private set; }

        public decimal Margin
        {
            get => _margin;

            set
            {
                if (_margin == value)
                    return;

                _margin = value;

                MarginUpdate?.Invoke();
            }
        }

        public decimal FreeAmount => Amount - Margin;

        public decimal LockedAmount => Margin;

        public Action MarginUpdate;

        public Domain.AssetInfo GetInfo()
        {
            return new Domain.AssetInfo((double)Amount, Currency);
        }
    }
}
