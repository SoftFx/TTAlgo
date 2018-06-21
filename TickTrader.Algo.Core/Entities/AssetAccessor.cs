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
        private decimal _volume;

        internal AssetAccessor(AssetEntity entity, Func<string, Currency> currencyInfoProvider)
        {
            Currency = entity.Currency;
            _volume = (decimal)entity.Volume;
            CurrencyInfo = currencyInfoProvider(Currency) ?? new NullCurrency(Currency);
        }

        internal bool Update(decimal newVol)
        {
            if (_volume != newVol)
            {
                _volume = newVol;
                return true;
            }
            return false;
        }

        internal void IncreaseBy(decimal amount)
        {
            _volume += amount;
        }

        public Currency CurrencyInfo { get; }
        public string Currency { get; private set; }
        public double Volume => (double)_volume;
        public double LockedVolume => (double)_margin;
        public double FreeVolume => Volume - LockedVolume;
        public bool IsNull => false;
        public bool IsEmpty => _volume == 0;

        decimal IAssetModel.Amount => _volume;
        decimal IAssetModel.FreeAmount => _volume - _margin;
        decimal IAssetModel.LockedAmount => _margin;
        decimal IAssetModel.Margin { get => _margin; set => _margin = value; }
    }
}
