using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OptCaseReport
    {
        public OptCaseReport(OptCaseConfig cfg, double metric, TestingStatistics stats, Exception error = null)
        {
            Config = cfg;
            MetricVal = metric;
            Stats = stats;
            ExecError = error;
        }

        public OptCaseConfig Config { get; }
        public double MetricVal { get; }
        public Exception ExecError { get; }
        public TestingStatistics Stats { get; }

        public List<BarData> Equity { get; set; }
        public List<BarData> Margin { get; set; }
    }
}
