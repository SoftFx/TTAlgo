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
        private List<PluginContext> nestedIndacators = new List<PluginContext>();
        private Dictionary<string, IDataSeriesProxy> inputs = new Dictionary<string, IDataSeriesProxy>();
        private Dictionary<string, IDataSeriesProxy> outputs = new Dictionary<string, IDataSeriesProxy>();

        internal PluginContext(AlgoPlugin plugin, IPluginDataProvider provider, BuffersCoordinator coordinator)
        {
            this.plugin = plugin;
            this.provider = provider;
            this.Coordinator = coordinator;
            SetThisAsActivator();
            Init();
        }

        protected void SetThisAsActivator()
        {
            AlgoPlugin.activator = this;
        }

        public BuffersCoordinator Coordinator { get; private set; }
        public AlgoPluginDescriptor Descriptor { get; private set; }

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

        public void InvokeInit()
        {
            foreach (var indicator in nestedIndacators)
                indicator.InvokeInit();

            SetThisAsActivator();
            plugin.InvokeInit();
        }

        private void Init()
        {
            Descriptor = AlgoPluginDescriptor.Get(plugin.GetType());
            Descriptor.Validate();
        }

        IPluginDataProvider IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (instance is Indicator)
            {
                var context = new IndicatorContext(instance, this.provider, Coordinator);
                nestedIndacators.Add(context);
                return this.provider;
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

        public static PluginContext Create(string id, IPluginDataProvider dataProvider)
        {
            AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);
            PluginFactory factory = new PluginFactory(descriptor.AlgoClassType, dataProvider);
            return factory.Create();
        }

        public static IndicatorContext CreateIndicator(string id, IPluginDataProvider dataProvider)
        {
            AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);

            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new InvalidPluginType("CreateIndicator() can be called only for indicators!");

            PluginFactory factory = new PluginFactory(descriptor.AlgoClassType, dataProvider);
            return (IndicatorContext)factory.Create();
        }
    }

    internal class IndicatorContext : PluginContext
    {
        internal IndicatorContext(AlgoPlugin plugin, IPluginDataProvider provider, BuffersCoordinator coordinator)
            : base(plugin, provider, coordinator)
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        public void InvokeCalculate()
        {
            ((Indicator)PluginInstance).InvokeCalculate();
        }
    }

    
}
