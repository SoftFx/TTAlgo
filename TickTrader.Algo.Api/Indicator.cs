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
            //if (currentInstance != null)
            //    currentInstance.nestedIndicators.Add(this);
        }

        protected abstract void Calculate();

        internal void InvokeCalculate()
        {
            Calculate();

            //foreach (Indicator i in nestedIndicators)
            //    i.DoCalculate();

            //Calculate();
        }
    }
}
