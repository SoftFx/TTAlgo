using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class BruteforceStrategy : ParamSeekStrategy
    {
        public override void Init()
        {
            CaseCount = Params.Values.Aggregate(1, (s, p) => s * p.Size);
        }

        public override IEnumerable<OptCaseConfig> GetCases()
        {
            for (int i = 0; i < CaseCount; i++)
            {
                var rm = i;
                var cfgCase = new OptCaseConfig();

                foreach (var p in Params)
                {
                    var set = p.Value;
                    var valCount = set.Size;
                    var val = set.GetParamValue(rm % valCount);
                    cfgCase.Add(p.Key, val);

                    rm = rm / valCount;
                }

                yield return cfgCase;
            }
        }
    }
}
