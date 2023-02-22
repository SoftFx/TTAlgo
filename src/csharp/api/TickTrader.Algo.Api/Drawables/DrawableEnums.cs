using System;

namespace TickTrader.Algo.Api
{
    public enum DrawableSymbolAnchor
    {
        Top = 0,
        Bottom = 1
    }

    public enum DrawableControlZeroPosition
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3,
    }

    [Flags]
    public enum DrawableObjectVisibility
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

    public enum DrawablePositionMode
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        CenterLeft = 3,
        Center = 4,
        CenterRight = 5,
        BottomLeft = 6,
        BottomCenter = 7,
        BottomRight = 8,
    }

    public enum DrawableGannDirection
    {
        UpTrend = 0,
        DownTrend = 1,
    }
}
