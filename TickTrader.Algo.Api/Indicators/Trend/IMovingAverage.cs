namespace TickTrader.Algo.Api.Indicators
{
    public enum MovingAverageMethod
    {
        Simple = 0,
        Exponential = 1,
        Smoothed = 2,
        LinearWeighted = 3,
        CustomExponential = 4,
        Triangular = 5,
    }

    public interface IMovingAverage
    {
        int Period { get; }

        int Shift { get; }

        MovingAverageMethod TargetMethod { get; }

        double SmoothFactor { get; }

        DataSeries Price { get; }

        DataSeries Average { get; }
    }
}
