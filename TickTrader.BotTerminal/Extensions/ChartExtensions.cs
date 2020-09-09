using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public static class ChartExtensions
    {
        public static ChartPeriods Convert(this Feed.Types.Timeframe timeFrame)
        {
            switch (timeFrame)
            {
                case Feed.Types.Timeframe.MN: return ChartPeriods.MN1;
                case Feed.Types.Timeframe.D: return ChartPeriods.D1;
                case Feed.Types.Timeframe.W: return ChartPeriods.W1;
                case Feed.Types.Timeframe.H4: return ChartPeriods.H4;
                case Feed.Types.Timeframe.H1: return ChartPeriods.H1;
                case Feed.Types.Timeframe.M30: return ChartPeriods.M30;
                case Feed.Types.Timeframe.M15: return ChartPeriods.M15;
                case Feed.Types.Timeframe.M5: return ChartPeriods.M5;
                case Feed.Types.Timeframe.M1: return ChartPeriods.M1;
                case Feed.Types.Timeframe.S10: return ChartPeriods.S10;
                case Feed.Types.Timeframe.S1: return ChartPeriods.S1;
                case Feed.Types.Timeframe.Ticks: return ChartPeriods.Ticks;
                default:
                    throw new ArgumentException("Can't convert provided TimeFrame to ChartPeriod");
            }
        }


        public static Feed.Types.Timeframe Convert(this ChartPeriods timeFrame)
        {
            switch (timeFrame)
            {
                case ChartPeriods.MN1: return Feed.Types.Timeframe.MN;
                case ChartPeriods.D1: return Feed.Types.Timeframe.D;
                case ChartPeriods.W1: return Feed.Types.Timeframe.W;
                case ChartPeriods.H4: return Feed.Types.Timeframe.H4;
                case ChartPeriods.H1: return Feed.Types.Timeframe.H1;
                case ChartPeriods.M30: return Feed.Types.Timeframe.M30;
                case ChartPeriods.M15: return Feed.Types.Timeframe.M15;
                case ChartPeriods.M5: return Feed.Types.Timeframe.M5;
                case ChartPeriods.M1: return Feed.Types.Timeframe.M1;
                case ChartPeriods.S10: return Feed.Types.Timeframe.S10;
                case ChartPeriods.S1: return Feed.Types.Timeframe.S1;
                case ChartPeriods.Ticks: return Feed.Types.Timeframe.Ticks;
                default:
                    throw new ArgumentException("Can't convert provided ChartPeriod to TimeFrame");
            }
        }
    }
}
