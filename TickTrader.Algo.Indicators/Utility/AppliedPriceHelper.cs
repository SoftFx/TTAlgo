using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Utility
{
    public static class AppliedPriceHelper
    {
        public static DataSeries GetDataSeries(BarSeries bars, AppliedPrice price)
        {
            switch (price)
            {
                case AppliedPrice.Close:
                    return bars.Close;
                case AppliedPrice.Open:
                    return bars.Open;
                case AppliedPrice.High:
                    return bars.High;
                case AppliedPrice.Low:
                    return bars.Low;
                case AppliedPrice.Median:
                    return bars.Median;
                case AppliedPrice.Typical:
                    return bars.Typical;
                case AppliedPrice.Weighted:
                    return bars.Weighted;
                case AppliedPrice.Move:
                    return bars.Move;
                case AppliedPrice.Range:
                    return bars.Range;
                default:
                    return null;
            }
        }
    }
}
