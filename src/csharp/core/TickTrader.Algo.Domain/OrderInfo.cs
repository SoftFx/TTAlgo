using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum OrderOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        MarketWithSlippage = 2,
        HiddenIceberg = 4,
    }


    public partial class OrderInfo
    {
        public OrderOptions Options
        {
            get { return (OrderOptions)OptionsBitmask; }
            set { OptionsBitmask = (int)value; }
        }

        public bool ImmediateOrCancel => Options.HasFlag(OrderOptions.ImmediateOrCancel);

        public bool MarketWithSlippdage => Options.HasFlag(OrderOptions.MarketWithSlippage);

        public bool HiddenIceberg => Options.HasFlag(OrderOptions.HiddenIceberg);
    }

    public static class OrderInfoExtensions
    {
        public static bool IsHidden(this OrderInfo order)
        {
            var maxVisibleVolume = order.MaxVisibleAmount;
            return maxVisibleVolume.HasValue && Math.Abs(maxVisibleVolume.Value) < 1e-9;
        }
    }
}
