using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Ext
{
    [Reduction("Open")]
    public class BarOpenReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Open;
        }
    }


    [Reduction("Close")]
    public class BarCloseReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Close;
        }
    }


    [Reduction("High")]
    public class BarHighReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.High;
        }
    }


    [Reduction("Low")]
    public class BarLowReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Low;
        }
    }


    [Reduction("Volume")]
    public class BarVolumeReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Volume;
        }
    }


    [Reduction("Median")]
    public class BarMedianReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return (bar.High + bar.Low) / 2;
        }
    }


    [Reduction("Typical")]
    public class BarTypicalReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return (bar.High + bar.Low + bar.Close) / 3;
        }
    }


    [Reduction("Weighted")]
    public class BarWeightedReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return (bar.High + bar.Low + 2 * bar.Close) / 4;
        }
    }


    [Reduction("Move")]
    public class BarMoveReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Close - bar.Open;
        }
    }


    [Reduction("Range")]
    public class BarRangeReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.High - bar.Low;
        }
    }
}
