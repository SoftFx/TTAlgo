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
    public class PluginFixture : NoTimeoutByRefObject, IPluginActivator, IPluginContext
    {
        private AlgoPlugin plugin;
        private List<PluginFixture> nestedIndacators = new List<PluginFixture>();
        private Dictionary<string, DataSeriesBuffer> inputs = new Dictionary<string, DataSeriesBuffer>();
        private Dictionary<string, DataSeriesBuffer> outputs = new Dictionary<string, DataSeriesBuffer>();
        private int virtualPos;

        internal PluginFixture(AlgoPlugin plugin)
        {
            this.plugin = plugin;
            SetThisAsActivator();
            Init();
        }

        protected void SetThisAsActivator()
        {
            AlgoPlugin.activator = this;
        }

        public AlgoDescriptor Descriptor { get; private set; }
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
            Descriptor = AlgoDescriptor.Get(plugin.GetType());
            Descriptor.Validate();
        }

        IPluginContext IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (instance is Indicator)
            {
                var fixture = new IndicatorFixture(instance);
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
    }

    public class IndicatorFixture : PluginFixture
    {
        public IndicatorFixture(AlgoPlugin plugin)
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
