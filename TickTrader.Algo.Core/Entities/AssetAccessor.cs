using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class AssetAccessor : Api.Asset, IAssetInfo
    {
        private decimal _margin;

        public event Action MarginUpdate;

        internal AssetAccessor(AssetInfo info, Func<string, Currency> currencyInfoProvider)
        {
            Currency = info.Currency;
            Volume = (decimal)info.Balance;
            CurrencyInfo = currencyInfoProvider(Currency) ?? new NullCurrency(Currency);
        }

        internal bool Update(decimal newVol)
        {
            if (Volume != newVol)
            {
                Volume = newVol;
                return true;
            }
            return false;
        }

        internal void IncreaseBy(decimal amount)
        {
            Volume += amount;
        }

        public Currency CurrencyInfo { get; }
        public string Currency { get; private set; }
        public decimal Volume { get; private set; }
        public double LockedVolume => (double)_margin;
        public decimal FreeVolume => Volume - _margin;
        public bool IsNull => false;
        public bool IsEmpty => Volume == 0;

        double Api.Asset.Volume => (double)Volume;
        double Api.Asset.FreeVolume => (double)FreeVolume;

        decimal IAssetInfo.Amount => Volume;
        decimal IAssetInfo.FreeAmount => Volume - _margin;
        decimal IAssetInfo.LockedAmount => _margin;
        decimal IAssetInfo.Margin
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
    }
}
