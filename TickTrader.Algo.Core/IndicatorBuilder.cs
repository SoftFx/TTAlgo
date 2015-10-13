using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder
    {
        private IndicatorProxy proxy;

        public IndicatorBuilder(string indicatorId)
        {
            this.Host = new AlgoHost();
            this.proxy = new IndicatorProxy(indicatorId, Host);
        }

        public void InvokeCalculate()
        {
            this.proxy.InvokeCalculate();
        }

        public AlgoHost Host { get; private set; }
    }
}
