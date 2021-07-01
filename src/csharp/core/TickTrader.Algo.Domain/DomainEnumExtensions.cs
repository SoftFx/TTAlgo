﻿namespace TickTrader.Algo.Domain
{
    public static class DomainEnumExtensions
    {
        public static bool IsTick(this Feed.Types.Timeframe timeframe)
        {
            return timeframe == Feed.Types.Timeframe.Ticks || timeframe == Feed.Types.Timeframe.TicksLevel2;
        }

        public static bool IsStop(this OrderInfo.Types.Type type)
        {
            return type == OrderInfo.Types.Type.Stop || type == OrderInfo.Types.Type.StopLimit;
        }

        public static bool IsBuy(this OrderInfo.Types.Side side) => side == OrderInfo.Types.Side.Buy;

        public static bool IsSell(this OrderInfo.Types.Side side) => side == OrderInfo.Types.Side.Sell;

        public static bool IsLeverageMode(this MarginInfo.Types.CalculationMode mode)
        {
            return mode == MarginInfo.Types.CalculationMode.Forex || mode == MarginInfo.Types.CalculationMode.CfdLeverage;
        }
    }
}
