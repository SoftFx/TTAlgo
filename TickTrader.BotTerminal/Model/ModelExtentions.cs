using Machinarium.Var;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal static class ModelExtentions
    {
        public static bool IsTicks(this Feed.Types.Timeframe timeFrame)
        {
            return timeFrame == Feed.Types.Timeframe.Ticks || timeFrame == Feed.Types.Timeframe.TicksLevel2;
        }

        public static BoolVar IsTicks(this Var<Feed.Types.Timeframe> timeFrame)
        {
            return timeFrame.Check(IsTicks);
        }
    }
}
