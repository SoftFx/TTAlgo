namespace TickTrader.Algo.Indicators.Utility
{
    internal interface IShift
    {
        int Shift { get; }

        int Position { get; }
        double Result { get; }

        void Init();
        void Reset();
        void Add(double value);
        void UpdateLast(double value);
    }
}
