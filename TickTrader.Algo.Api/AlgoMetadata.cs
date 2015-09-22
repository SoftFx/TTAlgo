using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal class AlgoMetadata
    {
        private static Dictionary<Type, AlgoMetadata> cache = new Dictionary<Type, AlgoMetadata>();

        public static AlgoMetadata Get(Type algoCustomType)
        {
            AlgoMetadata metadata;

            if (!cache.TryGetValue(algoCustomType, out metadata))
            {
                metadata = new AlgoMetadata(algoCustomType);
                cache.Add(algoCustomType, metadata);
            }

            return metadata;
        }

        private List<AlgoPropertyDescriptor> allProperties = new List<AlgoPropertyDescriptor>();
        private List<ParameterDescriptor> parameters = new List<ParameterDescriptor>();
        private List<InputDescriptor> inputs = new List<InputDescriptor>();
        private List<OutputDescriptor> outputs = new List<OutputDescriptor>();

        public AlgoMetadata(Type algoCustomType)
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

        private void InspectProperties()
        {
            var properties = AlgoClassType.GetAllAccessLevelProperties();
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
