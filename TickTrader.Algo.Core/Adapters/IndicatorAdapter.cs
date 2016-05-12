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

        public override void InvokeCalculate(bool isUpdate)
        {
            InvokeCalculateForNestedIndicators(isUpdate);
            ((Indicator)PluginInstance).InvokeCalculate(isUpdate);
        }

        public override void InvokeOnStart()
        {
        }

        public override void InvokeOnStop()
        {
        }

        public override string ToString()
        {
            return "Indicator: " + Descriptor.DisplayName;
        }
    }
}
