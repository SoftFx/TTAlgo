using System;

namespace TickTrader.Algo.Domain
{
    public static class TimeframeExtensions
    {
        public static DateTime AddTimeframe(this DateTime time, Feed.Types.Timeframe frame)
        {
            switch (frame)
            {
                case Feed.Types.Timeframe.S1:
                    return time.AddSeconds(1);

                case Feed.Types.Timeframe.S10:
                    return time.AddSeconds(10);

                case Feed.Types.Timeframe.M1:
                    return time.AddMinutes(1);

                case Feed.Types.Timeframe.M5:
                    return time.AddMinutes(5);

                case Feed.Types.Timeframe.M15:
                    return time.AddMinutes(15);

                case Feed.Types.Timeframe.M30:
                    return time.AddMinutes(30);

                case Feed.Types.Timeframe.H1:
                    return time.AddHours(1);

                case Feed.Types.Timeframe.H4:
                    return time.AddHours(4);

                case Feed.Types.Timeframe.D:
                    return time.AddDays(1);

                case Feed.Types.Timeframe.W:
                    return time.AddDays(7);

                case Feed.Types.Timeframe.MN:
                    return time.AddMonths(1);

                default:
                    return time;
            }
        }
    }
}
