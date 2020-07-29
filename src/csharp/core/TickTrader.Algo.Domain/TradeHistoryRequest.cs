using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum TradeHistoryRequestOptions
    {
        NoOptions = 0x0,
        SkipCanceled = 0x1,
        Backwards = 0x2,
    }

    public partial class TradeHistoryRequest
    {
        public TradeHistoryRequestOptions Options
        {
            get => (TradeHistoryRequestOptions)OptionsBitmask;
            set => OptionsBitmask = (int)value;
        }
    }
}
