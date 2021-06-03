namespace TickTrader.Algo.Domain
{
    public static class DomainEnumExtensions
    {
        public static bool IsTick(this Feed.Types.Timeframe timeframe)
        {
            return timeframe == Feed.Types.Timeframe.Ticks || timeframe == Feed.Types.Timeframe.TicksLevel2;
        }
    }
}
