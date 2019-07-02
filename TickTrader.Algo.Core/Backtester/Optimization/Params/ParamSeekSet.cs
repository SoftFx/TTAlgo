using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public abstract class ParamSeekSet
    {
        public abstract int Size { get; }
        public abstract object GetParamValue(int valNo);
    }

    public abstract class ParamSeekSet<T> : ParamSeekSet
    {
        protected abstract T GetValue(int valNo);

        public override object GetParamValue(int valNo)
        {
            return GetValue(valNo);
        }
    }
}
