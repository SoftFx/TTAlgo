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
        public OptCaseReport(OptCaseConfig cfg, double metric, Exception error = null)
        {
            Config = cfg;
            MetricVal = metric;
            ExecError = error;
        }

        public OptCaseConfig Config { get; }
        public double MetricVal { get; }
        public Exception ExecError { get; }
    }
}
