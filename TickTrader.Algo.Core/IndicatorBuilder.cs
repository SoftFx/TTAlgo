using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder : MarshalByRefObject
    {
        private IndicatorProxy proxy;
        private AlgoDescriptor descriptor;

        public IndicatorBuilder(string indicatorId)
        {
            descriptor = AlgoDescriptor.Get(indicatorId);
            this.Host = new AlgoHost(descriptor);
        }

        public void Init()
        {
            this.proxy = new IndicatorProxy(descriptor, Host);
        }

        public void Reset()
        {
            this.Host.Reset();
        }

        public void InvokeCalculate()
        {
            this.proxy.InvokeCalculate();
        }

        public AlgoHost Host { get; private set; }
    }
}
