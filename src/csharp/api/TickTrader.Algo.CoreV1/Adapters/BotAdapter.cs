using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.CoreV1.Metadata;

namespace TickTrader.Algo.CoreV1
{
    internal class BotAdapter : PluginAdapter
    {
        internal BotAdapter(PluginMetadata metadata, IPluginContext provider)
            : base(metadata, provider, new BuffersCoordinator())
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        public override void InvokeCalculate(bool isUpdate)
        {
            InvokeCalculateForNestedIndicators(isUpdate);
        }

        public override void InvokeOnQuote(Quote quote)
        {
            ((TradeBot)PluginInstance).InvokeOnQuote(quote);
        }

        public override void InvokeOnStart()
        {
            ((TradeBot)PluginInstance).InvokeStart();
        }

        public override void InvokeOnStop()
        {
            ((TradeBot)PluginInstance).InvokeStop();
        }

        public override Task InvokeAsyncStop()
        {
            return ((TradeBot)PluginInstance).InvokeAsyncStop();
        }

        public override double InvokeGetMetric()
        {
            return ((TradeBot)PluginInstance).InvokeGetMetric();
        }

        public override void InvokeOnModelTick()
        {
            ((TradeBot)PluginInstance).InvokeOnModelTick();
        }
    }
}
