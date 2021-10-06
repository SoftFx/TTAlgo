using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum HistoryRequestOptions
    {
        NoOptions = 0x0,
        SkipCanceled = 0x1,
        Backwards = 0x2,
    }

    public partial class TradeHistoryRequest
    {
        public HistoryRequestOptions Options
        {
            get => (HistoryRequestOptions)OptionsBitmask;
            set => OptionsBitmask = (int)value;
        }
    }

    public partial class TriggerHistoryRequest
    {
        public HistoryRequestOptions Options
        {
            get => (HistoryRequestOptions)OptionsBitmask;
            set => OptionsBitmask = (int)value;
        }
    }
}
