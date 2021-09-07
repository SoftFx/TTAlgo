using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Backtester
{
    public class BruteforceStrategy : OptimizationAlgorithm
    {
        private IEnumerator<ParamsMessage> _e;

        public override long CaseCount => InitParams.Values.Aggregate(1, (s, p) => s * p.Size);

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            SetQueue(queue);

            int firstPartSize = (int)Math.Min(CaseCount, (long)degreeOfParallelism * 2);

            _e = GetCases().GetEnumerator();

            for (int i = 0; i < firstPartSize; i++)
            {
                if (!_e.MoveNext())
                    break;

                SendParams(_e.Current);
            }
        }

        public override long OnCaseCompleted(OptCaseReport report)
        {
            return _e.MoveNext() ? SendParams(_e.Current) : 0;
        }

        private IEnumerable<ParamsMessage> GetCases()
        {
            for (long i = 0; i < CaseCount; i++)
            {
                var rm = i;
                var cfgCase = new ParamsMessage(i);

                foreach (var p in InitParams)
                {
                    var set = p.Value;
                    var valCount = set.Size;
                    var val = set.GetParamValue((int)(rm % (long)valCount));
                    cfgCase.Add(p.Key, val);

                    rm = rm / valCount;
                }

                yield return cfgCase;
            }
        }
    }
}
