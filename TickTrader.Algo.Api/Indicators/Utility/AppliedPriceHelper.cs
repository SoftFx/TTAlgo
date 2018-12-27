namespace TickTrader.Algo.Api.Indicators
{
    public static class AppliedPrice
    {
        public enum Target
        {
            Close, Open, High, Low, Median, Typical, Weighted, Move, Range
        }

        public static DataSeries GetDataSeries(BarSeries bars, Target target)
        {
            switch (target)
            {
                case Target.Close:
                    return bars.Close;
                case Target.Open:
                    return bars.Open;
                case Target.High:
                    return bars.High;
                case Target.Low:
                    return bars.Low;
                case Target.Median:
                    return bars.Median;
                case Target.Typical:
                    return bars.Typical;
                case Target.Weighted:
                    return bars.Weighted;
                case Target.Move:
                    return bars.Move;
                case Target.Range:
                    return bars.Range;
                default:
                    return null;
            }
        }
    }
}
