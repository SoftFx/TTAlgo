using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder : MarshalByRefObject
    {
        private IndicatorProxy proxy;
        private string indicatorId;

        public IndicatorBuilder(string indicatorId)
        {
            this.Host = new AlgoHost();
            this.indicatorId = indicatorId;
        }

        public void Init()
        {
            this.proxy = new IndicatorProxy(indicatorId, Host);
        }

        public void InvokeCalculate()
        {
            this.proxy.InvokeCalculate();
        }

        public AlgoHost Host { get; private set; }
    }
}
