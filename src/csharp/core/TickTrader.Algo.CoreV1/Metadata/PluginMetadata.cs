using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public class PluginMetadata
    {
        private Type _reflectionInfo;
        private Version _apiVersion;
        private List<PropertyMetadataBase> _allProperties = new List<PropertyMetadataBase>();
        private List<ParameterMetadata> _parameters = new List<ParameterMetadata>();
        private List<InputMetadata> _inputs = new List<InputMetadata>();
        private List<OutputMetadata> _outputs = new List<OutputMetadata>();


        public PluginDescriptor Descriptor { get; set; }

        public string Id => Descriptor.Id;

        public IEnumerable<PropertyMetadataBase> AllProperties => _allProperties;

        public IEnumerable<ParameterMetadata> Parameters => _parameters;

        public IEnumerable<InputMetadata> Inputs => _inputs;

        public IEnumerable<OutputMetadata> Outputs => _outputs;

        public PluginMetadata(Type reflectionInfo)
        {
            _reflectionInfo = reflectionInfo;

            Descriptor = new PluginDescriptor
            {
                Id = reflectionInfo.FullName,
                DisplayName = reflectionInfo.Name,
            };

            var refs = reflectionInfo.Assembly.GetReferencedAssemblies();
            var apiref = refs.FirstOrDefault(a => a.Name == "TickTrader.Algo.Api");
            _apiVersion = apiref?.Version;
            Descriptor.ApiVersionStr = apiref?.Version.ToString();

            if (typeof(Indicator).IsAssignableFrom(reflectionInfo))
            {
                Descriptor.Type = Domain.Metadata.Types.PluginType.Indicator;
                InspectIndicatorAttr(reflectionInfo);
            }
            else if (typeof(TradeBot).IsAssignableFrom(reflectionInfo))
            {
                Descriptor.Type = Domain.Metadata.Types.PluginType.TradeBot;
                InspectBotAttr(reflectionInfo);
            }
            else
            {
                Descriptor.Type = Domain.Metadata.Types.PluginType.UnknownPluginType;
                SetError(Domain.Metadata.Types.MetadataErrorCode.UnknownBaseType);
                return;
            }

            InspectCopyrightAttr(reflectionInfo);
            InspectProperties(reflectionInfo);

            if (_allProperties.Any(p => !p.IsValid))
                SetError(Domain.Metadata.Types.MetadataErrorCode.HasInvalidProperties);

            var currentApiVersion = typeof(Indicator).Assembly.GetName().Version;

            if (_apiVersion > currentApiVersion)
                SetError(Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiVersion);
        }


        public void Validate()
        {
            if (Descriptor.Error != Domain.Metadata.Types.MetadataErrorCode.NoMetadataError)
                throw new AlgoMetadataException(Descriptor.Error, _allProperties.Where(p => !p.IsValid));
        }

        public object CreateInstance()
        {
            if (_reflectionInfo == null)
                throw new Exception("This metadata does not belong to current AppDomain. Cannot set value!");

            return Activator.CreateInstance(_reflectionInfo);
        }


        private void SetError(Domain.Metadata.Types.MetadataErrorCode error)
        {
            if (Descriptor.IsValid)
                Descriptor.Error = error;
        }

        private void InspectCopyrightAttr(Type algoCustomType)
        {
            var copyrightAttr = algoCustomType.GetCustomAttribute<CopyrightAttribute>(false);
            if (copyrightAttr != null)
                Descriptor.Copyright = copyrightAttr.Text;
        }

        private void InspectIndicatorAttr(Type algoCustomType)
        {
            var indicatorAttr = algoCustomType.GetCustomAttribute<IndicatorAttribute>(false);
            if (indicatorAttr != null)
            {
                InspectAlgoPluginAttr(indicatorAttr);
            }
        }

        private void InspectBotAttr(Type algoCustomType)
        {
            var botAttr = algoCustomType.GetCustomAttribute<TradeBotAttribute>(false);
            if (botAttr != null)
            {
                InspectAlgoPluginAttr(botAttr);
            }
        }

        private void InspectAlgoPluginAttr(AlgoPluginAttribute pluginAttr)
        {
            if (pluginAttr != null)
            {
                var version = "";
                if (!string.IsNullOrWhiteSpace(pluginAttr.Version))
                    version = $" ({pluginAttr.Version})";
                if (!string.IsNullOrWhiteSpace(pluginAttr.DisplayName))
                    Descriptor.DisplayName = pluginAttr.DisplayName;
                Descriptor.UiDisplayName = $"{Descriptor.DisplayName}{version}";
                Descriptor.Category = string.IsNullOrWhiteSpace(pluginAttr.Category) ? "Misc" : pluginAttr.Category;
                Descriptor.Version = pluginAttr.Version;
                Descriptor.Description = pluginAttr.Description;
                Descriptor.SetupMainSymbol = pluginAttr.SetupMainSymbol;
            }
        }

        private void InspectProperties(Type algoCustomType)
        {
            var properties = algoCustomType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var metadata = BuildPropertyMetadata(property);
                if (metadata != null)
                    _allProperties.Add(metadata);
            }
        }

        private PropertyMetadataBase BuildPropertyMetadata(PropertyInfo property)
        {
            var algoAttributes = property.GetCustomAttributes(true).Where(
                    a => a is InputAttribute || a is OutputAttribute || a is ParameterAttribute).ToList();

            if (algoAttributes.Count == 0)
                return null;

            if (algoAttributes.Count > 1)
                return new PropertyMetadata(property, Domain.Metadata.Types.PropertyErrorCode.MultipleAttributes);

            var algoAttribute = algoAttributes[0];

            if (algoAttribute is OutputAttribute)
            {
                var metadata = new OutputMetadata(property, (OutputAttribute)algoAttribute);
                _outputs.Add(metadata);
                Descriptor.Outputs.Add(metadata.Descriptor);
                return metadata;
            }
            else if (algoAttribute is InputAttribute)
            {
                var metadata = new InputMetadata(property, (InputAttribute)algoAttribute);
                _inputs.Add(metadata);
                Descriptor.Inputs.Add(metadata.Descriptor);
                return metadata;
            }
            else if (algoAttribute is ParameterAttribute)
            {
                var metadata = new ParameterMetadata(property, (ParameterAttribute)algoAttribute);
                _parameters.Add(metadata);
                Descriptor.Parameters.Add(metadata.Descriptor);
                return metadata;
            }

            throw new Exception("Unknwon property attribute!");
        }
    }
}
