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
        private AssetEntity _entity;
        private double _margin;

        public AssetAccessor(AssetEntity entity, Dictionary<string, Currency> currencies)
        {
            _entity = entity;
            CurrencyInfo = currencies.ContainsKey(Currency) ? currencies[Currency] : new NullCurrency(Currency);
        }

        public Currency CurrencyInfo { get; }
        public string Currency => _entity.Currency;
        public double Volume => _entity.Volume;
        public double LockedVolume => _margin;
        public double FreeVolume => Volume - _margin;
        public bool IsNull => false;

        decimal IAssetModel.Amount => (decimal)Volume;
        decimal IAssetModel.FreeAmount => (decimal)FreeVolume;
        decimal IAssetModel.LockedAmount => (decimal)LockedVolume;
        decimal IAssetModel.Margin { get => (decimal)_margin; set => _margin = (double) value; }
    }
}
