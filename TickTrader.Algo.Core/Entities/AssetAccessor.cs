using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;
using TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public interface IAssetModel2
    {
        string Currency { get; }
        decimal Amount { get; }
        decimal FreeAmount { get; }
        decimal LockedAmount { get; }
        decimal Margin { get; set; }
    }

    public class AssetAccessor : Api.Asset, IAssetModel2
    {
        private decimal _margin;

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

        double Api.Asset.Volume => (double) Volume;
        double Api.Asset.FreeVolume => (double)FreeVolume;

        decimal IAssetModel2.Amount => Volume;
        decimal IAssetModel2.FreeAmount => Volume - _margin;
        decimal IAssetModel2.LockedAmount => _margin;
        decimal IAssetModel2.Margin { get => _margin; set => _margin = value; }
    }
}
