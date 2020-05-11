using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class GeneticStrategy : ParamSeekStrategy
    {
        public override long CaseCount => 10;

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            throw new NotImplementedException();
        }

        public override long OnCaseCompleted(OptCaseReport report, IBacktestQueue queue)
        {
            return 10;
        }
    }
}
