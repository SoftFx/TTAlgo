using System;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    public static class ChartExtensions
    {
        public static ChartPeriods Convert(this TimeFrames timeFrame)
        {
            switch (timeFrame)
            {
                case TimeFrames.MN: return ChartPeriods.MN1;
                case TimeFrames.D: return ChartPeriods.D1;
                case TimeFrames.W: return ChartPeriods.W1;
                case TimeFrames.H4: return ChartPeriods.H4;
                case TimeFrames.H1: return ChartPeriods.H1;
                case TimeFrames.M30: return ChartPeriods.M30;
                case TimeFrames.M15: return ChartPeriods.M15;
                case TimeFrames.M5: return ChartPeriods.M5;
                case TimeFrames.M1: return ChartPeriods.M1;
                case TimeFrames.S10: return ChartPeriods.S10;
                case TimeFrames.S1: return ChartPeriods.S1;
                case TimeFrames.Ticks: return ChartPeriods.Ticks;
                default:
                    throw new ArgumentException("Can't convert provided TimeFrame to ChartPeriod");
            }
        }


        public static TimeFrames Convert(this ChartPeriods timeFrame)
        {
            switch (timeFrame)
            {
                case ChartPeriods.MN1: return TimeFrames.MN;
                case ChartPeriods.D1: return TimeFrames.D;
                case ChartPeriods.W1: return TimeFrames.W;
                case ChartPeriods.H4: return TimeFrames.H4;
                case ChartPeriods.H1: return TimeFrames.H1;
                case ChartPeriods.M30: return TimeFrames.M30;
                case ChartPeriods.M15: return TimeFrames.M15;
                case ChartPeriods.M5: return TimeFrames.M5;
                case ChartPeriods.M1: return TimeFrames.M1;
                case ChartPeriods.S10: return TimeFrames.S10;
                case ChartPeriods.S1: return TimeFrames.S1;
                case ChartPeriods.Ticks: return TimeFrames.Ticks;
                default:
                    throw new ArgumentException("Can't convert provided ChartPeriod to TimeFrame");
            }
        }
    }
}
