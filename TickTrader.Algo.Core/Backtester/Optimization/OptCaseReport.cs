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
        public OptCaseReport(OptCaseConfig cfg, double metric)
        {
            Config = cfg;
            MetricVal = metric;
        }

        public OptCaseConfig Config { get; }
        public double MetricVal { get; }
    }
}
