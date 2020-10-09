using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    internal abstract class PluginAdapter : CrossDomainObject, IPluginActivator
    {
        private AlgoPlugin plugin;
        private IPluginContext provider;
        private List<IndicatorAdapter> nestedIndacators = new List<IndicatorAdapter>();
        private Dictionary<string, IDataSeriesProxy> inputs = new Dictionary<string, IDataSeriesProxy>();
        private Dictionary<string, IDataSeriesProxy> outputs = new Dictionary<string, IDataSeriesProxy>();

        internal PluginAdapter(AlgoPlugin plugnInstance, IPluginContext provider, BuffersCoordinator coordinator)
        {
            this.provider = provider;
            this.plugin = plugnInstance;
            this.Coordinator = coordinator;

            Metadata = AlgoAssemblyInspector.GetPlugin(plugin.GetType());
            Metadata.Validate();
        }

        internal PluginAdapter(Func<AlgoPlugin> pluginFactoy, IPluginContext provider, BuffersCoordinator coordinator)
        {
            this.provider = provider;
            this.Coordinator = coordinator;

            try
            {
                AlgoPlugin.activator = this;
                plugin = pluginFactoy();
            }
            finally
            {
                AlgoPlugin.activator = null;
            }

            Metadata = AlgoAssemblyInspector.GetPlugin(plugin.GetType());
            Metadata.Validate();
        }

        public BuffersCoordinator Coordinator { get; private set; }
        public PluginMetadata Metadata { get; private set; }
        protected IReadOnlyList<IndicatorAdapter> NestedIndicators { get { return nestedIndacators; } }
        protected AlgoPlugin PluginInstance { get { return plugin; } }

        public void SetParameter(string id, object val)
        {
            var paramMetadata = Metadata.Parameters.FirstOrDefault(p => p.Id == id);
            if (paramMetadata == null)
                throw new InvalidOperationException("Can't find parameter with id = " + id);
            paramMetadata.Set(plugin, val);
        }

        public IDataSeriesProxy GetInput(string id)
        {
            return inputs[id];
        }

        public DataSeriesImpl<T> GetInput<T>(string id)
        {
            return (DataSeriesImpl<T>)inputs[id];
        }

        public IDataSeriesProxy GetOutput(string id)
        {
            IDataSeriesProxy proxy;
            if (!outputs.TryGetValue(id, out proxy))
                throw new Exception("Output Not Found: " + id);
            return proxy;
        }

        public void InvokeInit()
        {
            try
            {
                AlgoPlugin.activator = this;

                for (int i = nestedIndacators.Count - 1; i >= 0; i--)
                    nestedIndacators[i].plugin.InvokeInit(true);

                plugin.InvokeInit(false);
            }
            finally
            {
                AlgoPlugin.activator = null;
            }
        }

        public void InvokeConnectedEvent()
        {
            var args = new ConnectedEventArgs();

            for (int i = nestedIndacators.Count - 1; i >= 0; i--)
                nestedIndacators[i].plugin.InvokeConnected(args);

            plugin.InvokeConnected(args);
        }

        public void InvokeDisconnectedEvent()
        {
            var args = new DisconnectedEventArgs();

            for (int i = nestedIndacators.Count - 1; i >= 0; i--)
                nestedIndacators[i].plugin.InvokeDisconnected(args);

            plugin.InvokeDisconnected(args);
        }

        IPluginContext IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (plugin == null) // Activate() is called from constructor
                return provider;

            if (instance is Indicator)
            {
                var context = new IndicatorAdapter(instance, this.provider, Coordinator);
                nestedIndacators.Add(context);
                return provider;
            }

            return null;
        }

        protected void InitParameters()
        {
            foreach (var paramProperty in Metadata.Parameters)
            {
                if (paramProperty.DefaultValue != null)
                    paramProperty.ResetValue(plugin);
            }
        }

        protected void BindUpInputs()
        {
            foreach (var inputProperty in Metadata.Inputs)
                ReflectGenericMethod(inputProperty.DataSeriesBaseType, "BindInput", inputProperty);
        }

        protected void BindUpOutputs()
        {
            foreach (var outputProperty in Metadata.Outputs)
                ReflectGenericMethod(outputProperty.DataSeriesBaseType, "BindOutput", outputProperty);
        }

        public void BindInput<T>(InputMetadata d)
        {
            var input = d.CreateInput2<T>();
            input.Buffer = new EmptyBuffer<T>();
            d.Set(plugin, input);
            inputs.Add(d.Id, input);
        }

        public void BindOutput<T>(OutputMetadata d)
        {
            var output = d.CreateOutput2<T>();
            output.Buffer = OutputBuffer<T>.Create(Coordinator, d.IsHiddenEntity);
            d.Set(plugin, output);
            outputs.Add(d.Id, output);
        }

        private void ReflectGenericMethod(Type genericType, string methodName, params object[] parameters)
        {
            MethodInfo method = GetType().GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(genericType);
            genericMethod.Invoke(this, parameters);
        }

        private static PluginMetadata GetMetadataOrThrow(string id)
        {
            PluginMetadata metadata = AlgoAssemblyInspector.GetPlugin(id);

            if (metadata == null)
                throw new ArgumentException("Cannot find plugin metadata: " + id);

            return metadata;
        }

        public static PluginAdapter Create(PluginMetadata metadata, IPluginContext dataProvider)
        {
            if (metadata.Descriptor.Type == AlgoTypes.Indicator)
                return new IndicatorAdapter(() => (AlgoPlugin)metadata.CreateInstance(), dataProvider);
            else if (metadata.Descriptor.Type == AlgoTypes.Robot)
                return new BotAdapter(() => (TradeBot)metadata.CreateInstance(), dataProvider);
            else
                throw new InvalidPluginType("Unsupported plugin type: " + metadata.Descriptor.Type);
        }

        public abstract void InvokeCalculate(bool isUpdate);
        public abstract void InvokeOnStart();
        public abstract void InvokeOnStop();
        public abstract void InvokeOnQuote(Quote quote);
        public abstract Task InvokeAsyncStop();
        public abstract double InvokeGetMetric();
        public abstract void InvokeOnModelTick();

        protected void InvokeCalculateForNestedIndicators(bool isUpdate)
        {
            for (int i = NestedIndicators.Count - 1; i >= 0; i--)
                NestedIndicators[i].InvokeCalculate(isUpdate);
        }
    }
}
