using LiveChartsCore.Kernel;
using System;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed record EventPoint
    {
        internal double? Price { get; init; }

        internal double Time { get; init; }

        internal string ToolTip { get; init; }


        internal static void MapPoint(EventPoint point, ChartPoint chartPoint)
        {
            chartPoint.PrimaryValue = (double)point?.Price;
            chartPoint.SecondaryValue = (double)point?.Time;
        }
    }


    internal sealed record IndicatorPoint
    {
        internal double Value { get; init; }

        internal DateTime Time { get; init; }


        internal static void MapPoint(IndicatorPoint point, ChartPoint chartPoint)
        {
            chartPoint.PrimaryValue = (double)point?.Value;
            chartPoint.SecondaryValue = (double)point?.Time.Ticks;
        }
    }
}