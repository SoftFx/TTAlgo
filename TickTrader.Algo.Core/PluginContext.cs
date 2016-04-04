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
    /// <summary>
    /// Note: PluginContext is located in separate domain in case of isolated execution
    /// </summary>
    public class PluginContext : NoTimeoutByRefObject, IPluginActivator, IPluginDataProvider
    {
        private AlgoPlugin plugin;
        private List<PluginContext> nestedIndacators = new List<PluginContext>();
        private Dictionary<string, DataSeriesBuffer> inputs = new Dictionary<string, DataSeriesBuffer>();
        private Dictionary<string, DataSeriesBuffer> outputs = new Dictionary<string, DataSeriesBuffer>();
        private int virtualPos;

        internal PluginContext(AlgoPlugin plugin)
        {
            this.plugin = plugin;
            SetThisAsActivator();
            Init();
        }

        protected void SetThisAsActivator()
        {
            AlgoPlugin.activator = this;
        }

        public AlgoPluginDescriptor Descriptor { get; private set; }
        public int VirtualPos { get { return virtualPos; } }

        protected AlgoPlugin PluginInstance { get { return plugin; } }

        public void SetParameter(string id, object val)
        {
            var paramDescriptor = Descriptor.Parameters.FirstOrDefault(p => p.Id == id);
            if (paramDescriptor == null)
                throw new InvalidOperationException("Can't find parameter with id = " + id);
            paramDescriptor.Set(plugin, val);
        }

        public IDataSeriesBuffer GetInput(string id)
        {
            return inputs[id];
        }

        public IDataSeriesBuffer GetOutput(string id)
        {
            return outputs[id];
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveNext()
        {
            foreach (var indicator in nestedIndacators)
                indicator.MoveNext();

            foreach (var input in inputs.Values)
                input.IncrementVirtualSize();

            foreach (var output in outputs.Values)
                output.IncrementVirtualSize();

            virtualPos++;
        }

        public void Reset()
        {
            foreach (var indicator in nestedIndacators)
                indicator.MoveNext();

            foreach (var input in inputs.Values)
                input.Reset();

            foreach (var output in outputs.Values)
                output.Reset();

            virtualPos = 0;
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
                var fixture = new IndicatorContext(instance);
                nestedIndacators.Add(fixture);
                return fixture;
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
            var input = d.CreateInput<T>();
            d.Set(plugin, input);
            inputs.Add(d.Id, input);
        }

        public void BindOutput<T>(OutputDescriptor d)
        {
            var output = d.CreateOutput<T>();
            d.Set(plugin, output);
            outputs.Add(d.Id, output);
        }

        private void ReflectGenericMethod(Type genericType, string methodName, params object[] parameters)
        {
            MethodInfo method = GetType().GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(genericType);
            genericMethod.Invoke(this, parameters);
        }

        public OrderList GetOrdersCollection()
        {
            throw new NotImplementedException();
        }

        public PositionList GetPositionsCollection()
        {
            throw new NotImplementedException();
        }

        public MarketSeries GetMainMarketSeries()
        {
            throw new NotImplementedException();
        }

        public Leve2Series GetMainLevel2Series()
        {
            throw new NotImplementedException();
        }

        private static AlgoPluginDescriptor GetDescriptorOrThrow(string id)
        {
            AlgoPluginDescriptor descriptor = AlgoPluginDescriptor.Get(id);

            if (descriptor == null)
                throw new ArgumentException("Cannot find plugin descriptor: " + id);

            return descriptor;
        }

        public static PluginContext Create(string id)
        {
            AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);
            PluginFactory factory = new PluginFactory(descriptor.AlgoClassType);
            return factory.Create();
        }

        public static IndicatorContext CreateIndicator(string id)
        {
            AlgoPluginDescriptor descriptor = GetDescriptorOrThrow(id);

            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new InvalidPluginType("CreateIndicator() can be called only for indicators!");

            PluginFactory factory = new PluginFactory(descriptor.AlgoClassType);
            return (IndicatorContext)factory.Create();
        }
    }

    public class IndicatorContext : PluginContext
    {
        internal IndicatorContext(AlgoPlugin plugin)
            : base(plugin)
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
