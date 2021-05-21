using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    public abstract class MetricProvider
    {
        public static Equity Default { get; } = new Equity();

        public abstract double GetMetric(PluginBuilder builder, TestingStatistics stats);


        public class Equity : MetricProvider
        {
            public override double GetMetric(PluginBuilder builder, TestingStatistics stats)
            {
                return builder.Account.Equity;
            }
        }


        public class Custom : MetricProvider
        {
            public override double GetMetric(PluginBuilder builder, TestingStatistics stats)
            {
                return builder.InvokeGetMetric(out _);
            }
        }
    }
}
