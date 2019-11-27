using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class Indicator : AlgoPlugin
    {
        public Indicator()
        {
        }

        /// <summary>
        /// True if Calculate() is called to recalculate last bar/quote. Inputs and outputs are not shifted in this case.
        /// False if Calculate() is called on a new bar/quote. Inputs and outputs are shifted, fresh data is placed at zero index.
        /// </summary>

        [Obsolete("Will be removed in future. Use IsNewBar instead")]
        protected bool IsUpdate { get; private set; }
        protected bool IsNewBar { get; private set; }


        protected abstract void Calculate();

        internal void InvokeCalculate(bool isUpdate)
        {
            IsUpdate = isUpdate;
            IsNewBar = !isUpdate;
            Calculate();
        }
    }
}
