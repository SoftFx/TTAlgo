using TickTrader.Algo.Core;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.UTest.Utility
{
    public static class BarInputHelper
    {
        public static void MapBars(IndicatorBuilder builder, string symbol)
        {
            builder.MapBarInput("Bars", symbol);
        }

        public static void MapPrice(IndicatorBuilder builder, string symbol, AppliedPrice.Target targetPrice,
            string inputName = "Price")
        {
            switch (targetPrice)
            {
                case AppliedPrice.Target.Close:
                    builder.MapBarInput(inputName, symbol, entity => entity.Close);
                    break;
                case AppliedPrice.Target.Open:
                    builder.MapBarInput(inputName, symbol, entity => entity.Open);
                    break;
                case AppliedPrice.Target.High:
                    builder.MapBarInput(inputName, symbol, entity => entity.High);
                    break;
                case AppliedPrice.Target.Low:
                    builder.MapBarInput(inputName, symbol, entity => entity.Low);
                    break;
                case AppliedPrice.Target.Median:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low)/2);
                    break;
                case AppliedPrice.Target.Typical:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low + entity.Close)/3);
                    break;
                case AppliedPrice.Target.Weighted:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low + 2*entity.Close)/4);
                    break;
                case AppliedPrice.Target.Move:
                    builder.MapBarInput(inputName, symbol, entity => entity.Close - entity.Open);
                    break;
                case AppliedPrice.Target.Range:
                    builder.MapBarInput(inputName, symbol, entity => entity.High - entity.Low);
                    break;
            }
        }
    }
}
