using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class AlgoDescriptor
    {
        private static Dictionary<Type, AlgoDescriptor> cacheByType = new Dictionary<Type, AlgoDescriptor>();
        private static Dictionary<string, AlgoDescriptor> cacheByName = new Dictionary<string, AlgoDescriptor>();

        public static AlgoDescriptor Get(Type algoCustomType)
        {
            AlgoDescriptor metadata;

            if (!cacheByType.TryGetValue(algoCustomType, out metadata))
            {
                metadata = new AlgoDescriptor(algoCustomType);
                cacheByType.Add(algoCustomType, metadata);
                cacheByName.Add(algoCustomType.FullName, metadata);
            }

            return metadata;
        }

        public static AlgoDescriptor Get(string algoCustomType)
        {
            AlgoDescriptor result;
            cacheByName.TryGetValue(algoCustomType, out result);
            return result;
        }

        public static IEnumerable<AlgoDescriptor> InspectAssembly(Assembly targetAssembly)
        {
            List<AlgoDescriptor> result = new List<AlgoDescriptor>();

            foreach (Type t in targetAssembly.GetTypes())
            {
                IndicatorAttribute indicatorAttr = t.GetCustomAttribute<IndicatorAttribute>(false);
                TradeBotAttribute botAttr = t.GetCustomAttribute<TradeBotAttribute>(false);

                if (indicatorAttr != null && botAttr != null)
                    continue;

                if (indicatorAttr != null)
                {
                    var meta = AlgoDescriptor.Get(t);
                    if (meta.AlgoLogicType == AlgoTypes.Indicator)
                        result.Add(meta);
                }
                else if (botAttr != null)
                {
                    var meta = AlgoDescriptor.Get(t);
                    if (meta.AlgoLogicType == AlgoTypes.Robot)
                        result.Add(meta);
                }
            }

            return result;
        }

        private List<AlgoPropertyDescriptor> allProperties = new List<AlgoPropertyDescriptor>();
        private List<ParameterDescriptor> parameters = new List<ParameterDescriptor>();
        private List<InputDescriptor> inputs = new List<InputDescriptor>();
        private List<OutputDescriptor> outputs = new List<OutputDescriptor>();
        private PluginFactory factory;

        public AlgoDescriptor(Type algoCustomType)
        {
            this.AlgoClassType = algoCustomType;

            if (typeof(Indicator).IsAssignableFrom(algoCustomType))
            {
                AlgoLogicType = AlgoTypes.Indicator;
                InspectIndicatorAttr();
            }
            else
            {
                AlgoLogicType = AlgoTypes.Unknown;
                Error = AlgoMetadataErrors.UnknwonBaseType;
                return;
            }

            InspectCopyrightAttr();
            InspectProperties();

            if (allProperties.Any(p => !p.IsValid))
                Error = AlgoMetadataErrors.HasInvalidProperties;

            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = algoCustomType.Name;

            this.factory = new PluginFactory(AlgoClassType);
            this.Id = AlgoClassType.FullName;
        }

        public IndicatorFixture CreateIndicator()
        {
            if (AlgoLogicType != AlgoTypes.Indicator)
                throw new InvalidPluginType("CreateIndicator() can be called only for indicators!");

            return factory.CreateIndicator();
        }

        public void Validate()
        {
            if (Error != null)
                throw new AlgoMetadataException(Error.Value, allProperties.Where(p => !p.IsValid));
        }

        private void InspectIndicatorAttr()
        {
            IndicatorAttribute indicatorAttr = AlgoClassType.GetCustomAttribute<IndicatorAttribute>(false);
            if (indicatorAttr != null)
            {
                DisplayName = indicatorAttr.DisplayName;
                Category = indicatorAttr.Category;
                IsOverlay = indicatorAttr.IsOverlay;
            }
        }

        private void InspectCopyrightAttr()
        {
            CopyrightAttribute copyrightAttr = AlgoClassType.GetCustomAttribute<CopyrightAttribute>(false);
            if (copyrightAttr != null)
                Copyright = copyrightAttr.Text;
        }

        private void InspectBotAttr()
        {
            TradeBotAttribute botAttr = AlgoClassType.GetCustomAttribute<TradeBotAttribute>(false);
            if (botAttr != null)
                DisplayName = botAttr.DisplayName;
        }

        private void InspectProperties()
        {
            var properties = AlgoClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var descriptor = BuildtPropertyDescriptor(property);
                if (descriptor != null)
                    this.allProperties.Add(descriptor);
            }
        }

        private AlgoPropertyDescriptor BuildtPropertyDescriptor(PropertyInfo property)
        {
            var algoAttributes = property.GetCustomAttributes(true).Where(
                    a => a is InputAttribute || a is OutputAttribute || a is ParameterAttribute).ToList();

            if (algoAttributes.Count == 0)
                return null;

            var algoAttribute = algoAttributes[0];

            if (algoAttributes.Count > 1)
                return new AlgoPropertyDescriptor(this, property, AlgoPropertyErrors.MultipleAttributes);
            else if (algoAttribute is OutputAttribute)
            {
                var descriptor = new OutputDescriptor(this, property, (OutputAttribute)algoAttribute);
                outputs.Add(descriptor);
                return descriptor;
            }
            else if (algoAttribute is InputAttribute)
            {
                var descriptor = new InputDescriptor(this, property, algoAttribute);
                inputs.Add(descriptor);
                return descriptor;
            }
            else if (algoAttribute is ParameterAttribute)
            {
                var descriptor = new ParameterDescriptor(this, property, (ParameterAttribute)algoAttribute);
                parameters.Add(descriptor);
                return descriptor;
            }

            throw new Exception("Unknwon property attribute!");
        }

        public string Id { get; private set; }
        public Type AlgoClassType { get; private set; }
        public string DisplayName { get; private set; }
        public string Category { get; private set; }
        public string Copyright { get; private set; }
        public AlgoTypes AlgoLogicType { get; private set; }
        public AlgoMetadataErrors? Error { get; private set; }
        public bool IsValid { get { return Error == null; } }
        public bool IsOverlay { get; private set; }
        public IEnumerable<AlgoPropertyDescriptor> AllProperties { get { return allProperties; } }
        public IEnumerable<ParameterDescriptor> Parameters { get { return parameters; } }
        public IEnumerable<InputDescriptor> Inputs { get { return inputs; } }
        public IEnumerable<OutputDescriptor> Outputs { get { return outputs; } }

        private class PluginFactory : NoTimeoutByRefObject, IPluginActivator
        {
            private Type pluginType;
            private PluginFixture fixture;

            public PluginFactory(Type algoType)
            {
                this.pluginType = algoType;
            }

            private void  CreateInstance()
            {
                try
                {
                    Api.AlgoPlugin.activator = this;
                    Activator.CreateInstance(pluginType);
                }
                finally
                {
                    Api.AlgoPlugin.activator = null;
                }
            }

            public IndicatorFixture CreateIndicator()
            {
                CreateInstance();
                return (IndicatorFixture)fixture;
            }

            IPluginContext IPluginActivator.Activate(AlgoPlugin instance)
            {
                if (instance is Indicator)
                {
                    fixture = new IndicatorFixture(instance);
                    return fixture;
                }

                return null;
            }
        }
    }

    public enum AlgoTypes { Indicator, Robot, Unknown }
    public enum AlgoMetadataErrors { HasInvalidProperties, UnknwonBaseType }
}
