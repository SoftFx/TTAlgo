using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public abstract class ParamSeekStrategy
    {
        internal void OnInit(Dictionary<string, ParamSeekSet> parameters)
        {
            Params = parameters;
            Init();
        }

        public int CaseCount { get; protected set; }

        protected IReadOnlyDictionary<string, ParamSeekSet> Params { get; private set; }

        public abstract void Init();

        public abstract IEnumerable<OptCaseConfig> GetCases();
    }
}
