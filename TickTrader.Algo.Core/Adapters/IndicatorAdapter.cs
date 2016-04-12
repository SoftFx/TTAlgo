using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class IndicatorAdapter : PluginAdapter
    {
        internal IndicatorAdapter(AlgoPlugin pluginInstance, IPluginDataProvider provider, BuffersCoordinator coordinator)
            : base(pluginInstance, provider, coordinator)
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        internal IndicatorAdapter(Func<AlgoPlugin> pluginFactory, IPluginDataProvider provider)
            : base(pluginFactory, provider, new BuffersCoordinator())
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        private void InvokeCalculate(bool isUpdate)
        {
            ((Indicator)PluginInstance).InvokeCalculate(isUpdate);
        }

        public void Calculate(bool isUpdate)
        {
            for (int i = NestedIndicators.Count - 1; i >= 0; i--)
                NestedIndicators[i].InvokeCalculate(isUpdate);

            InvokeCalculate(isUpdate);
        }

        public override string ToString()
        {
            return "Indicator: " + Descriptor.DisplayName;
        }
    }
}
