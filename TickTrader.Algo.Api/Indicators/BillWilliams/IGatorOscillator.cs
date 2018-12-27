namespace TickTrader.Algo.Api.Indicators
{
    public interface IGatorOscillator
    {
        int JawsPeriod { get; }

        int JawsShift { get; }

        int TeethPeriod { get; }

        int TeethShift { get; }

        int LipsPeriod { get; }

        int LipsShift { get; }

        MovingAverageMethod TargetMethod { get; }

        DataSeries Price { get; }

        DataSeries TeethLipsUp { get; }

        DataSeries TeethLipsDown { get; }

        DataSeries JawsTeethUp { get; }

        DataSeries JawsTeethDown { get; }

        int LastPositionChanged { get; }
    }
}
