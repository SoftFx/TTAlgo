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
        private readonly Dictionary<Params, double> _exsistDict = new Dictionary<Params, double>();
        private IBacktestQueue _queue;
        protected long _casesLeft;

        protected Generator generator;


        internal void OnInit(Dictionary<string, ParamSeekSet> parameters)
        {
            generator = new Generator();

            InitParams = parameters;
        }

        protected IReadOnlyDictionary<string, ParamSeekSet> InitParams { get; private set; }

        protected void SetQueue(IBacktestQueue queue)
        {
            _queue = queue;
            _casesLeft = CaseCount;
        }

        protected bool SendParams(Params dict)
        {
            --_casesLeft;

            if (!_exsistDict.ContainsKey(dict))
            {
                _exsistDict.Add(dict, 0);
                _queue.Enqueue(dict);
                return true;
            }

            return false;
        }

        public abstract long CaseCount { get; }
        public abstract void Start(IBacktestQueue queue, int degreeOfParallelism);
        public abstract long OnCaseCompleted(OptCaseReport report);
    }

    public interface IBacktestQueue
    {
        void Enqueue(Params caseCfg);
    }
}
