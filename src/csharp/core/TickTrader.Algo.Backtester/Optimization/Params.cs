using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Backtester
{
    public class Params : List<AlgoParameter>
    {
        public Params() { }

        public double? Result { get; set; }

        public Params(int count) : base(count) { }

        public Params(IEnumerable<AlgoParameter> list) : base(list) { }

        public Params Copy() => new Params(this.Select(u => u.FullCopy())) { Result = Result };

        public void AddCopyRange(IEnumerable<AlgoParameter> list)
        {
            foreach (var p in list)
                Add(p.Copy());
        }

        public override string ToString() => string.Join(",", this.Select(u => u.ToShortString()));
    }
}
