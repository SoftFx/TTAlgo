using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.Utility
{
    public static class BarInputHelper
    {
        public static void MapBars(IndicatorBuilder builder, string symbol, string inputName = "Bars")
        {
            builder.MapBarInput(inputName, symbol);
        }

        public static void MapPrice(IndicatorBuilder builder, string symbol, AppliedPrice targetPrice,
            string inputName = "Price")
        {
            switch (targetPrice)
            {
                case AppliedPrice.Close:
                    builder.MapBarInput(inputName, symbol, entity => entity.Close);
                    break;
                case AppliedPrice.Open:
                    builder.MapBarInput(inputName, symbol, entity => entity.Open);
                    break;
                case AppliedPrice.High:
                    builder.MapBarInput(inputName, symbol, entity => entity.High);
                    break;
                case AppliedPrice.Low:
                    builder.MapBarInput(inputName, symbol, entity => entity.Low);
                    break;
                case AppliedPrice.Median:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low)/2);
                    break;
                case AppliedPrice.Typical:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low + entity.Close)/3);
                    break;
                case AppliedPrice.Weighted:
                    builder.MapBarInput(inputName, symbol, entity => (entity.High + entity.Low + 2*entity.Close)/4);
                    break;
                case AppliedPrice.Move:
                    builder.MapBarInput(inputName, symbol, entity => entity.Close - entity.Open);
                    break;
                case AppliedPrice.Range:
                    builder.MapBarInput(inputName, symbol, entity => entity.High - entity.Low);
                    break;
            }
        }
    }
}
