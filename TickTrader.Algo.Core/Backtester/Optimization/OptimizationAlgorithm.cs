using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class OptimizationAlgorithm
    {
        protected IReadOnlyDictionary<string, ParamSeekSet> InitParams { get; private set; }
        protected Dictionary<string, Params> _existingItems { get; private set; }
        protected Dictionary<long, Params> _dispatchQueue { get; private set; }

        protected Params BestSet { get; set; }
        protected Params Set { get; set; }

        protected Generator generator;
        private IBacktestQueue _queue;

        protected long _casesLeft;
        protected long _sendMessages = 0;
        protected long _allCases = 0;

        protected bool AlgoCompleate => _casesLeft < 1 || _existingItems.Count == _allCases;

        internal void OnInit(Dictionary<string, ParamSeekSet> parameters)
        {
            _existingItems = new Dictionary<string, Params>();
            _dispatchQueue = new Dictionary<long, Params>();
            generator = new Generator();
            InitParams = parameters;
            Set = new Params();

            _allCases = InitParams.Values.Aggregate(1, (x, y) => x * y.Size);

            foreach (var v in InitParams.Values)
            {
                AlgoParameter parameter = null;

                if (v is ConstParam p)
                    parameter = new AlgoParameter();
                if (v is RangeParamSet<int> pi)
                    parameter = new AlgoParameter(pi.Min, pi.Max, pi.Step);
                if (v is RangeParamSet<double> pd)
                    parameter = new AlgoParameter(pd.Min, pd.Max, pd.Step);
                if (v is EnumParamSet<bool> pb)
                    parameter = new AlgoParameter(0, 1, 1);
                if (v is EnumParamSet<string> ps)
                    parameter = new AlgoParameter(0, ps.Size, 1);

                if (parameter == null)
                    throw new ArgumentException($"Unsupported property type: {v.GetType()}");

                Set.Add(parameter);
            }

            BestSet = Set.Copy();
        }

        protected void SetQueue(IBacktestQueue queue)
        {
            _queue = queue;
            _casesLeft = CaseCount;
        }

        protected void SetResult(ParamsMessage mes, double result)
        {
            if (_dispatchQueue.ContainsKey(mes.Id))
            {
                var set = _dispatchQueue[mes.Id];
                set.Result = result;

                if (!BestSet.Result.HasValue || BestSet.Result < set.Result)
                    BestSet = set.Copy();

                _dispatchQueue.Remove(mes.Id);
            }
            else
                throw new ArgumentException($"Unknown message id={mes.Id}");
        }

        protected long SendParams(ParamsMessage dict)
        {
            _queue.Enqueue(dict);

            return --_casesLeft;
        }

        protected void SendParams(Params par, ref int count)
        {
            --_casesLeft;

            if (!_existingItems.ContainsKey(par.ToString()))
            {
                var dict = new ParamsMessage(_sendMessages++);

                int i = 0;

                foreach (var pair in InitParams)
                {
                    object o = pair.Value.GetParamValue(0);

                    if (pair.Value is RangeParamSet<int>)
                        o = (int)par[i].Current;
                    if (pair.Value is RangeParamSet<double>)
                        o = par[i].Current;
                    if (pair.Value is EnumParamSet<bool>)
                        o = pair.Value.GetParamValue((int)par[i].Current);
                    if (pair.Value is EnumParamSet<string>)
                        o = pair.Value.GetParamValue((int)par[i].Current);

                    dict.Add(pair.Key, o);
                    i++;
                }

                _queue.Enqueue(dict);
                _dispatchQueue.Add(dict.Id, par);
                _existingItems.Add(par.ToString(), par);
            }
            else
            {
                par.Result = _existingItems[par.ToString()].Result ?? 0;
                count++;
            }
        }

        public abstract long CaseCount { get; }
        public abstract void Start(IBacktestQueue queue, int degreeOfParallelism);
        public abstract long OnCaseCompleted(OptCaseReport report);
    }

    public interface IBacktestQueue
    {
        void Enqueue(ParamsMessage caseCfg);
    }
}
