using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BruteforceStrategy : ParamSeekStrategy
    {
        private IEnumerator<Params> _e;
        private long _casesLeft;

        public override long CaseCount => Params.Values.Aggregate(1, (s, p) => s * p.Size);

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            _casesLeft = CaseCount;

            if (_casesLeft <= 0)
                return;

            int firstPartSize = (int)Math.Min(_casesLeft, (long)degreeOfParallelism * 2);

            _e = GetCases().GetEnumerator();

            for (int i = 0; i < firstPartSize; i++)
            {
                if (!_e.MoveNext())
                    break;
                queue.Enqueue(_e.Current);
            }
        }

        public override long OnCaseCompleted(OptCaseReport report, IBacktestQueue queue)
        {
            if (_e.MoveNext())
                queue.Enqueue(_e.Current);

            _casesLeft--;
            return _casesLeft;
        }

        private IEnumerable<Params> GetCases()
        {
            for (long i = 0; i < CaseCount; i++)
            {
                var rm = i;
                var cfgCase = new Params(i);

                foreach (var p in Params)
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
