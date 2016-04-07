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
    internal class PluginContext : NoTimeoutByRefObject, IPluginActivator
    {
        private AlgoPlugin plugin;
        private IPluginDataProvider provider;
        private List<IndicatorContext> nestedIndacators = new List<IndicatorContext>();
        private Dictionary<string, IDataSeriesProxy> inputs = new Dictionary<string, IDataSeriesProxy>();
        private Dictionary<string, IDataSeriesProxy> outputs = new Dictionary<string, IDataSeriesProxy>();

        internal PluginContext(AlgoPlugin plugnInstance, IPluginDataProvider provider, BuffersCoordinator coordinator)
        {
            this.provider = provider;
            this.plugin = plugnInstance;
            this.Coordinator = coordinator;

            Descriptor = AlgoPluginDescriptor.Get(plugin.GetType());
            Descriptor.Validate();
        }

        internal PluginContext(Func<AlgoPlugin> pluginFactoy, IPluginDataProvider provider, BuffersCoordinator coordinator)
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
        protected IReadOnlyList<IndicatorContext> NestedIndicators { get { return nestedIndacators; } }
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
            return outputs[id];
        }

        private void InvokeInit()
        {
            plugin.InvokeInit();
        }

        public void Init()
        {
            try
            {
                AlgoPlugin.activator = this;

                for (int i = nestedIndacators.Count - 1; i >= 0; i--)
                    nestedIndacators[i].InvokeInit();

                InvokeInit();
            }
            finally
            {
                AlgoPlugin.activator = null;
            }
        }

        IPluginDataProvider IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (plugin == null) // Activate() is called from constructor
                return provider;

            if (instance is Indicator)
            {
                var context = new IndicatorContext(instance, this.provider, Coordinator);
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
                ReflectGenericMethod(outputProperty.DatdaSeriesBaseType, "BindOutput", outputProperty);
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
            output.Buffer = new OutputBuffer<T>(Coordinator);
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


        public static IndicatorContext CreateIndicator(string id, IPluginDataProvider dataProvider)
        {
            AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);

            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new InvalidPluginType("CreateIndicator() can be called only for indicators!");

            descriptor.Validate();

            return new IndicatorContext(() => (AlgoPlugin)Activator.CreateInstance(descriptor.AlgoClassType), dataProvider);
        }
    }

    internal class IndicatorContext : PluginContext
    {
        internal IndicatorContext(AlgoPlugin pluginInstance, IPluginDataProvider provider, BuffersCoordinator coordinator)
            : base(pluginInstance, provider, coordinator)
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        internal IndicatorContext(Func<AlgoPlugin> pluginFactory, IPluginDataProvider provider)
            : base(pluginFactory, provider, new BuffersCoordinator())
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        private void InvokeCalculate()
        {
            ((Indicator)PluginInstance).InvokeCalculate();
        }

        public void Calculate()
        {
            for (int i = NestedIndicators.Count - 1; i >= 0; i--)
                NestedIndicators[i].InvokeCalculate();

            InvokeCalculate();
        }

        public override string ToString()
        {
            return "Indicator: " + Descriptor.DisplayName;
        }
    }
}
