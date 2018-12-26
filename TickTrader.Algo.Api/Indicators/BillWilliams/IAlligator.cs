namespace TickTrader.Algo.Api.Indicators
{
    public interface IAlligator
    {
        int JawsPeriod { get; }

        int JawsShift { get; }

        int TeethPeriod { get; }

        int TeethShift { get; }

        int LipsPeriod { get; }

        int LipsShift { get; }

        MovingAverageMethod TargetMethod { get; }

        DataSeries Jaws { get; }

        DataSeries Teeth { get; }

        DataSeries Lips { get; }
    }
}
