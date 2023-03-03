using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum DrawableVisibility : uint
    {
        NoTimeframes = 0,
        TimeframeS1 = 0x1,
        TimeframeS10 = 0x2,
        TimeframeM1 = 0x4,
        TimeframeM5 = 0x8,
        TimeframeM15 = 0x10,
        TimeframeM30 = 0x20,
        TimeframeH1 = 0x40,
        TimeframeH4 = 0x80,
        TimeframeD1 = 0x100,
        TimeframeW1 = 0x200,
        TimeframeMN1 = 0x400,

        AllTimeframes = 0x7ff,
    }
}
