namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal interface IMA
    {
        int Period { get; }

        int Accumulated { get; }
        double LastAdded { get; }
        double Average { get; }

        void Init();
        void Reset();
        void Add(double value);
        void UpdateLast(double value);
    }
}
