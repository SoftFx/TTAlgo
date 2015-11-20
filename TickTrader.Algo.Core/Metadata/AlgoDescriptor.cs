using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
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

        public AlgoDescriptor(Type algoCustomType)
        {
            this.AlgoClassType = algoCustomType;

            if (typeof(Indicator).IsAssignableFrom(algoCustomType))
                AlgoLogicType = AlgoTypes.Indicator;
            else
            {
                AlgoLogicType = AlgoTypes.Unknown;
                Error = AlgoMetadataErrors.UnknwonBaseType;
                return;
            }

            InspectProperties();

            if (allProperties.Any(p => p.PropertyType == AlgoPropertyTypes.Unknown))
                Error = AlgoMetadataErrors.HasUnknownProperties;
            else if (allProperties.Any(p => !p.IsValid))
                Error = AlgoMetadataErrors.HasInvalidProperties;
        }

        public AlgoInfo GetInteropCopy()
        {
            AlgoInfo copy = new AlgoInfo();
            copy.Id = this.AlgoClassType.FullName;
            copy.DisplayName = this.AlgoClassType.Name;
            copy.AlgoLogicType = this.AlgoLogicType;
            copy.Error = this.Error;

            copy.AllProperties = allProperties.Select(p => p.GetInteropCopy()).ToList();
            copy.Parameters = copy.AllProperties.OfType<ParameterInfo>().ToList();
            copy.Inputs = copy.AllProperties.OfType<InputInfo>().ToList();
            copy.Outputs = copy.AllProperties.OfType<OutputInfo>().ToList();

            return copy;
        }

        public Api.Algo CreateInstance()
        {
            return (Api.Algo)Activator.CreateInstance(AlgoClassType);
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
                var descriptor = new OutputDescriptor(this, property, algoAttribute);
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
                var descriptor = new ParameterDescriptor(this, property, algoAttribute);
                parameters.Add(descriptor);
                return descriptor;
            }

            throw new Exception("Unknwon property attribute!");
        }

        public Type AlgoClassType { get; private set; }
        public AlgoTypes AlgoLogicType { get; private set; }
        public AlgoMetadataErrors? Error { get; private set; }
        public bool IsValid { get { return Error == null; } }
        public IEnumerable<AlgoPropertyDescriptor> AllProperties { get { return allProperties; } }
        public IEnumerable<ParameterDescriptor> Parameters { get { return parameters; } }
        public IEnumerable<InputDescriptor> Inputs { get { return inputs; } }
        public IEnumerable<OutputDescriptor> Outputs { get { return outputs; } }
    }

    public enum AlgoTypes { Indicator, Robot, Unknown }
    public enum AlgoMetadataErrors { HasInvalidProperties, HasUnknownProperties, UnknwonBaseType }
}
