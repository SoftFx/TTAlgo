using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account.FeedStorage
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

    }
}
