using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class ParamSeekStrategy
    {
        protected Generator generator;

        internal void OnInit(Dictionary<string, ParamSeekSet> parameters)
        {
            generator = new Generator();
            Params = parameters;
        }

        protected IReadOnlyDictionary<string, ParamSeekSet> Params { get; private set; }

        public abstract long CaseCount { get; }
        public abstract void Start(IBacktestQueue queue, int degreeOfParallelism);
        public abstract long OnCaseCompleted(OptCaseReport report, IBacktestQueue queue);
    }

    public interface IBacktestQueue
    {
        void Enqueue(OptCaseConfig caseCfg);
    }
}
