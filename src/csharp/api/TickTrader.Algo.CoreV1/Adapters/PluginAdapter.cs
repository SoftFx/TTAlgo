using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal abstract class PluginAdapter : IPluginActivator
    {
        private AlgoPlugin _plugin;
        private IPluginContext _provider;
        private List<IndicatorAdapter> _nestedIndacators = new List<IndicatorAdapter>();
        private Dictionary<string, IDataSeriesProxy> _inputs = new Dictionary<string, IDataSeriesProxy>();
        private Dictionary<string, IDataSeriesProxy> _outputs = new Dictionary<string, IDataSeriesProxy>();

        internal PluginAdapter(AlgoPlugin plugnInstance, IPluginContext provider, BuffersCoordinator coordinator)
        {
            _provider = provider;
            _plugin = plugnInstance;
            Coordinator = coordinator;

            Metadata = new PluginMetadata(_plugin.GetType());
            Metadata.Validate();
        }

        internal PluginAdapter(PluginMetadata metadata, IPluginContext provider, BuffersCoordinator coordinator)
        {
            _provider = provider;
            Coordinator = coordinator;

            try
            {
                AlgoPlugin.activator = this;
                _plugin = (AlgoPlugin)metadata.CreateInstance();
            }
            finally
            {
                AlgoPlugin.activator = null;
            }

            Metadata = metadata;
            Metadata.Validate();
        }

        public BuffersCoordinator Coordinator { get; private set; }
        public PluginMetadata Metadata { get; private set; }
        protected IReadOnlyList<IndicatorAdapter> NestedIndicators { get { return _nestedIndacators; } }
        protected AlgoPlugin PluginInstance { get { return _plugin; } }

        public void SetParameter(string id, object val)
        {
            var paramMetadata = Metadata.Parameters.FirstOrDefault(p => p.Id == id);
            if (paramMetadata == null)
                throw new InvalidOperationException("Can't find parameter with id = " + id);
            paramMetadata.Set(_plugin, val);
        }

        public IDataSeriesProxy GetInput(string id)
        {
            return _inputs[id];
        }

        public DataSeriesImpl<T> GetInput<T>(string id)
        {
            return (DataSeriesImpl<T>)_inputs[id];
        }

        public IDataSeriesProxy GetOutput(string id)
        {
            IDataSeriesProxy proxy;
            if (!_outputs.TryGetValue(id, out proxy))
                throw new Exception("Output Not Found: " + id);
            return proxy;
        }

        public void InvokeInit()
        {
            try
            {
                AlgoPlugin.activator = this;

                for (int i = _nestedIndacators.Count - 1; i >= 0; i--)
                    _nestedIndacators[i]._plugin.InvokeInit(true);

                _plugin.InvokeInit(false);
            }
            finally
            {
                AlgoPlugin.activator = null;
            }
        }

        public void InvokeConnectedEvent()
        {
            var args = new ConnectedEventArgs();

            for (int i = _nestedIndacators.Count - 1; i >= 0; i--)
                _nestedIndacators[i]._plugin.InvokeConnected(args);

            _plugin.InvokeConnected(args);
        }

        public void InvokeDisconnectedEvent()
        {
            var args = new DisconnectedEventArgs();

            for (int i = _nestedIndacators.Count - 1; i >= 0; i--)
                _nestedIndacators[i]._plugin.InvokeDisconnected(args);

            _plugin.InvokeDisconnected(args);
        }

        IPluginContext IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (_plugin == null) // Activate() is called from constructor
                return _provider;

            if (instance is Indicator)
            {
                var context = new IndicatorAdapter(instance, _provider, Coordinator);
                _nestedIndacators.Add(context);
                return _provider;
            }

            return null;
        }

        protected void InitParameters()
        {
            foreach (var paramProperty in Metadata.Parameters)
            {
                if (paramProperty.DefaultValue != null)
                    paramProperty.ResetValue(_plugin);
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
            var input = CreateInput2<T>(d);
            input.Buffer = new EmptyBuffer<T>();
            d.Set(_plugin, input);
            _inputs.Add(d.Id, input);
        }

        public void BindOutput<T>(OutputMetadata d)
        {
            var output = CreateOutput2<T>(d);
            output.Buffer = OutputBuffer<T>.Create(Coordinator, d.IsHiddenEntity);
            d.Set(_plugin, output);
            _outputs.Add(d.Id, output);
        }

        private DataSeriesImpl<T> CreateInput2<T>(InputMetadata input)
        {
            var isShortDef = input.IsShortDefinition;
            if (typeof(T) == typeof(double) && isShortDef)
                return (DataSeriesImpl<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(DateTime) && isShortDef)
                return (DataSeriesImpl<T>)(object)new TimeSeriesProxy();
            else if (typeof(T) == typeof(Bar) && isShortDef)
                return (DataSeriesImpl<T>)(object)new BarSeriesProxy();
            else if (typeof(T) == typeof(Quote) && isShortDef)
                return (DataSeriesImpl<T>)(object)new QuoteSeriesProxy();
            else
                return new DataSeriesImpl<T>();
        }

        internal DataSeriesImpl<T> CreateOutput2<T>(OutputMetadata output)
        {
            var isShortDef = output.IsShortDefinition;
            if (typeof(T) == typeof(double) && isShortDef)
                return (DataSeriesImpl<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(Marker) && isShortDef)
                return (DataSeriesImpl<T>)(object)new MarkerSeriesProxy();
            else
                return new DataSeriesImpl<T>();
        }

        private void ReflectGenericMethod(Type genericType, string methodName, params object[] parameters)
        {
            MethodInfo method = GetType().GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(genericType);
            genericMethod.Invoke(this, parameters);
        }

        public static PluginAdapter Create(PluginMetadata metadata, IPluginContext dataProvider)
        {
            if (metadata.Descriptor.IsIndicator)
                return new IndicatorAdapter(metadata, dataProvider);
            else if (metadata.Descriptor.IsTradeBot)
                return new BotAdapter(metadata, dataProvider);
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
