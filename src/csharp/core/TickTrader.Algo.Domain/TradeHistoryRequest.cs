using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum HistoryRequestOptions
    {
        NoOptions = 0x0,
        SkipCanceled = 0x1,
        Backwards = 0x2,
        SkipFailed = 0x4,
    }

    public partial class TradeHistoryRequest
    {
        public UtcTicks? From
        {
            get => FromTicks.ToUtcTicks();
            set => FromTicks = value.ToInt64();
        }

        public UtcTicks? To
        {
            get => ToTicks.ToUtcTicks();
            set => ToTicks = value.ToInt64();
        }

        public HistoryRequestOptions Options
        {
            get => (HistoryRequestOptions)OptionsBitmask;
            set => OptionsBitmask = (int)value;
        }
    }

    public partial class TriggerHistoryRequest
    {
        public UtcTicks? From
        {
            get => FromTicks.ToUtcTicks();
            set => FromTicks = value.ToInt64();
        }

        public UtcTicks? To
        {
            get => ToTicks.ToUtcTicks();
            set => ToTicks = value.ToInt64();
        }

        public HistoryRequestOptions Options
        {
            get => (HistoryRequestOptions)OptionsBitmask;
            set => OptionsBitmask = (int)value;
        }
    }
}
