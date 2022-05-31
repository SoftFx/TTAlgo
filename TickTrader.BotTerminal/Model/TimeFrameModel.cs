using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public static class TimeFrameModel
    {
        public static readonly Feed.Types.Timeframe[] AllTimeFrames = new Feed.Types.Timeframe[]
        {
            Feed.Types.Timeframe.MN,
            Feed.Types.Timeframe.W,
            Feed.Types.Timeframe.D,
            Feed.Types.Timeframe.H4,
            Feed.Types.Timeframe.H1,
            Feed.Types.Timeframe.M30,
            Feed.Types.Timeframe.M15,
            Feed.Types.Timeframe.M5,
            Feed.Types.Timeframe.M1,
            Feed.Types.Timeframe.S10,
            Feed.Types.Timeframe.S1,
            Feed.Types.Timeframe.Ticks,
            Feed.Types.Timeframe.TicksLevel2
        };

        public static readonly Feed.Types.Timeframe[] BarTimeFrames = new Feed.Types.Timeframe[]
        {
            Feed.Types.Timeframe.MN,
            Feed.Types.Timeframe.W,
            Feed.Types.Timeframe.D,
            Feed.Types.Timeframe.H4,
            Feed.Types.Timeframe.H1,
            Feed.Types.Timeframe.M30,
            Feed.Types.Timeframe.M15,
            Feed.Types.Timeframe.M5,
            Feed.Types.Timeframe.M1,
            Feed.Types.Timeframe.S10,
            Feed.Types.Timeframe.S1,
        };

        public static TimeSpan ToTimespan(this Feed.Types.Timeframe frame)
        {
            return frame switch
            {
                Feed.Types.Timeframe.S1 => new TimeSpan(0, 0, 1),
                Feed.Types.Timeframe.S10 => new TimeSpan(0, 0, 10),
                Feed.Types.Timeframe.M1 => new TimeSpan(0, 1, 0),
                Feed.Types.Timeframe.M5 => new TimeSpan(0, 5, 0),
                Feed.Types.Timeframe.M15 => new TimeSpan(0, 15, 0),
                Feed.Types.Timeframe.M30 => new TimeSpan(0, 30, 0),
                Feed.Types.Timeframe.H1 => new TimeSpan(1, 0, 0),
                Feed.Types.Timeframe.H4 => new TimeSpan(4, 0, 0),
                Feed.Types.Timeframe.D => new TimeSpan(1, 0, 0, 0, 0),
                Feed.Types.Timeframe.W => new TimeSpan(7, 0, 0, 0, 0),
                Feed.Types.Timeframe.MN => new TimeSpan(31, 0, 0, 0, 0),
                Feed.Types.Timeframe.Ticks or Feed.Types.Timeframe.TicksLevel2 or Feed.Types.Timeframe.TicksVwap => new TimeSpan(1L),
            };
        }
    }
}
