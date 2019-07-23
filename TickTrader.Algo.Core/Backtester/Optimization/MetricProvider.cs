using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class MetricProvider
    {
        public static Equity Default { get; } = new Equity();

        public abstract double GetMetric(PluginBuilder builder, TestingStatistics stats);

        [Serializable]
        public class Equity : MetricProvider
        {
            public override double GetMetric(PluginBuilder builder, TestingStatistics stats)
            {
                return builder.Account.Equity;
            }
        }

        [Serializable]
        public class Custom : MetricProvider
        {
            public override double GetMetric(PluginBuilder builder, TestingStatistics stats)
            {
                return builder.InvokeGetMetric(out _);
            }
        }
    }
}
