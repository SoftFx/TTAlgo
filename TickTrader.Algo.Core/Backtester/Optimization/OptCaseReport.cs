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
        public OptCaseReport(OptCaseConfig cfg, TestingStatistics stats, Exception error = null)
        {
            Config = cfg;
            //MetricVal = metric;
            Stats = stats;
            ExecError = error;
        }

        public OptCaseConfig Config { get; }
        //public double MetricVal { get; }
        public Exception ExecError { get; }
        public TestingStatistics Stats { get; }

        public List<BarEntity> Equity { get; set; }
        public List<BarEntity> Margin { get; set; }
    }
}
