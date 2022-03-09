namespace TickTrader.Algo.BacktesterApi
{
    public enum OptimizationAlgorithms { Bruteforce, Genetic, /*Annealing*/ }

    public enum OptimizationMetrics { Equity, Custom }


    public class OptimizationConfig
    {
        public OptimizationAlgorithms Algorithm { get; set; }

        public OptimizationMetrics Metric { get; set; }

        public int DegreeOfParallelism { get; set; }
    }
}
