using System;

namespace TickTrader.Algo.Domain
{
    public partial class AssetInfo : IAssetInfo
    {
        private double _margin;

        public AssetInfo(double balance, string currency)
        {
            Balance = balance;
            Currency = currency;
        }

        public double Margin
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

        public event Action MarginUpdate;

        double IAssetInfo.Amount => Balance;

        double IAssetInfo.FreeAmount => Balance - Margin;

        double IAssetInfo.LockedAmount => Margin;
    }

    public interface IAssetInfo
    {
        string Currency { get; }
        double Amount { get; }
        double FreeAmount { get; }
        double LockedAmount { get; }
        double Margin { get; set; }

        event Action MarginUpdate;
    }
}
