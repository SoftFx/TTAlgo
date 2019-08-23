using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public class AssetAccessor : Api.Asset, BusinessLogic.IAssetModel
    {
        private decimal _margin;

        internal AssetAccessor(AssetEntity entity, Func<string, Currency> currencyInfoProvider)
        {
            Currency = entity.Currency;
            Volume = (decimal)entity.Volume;
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

        double Api.Asset.Volume => (double) Volume;
        double Api.Asset.FreeVolume => (double)FreeVolume;

        decimal IAssetModel.Amount => Volume;
        decimal IAssetModel.FreeAmount => Volume - _margin;
        decimal IAssetModel.LockedAmount => _margin;
        decimal IAssetModel.Margin { get => _margin; set => _margin = value; }
    }
}
