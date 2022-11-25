namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal interface IMA
    {
        double Average { get; }

        void Init();
        void Reset();
        void Add(double value);
        void UpdateLast(double value);
    }
}
