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
    internal abstract class PluginAdapter : NoTimeoutByRefObject, IPluginActivator
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

            Descriptor = AlgoPluginDescriptor.Get(plugin.GetType());
            Descriptor.Validate();
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

            Descriptor = AlgoPluginDescriptor.Get(plugin.GetType());
            Descriptor.Validate();
        }

        public BuffersCoordinator Coordinator { get; private set; }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        protected IReadOnlyList<IndicatorAdapter> NestedIndicators { get { return nestedIndacators; } }
        protected AlgoPlugin PluginInstance { get { return plugin; } }

        public void SetParameter(string id, object val)
        {
            var paramDescriptor = Descriptor.Parameters.FirstOrDefault(p => p.Id == id);
            if (paramDescriptor == null)
                throw new InvalidOperationException("Can't find parameter with id = " + id);
            paramDescriptor.Set(plugin, val);
        }

        public IDataSeriesProxy GetInput(string id)
        {
            return inputs[id];
        }

        public DataSeriesProxy<T> GetInput<T>(string id)
        {
            return (DataSeriesProxy<T>)inputs[id];
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
                    nestedIndacators[i].plugin.InvokeInit();

                plugin.InvokeInit();
            }
            finally
            {
                AlgoPlugin.activator = null;
            }
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
            foreach (var paramProperty in Descriptor.Parameters)
            {
                if (paramProperty.DefaultValue != null)
                    paramProperty.Set(plugin, paramProperty.DefaultValue);
            }
        }

        protected void BindUpInputs()
        {
            foreach (var inputProperty in Descriptor.Inputs)
                ReflectGenericMethod(inputProperty.DatdaSeriesBaseType, "BindInput", inputProperty);
        }

        protected void BindUpOutputs()
        {
            foreach (var outputProperty in Descriptor.Outputs)
                ReflectGenericMethod(outputProperty.DataSeriesBaseType, "BindOutput", outputProperty);
        }

        public void BindInput<T>(InputDescriptor d)
        {
            var input = d.CreateInput2<T>();
            input.Buffer = new EmptyBuffer<T>();
            d.Set(plugin, input);
            inputs.Add(d.Id, input);
        }

        public void BindOutput<T>(OutputDescriptor d)
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

        private static AlgoPluginDescriptor GetDescriptorOrThrow(string id)
        {
            AlgoPluginDescriptor descriptor = AlgoPluginDescriptor.Get(id);

            if (descriptor == null)
                throw new ArgumentException("Cannot find plugin descriptor: " + id);

            return descriptor;
        }

        public static PluginAdapter Create(AlgoPluginDescriptor descriptor, IPluginContext dataProvider)
        {
            if (descriptor.AlgoLogicType == AlgoTypes.Indicator)
                return new IndicatorAdapter(() => (AlgoPlugin)Activator.CreateInstance(descriptor.AlgoClassType), dataProvider);
            else if (descriptor.AlgoLogicType == AlgoTypes.Robot)
                return new BotAdapter(() => (TradeBot)Activator.CreateInstance(descriptor.AlgoClassType), dataProvider);
            else
                throw new InvalidPluginType("Unsupported plugin type: " + descriptor.AlgoLogicType);
        }

        public abstract void InvokeCalculate(bool isUpdate);
        public abstract void InvokeOnStart();
        public abstract void InvokeOnStop();
        public abstract void InvokeOnQuote(Quote quote);

        protected void InvokeCalculateForNestedIndicators(bool isUpdate)
        {
            for (int i = NestedIndicators.Count - 1; i >= 0; i--)
                NestedIndicators[i].InvokeCalculate(isUpdate);
        }

        //public static IndicatorAdapter CreateIndicator(string id, IPluginDataProvider dataProvider)
        //{
        //    AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);

        //    if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
        //        throw new InvalidPluginType("CreateIndicator() can be called only for indicators!");

        //    descriptor.Validate();

        //    return new IndicatorAdapter(() => (AlgoPlugin)Activator.CreateInstance(descriptor.AlgoClassType), dataProvider);
        //}

        //public static BotAdapter CreateBot(string id, IPluginDataProvider dataProvider)
        //{
        //    AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);

        //    if (descriptor.AlgoLogicType != AlgoTypes.Robot)
        //        throw new InvalidPluginType("CreateBot() can be called only for bot plugins!");

        //    descriptor.Validate();

        //    return new BotAdapter(() => (TradeBot)Activator.CreateInstance(descriptor.AlgoClassType), dataProvider);
        //}
    }
}
