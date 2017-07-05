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
        private double _margin;

        internal AssetAccessor(AssetEntity entity, Dictionary<string, Currency> currencies)
        {
            Currency = entity.Currency;
            Volume = entity.Volume;
            CurrencyInfo = currencies.ContainsKey(Currency) ? currencies[Currency] : new NullCurrency(Currency);
        }

        internal bool Update(double newVol)
        {
            if (Volume != newVol)
            {
                Volume = newVol;
                return true;
            }
            return false;
        }

        public Currency CurrencyInfo { get; }
        public string Currency { get; private set; }
        public double Volume { get; private set; }
        public double LockedVolume => _margin;
        public double FreeVolume => Volume - _margin;
        public bool IsNull => false;

        decimal IAssetModel.Amount => (decimal)Volume;
        decimal IAssetModel.FreeAmount => (decimal)FreeVolume;
        decimal IAssetModel.LockedAmount => (decimal)LockedVolume;
        decimal IAssetModel.Margin { get => (decimal)_margin; set => _margin = (double) value; }
    }
}
