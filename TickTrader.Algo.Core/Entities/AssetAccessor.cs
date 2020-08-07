using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class AssetAccessor : Api.Asset
    {
        private readonly CurrenciesCollection _currencies;

        public AssetInfo Info { get; private set; }

        internal AssetAccessor(AssetInfo info, CurrenciesCollection currencies)
        {
            _currencies = currencies;

            Info = info;
        }

        internal bool Update(AssetInfo info)
        {
            if (!Info.Balance.E(info.Balance))
            {
                Info = info;
                return true;
            }
            return false;
        }

        internal void IncreaseBy(decimal amount)
        {
            Info.Balance += (double)amount;
        }

        double Asset.Volume => Info.Balance;

        double Asset.FreeVolume => Info.Balance - (double)Info.Margin;

        string Asset.Currency => Info.Currency;

        Currency Asset.CurrencyInfo => _currencies.GetOrNull(Info.Currency) as Currency ?? new NullCurrency(Info.Currency);

        double Asset.LockedVolume => (double)Info.Margin;

        bool Asset.IsNull => false;
    }
}
