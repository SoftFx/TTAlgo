using System;

namespace TickTrader.Algo.Domain
{
    public partial class AssetInfo : IAssetInfo
    {
        private decimal _margin;

        public AssetInfo(double balance, string currency)
        {
            Balance = balance;
            Currency = currency;
        }

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

        public event Action MarginUpdate;

        decimal IAssetInfo.Amount => (decimal)Balance;

        decimal IAssetInfo.FreeAmount => (decimal)Balance - Margin;

        decimal IAssetInfo.LockedAmount => Margin;
    }

    public interface IAssetInfo
    {
        string Currency { get; }
        decimal Amount { get; }
        decimal FreeAmount { get; }
        decimal LockedAmount { get; }
        decimal Margin { get; set; }

        event Action MarginUpdate;
    }
}
