namespace TickTrader.Algo.Backtester
{
    public class AnnConfig
    {
        public double InitialTemperature { get; set; }

        public double DeltaTemparature { get; set; }

        public long InnerIterationCount { get; set; }

        public long? OutherIterationCount { get; set; }

        public double VeryFastTempDecrement { get; set; }

        public DecreaseConditionMode DecreaseConditionMode { get; set; }

        public SimulatedAnnealingMethod MethodForT { get; set; }

        public SimulatedAnnealingMethod MethodForG { get; set; }
    }

    public enum DecreaseConditionMode
    {
        ImproveAnswer,
        FullCycle,
    }

    public enum SimulatedAnnealingMethod
    {
        Custom,
        Boltzmann,
        Cauchy,
        VeryFast,
    }
}
