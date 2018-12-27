namespace TickTrader.Algo.Api.Indicators
{
    public interface IBoolingerBands
    {
        int Period { get; }

        int Shift { get; }

        double Deviations { get; }

        DataSeries Price { get; }

        DataSeries MiddleLine { get; }

        DataSeries TopLine { get; }

        DataSeries BottomLine { get; }

        int LastPositionChanged { get; }
    }
}
