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
        internal IndicatorAdapter(AlgoPlugin pluginInstance, IPluginContext provider, BuffersCoordinator coordinator)
            : base(pluginInstance, provider, coordinator)
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        internal IndicatorAdapter(Func<AlgoPlugin> pluginFactory, IPluginContext provider)
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
            // Do nothing. Indicators does not have OnStart() method
        }

        public override void InvokeOnStop()
        {
            // Do nothing. Indicators does not have OnStop() method
        }

        public override Task InvokeAsyncStop()
        {
            // Do nothing. Indicators does not have OnStop() method
            return Task.FromResult(this);
        }

        public override void InvokeOnQuote(Quote quote)
        {
            // Do nothing. Indicators does not have OnQuote() method
        }

        public override double InvokeGetMetric()
        {
            // Do nothing. Indicators cannot be optimized
            return -1;
        }

        public override void InvokeOnModelTick()
        {
            // Do nothing. Indicators does not have OnModelTick() method
        }

        public override string ToString()
        {
            return "Indicator: " + Metadata.Descriptor.DisplayName;
        }
    }
}
