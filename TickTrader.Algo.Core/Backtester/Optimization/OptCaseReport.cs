using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OptCaseReport
    {
        public OptCaseReport(ParamsMessage cfg, double metric, TestingStatistics stats, Exception error = null)
        {
            Config = cfg;
            MetricVal = metric;
            Stats = stats;
            ExecError = error;
        }

        public ParamsMessage Config { get; }
        public double MetricVal { get; }
        public Exception ExecError { get; }
        public TestingStatistics Stats { get; }

        public List<BarEntity> Equity { get; set; }
        public List<BarEntity> Margin { get; set; }
    }
}
