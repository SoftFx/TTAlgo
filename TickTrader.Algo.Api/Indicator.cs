using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class Indicator : Algo
    {
        public Indicator()
        {
            if (currentInstance != null)
                currentInstance.nestedIndicators.Add(this);
        }

        protected abstract void Calculate();

        internal void DoCalculate()
        {
            Calculate();
        }
    }

    public abstract class BarIndicator : Indicator
    {
    }

    public abstract class TickIndicator : Indicator
    {
    }
}
