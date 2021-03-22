using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Namespace = "TTAlgo.Config.v2")]
    public class PluginConfig
    {
        [DataMember(Name = "ModelTimeFrame")]
        private TimeFrames? _modelTimeframe;


        [DataMember(Name = "Key")]
        public PluginKey Key { get; set; }

        [DataMember(Name = "TimeFrame")]
        public TimeFrames TimeFrame { get; set; }

        [DataMember(Name = "MainSymbol")]
        public SymbolConfig MainSymbol { get; set; }

        [DataMember(Name = "Mapping")]
        public MappingKey SelectedMapping { get; set; }

        [DataMember(Name = "InstanceId")]
        public string InstanceId { get; set; }

        [DataMember(Name = "Permissions")]
        public PluginPermissions Permissions { get; set; }

        [DataMember(Name = "Properties")]
        public List<Property> Properties { get; internal set; }


        public TimeFrames ModelTimeFrame
        {
            get => _modelTimeframe.HasValue ? _modelTimeframe.Value : TimeFrames.Ticks;
            set => _modelTimeframe = value;
        }


        public PluginConfig()
        {
            Properties = new List<Property>();
        }


        public PluginConfig Clone()
        {
            return new PluginConfig
            {
                Key = Key.Clone(),
                TimeFrame = TimeFrame,
                ModelTimeFrame = ModelTimeFrame,
                MainSymbol = MainSymbol.Clone(),
                SelectedMapping = SelectedMapping.Clone(),
                InstanceId = InstanceId,
                Permissions = Permissions.Clone(),
                Properties = Properties.Select(p => p.Clone()).ToList(),
            };
        }


        public Domain.PluginConfig ToDomain()
        {
            var res = new Domain.PluginConfig
            {
                Key = Key.Convert(),
                Timeframe = TimeFrame.ToDomainEnum(),
                MainSymbol = MainSymbol.Convert(),
                SelectedMapping = SelectedMapping.Convert(),
                InstanceId = InstanceId,
                Permissions = Permissions.Convert(),
            };

            return res;
        }

        public static PluginConfig FromDomain(Domain.PluginConfig config)
        {
            var res = new PluginConfig
            {
                Key = config.Key.Convert(),
                TimeFrame = config.Timeframe.ToApiEnum(),
                MainSymbol = config.MainSymbol.Convert(),
                SelectedMapping = config.SelectedMapping.Convert(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.Convert(),
            };

            return res;
        }
    }


    internal static class ConfigExtensions
    {
        public static PackageKey ParsePackageKey(this string packageId)
        {
            PackageHelper.UnpackPackageId(packageId, out var locationId, out var packageName);

            RepositoryLocation location;
            switch (locationId)
            {
                case PackageHelper.EmbeddedRepositoryId:
                    location = RepositoryLocation.Embedded;
                    break;
                case PackageHelper.LocalRepositoryId:
                    location = RepositoryLocation.LocalRepository;
                    break;
                case PackageHelper.CommonRepositoryId:
                    location = RepositoryLocation.CommonRepository;
                    break;
                default:
                    throw new ArgumentException("Invalid location id");
            }
            return new PackageKey { Location = location, Name = packageName };
        }

        public static PluginKey Convert(this Domain.PluginKey plugin)
        {
            return new PluginKey(plugin.PackageId.ParsePackageKey(), plugin.DescriptorId);
        }

        public static ReductionKey Convert(this Domain.ReductionKey reduction)
        {
            return new ReductionKey(reduction.PackageId.ParsePackageKey(), reduction.DescriptorId);
        }

        public static MappingKey Convert(this Domain.MappingKey mapping)
        {
            return new MappingKey(mapping.PrimaryReduction.Convert(), mapping.SecondaryReduction?.Convert());
        }

        public static SymbolConfig Convert(this Domain.SymbolConfig symbol)
        {
            return new SymbolConfig { Name = symbol.Name, Origin = (SymbolOrigin)symbol.Origin };
        }

        public static PluginPermissions Convert(this Domain.PluginPermissions permissions)
        {
            return new PluginPermissions { Isolated = permissions.Isolated, TradeAllowed = permissions.TradeAllowed };
        }

        public static MarkerSizes Convert(this Domain.Metadata.Types.MarkerSize markerSize)
        {
            switch (markerSize)
            {
                case Domain.Metadata.Types.MarkerSize.Large: return MarkerSizes.Large;
                case Domain.Metadata.Types.MarkerSize.Medium: return MarkerSizes.Medium;
                case Domain.Metadata.Types.MarkerSize.Small: return MarkerSizes.Small;

                default: throw new ArgumentException($"Unsupported marker size {markerSize}");
            }
        }

        public static Property Config(this Domain.IPropertyConfig property)
        {
            switch (property)
            {
                case Domain.BoolParameterConfig par:
                    return new BoolParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.Int32ParameterConfig par:
                    return new IntParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.NullableInt32ParameterConfig par:
                    return new NullableIntParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.DoubleParameterConfig par:
                    return new DoubleParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.NullableDoubleParameterConfig par:
                    return new NullableDoubleParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.StringParameterConfig par:
                    return new StringParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.EnumParameterConfig par:
                    return new EnumParameter { Id = par.PropertyId, Value = par.Value };
                case Domain.FileParameterConfig par:
                    return new FileParameter { Id = par.PropertyId, FileName = par.FileName };

                case Domain.QuoteInputConfig input:
                    return new QuoteInput { Id = input.PropertyId, UseL2 = input.UseL2, SelectedSymbol = input.SelectedSymbol.Convert(), };
                case Domain.QuoteToBarInputConfig input:
                    return new QuoteToBarInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case Domain.QuoteToDoubleInputConfig input:
                    return new QuoteToDoubleInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case Domain.BarToBarInputConfig input:
                    return new BarToBarInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case Domain.BarToDoubleInputConfig input:
                    return new BarToDoubleInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };

                case Domain.ColoredLineOutputConfig output:
                    return new ColoredLineOutput { Id = output.PropertyId, IsEnabled = output.IsEnabled, LineColor = OutputColor.FromArgb(output.LineColorArgb), LineThickness = output.LineThickness, LineStyle = output.LineStyle.ToApiEnum() };
                case Domain.MarkerSeriesOutputConfig output:
                    return new MarkerSeriesOutput { Id = output.PropertyId, IsEnabled = output.IsEnabled, LineColor = OutputColor.FromArgb(output.LineColorArgb), LineThickness = output.LineThickness, MarkerSize = output.MarkerSize.Convert() };

                default:
                    return null;
            }
        }

        public static string Convert(this PackageKey package)
        {
            var locationId = string.Empty;
            switch(package.Location)
            {
                case RepositoryLocation.LocalRepository:
                case RepositoryLocation.LocalExtensions:
                    locationId = PackageHelper.LocalRepositoryId;
                    break;
                case RepositoryLocation.CommonRepository:
                case RepositoryLocation.CommonExtensions:
                    locationId = PackageHelper.CommonRepositoryId;
                    break;
            }
            return PackageHelper.GetPackageIdFromName(locationId, package.Name);
        }

        public static Domain.PluginKey Convert(this PluginKey plugin)
        {
            return new Domain.PluginKey(plugin.GetPackageKey().Convert(), plugin.DescriptorId);
        }

        public static Domain.ReductionKey Convert(this ReductionKey reduction)
        {
            return new Domain.ReductionKey(reduction.GetPackageKey().Convert(), reduction.DescriptorId);
        }

        public static Domain.MappingKey Convert(this MappingKey mapping)
        {
            return new Domain.MappingKey(mapping.PrimaryReduction.Convert(), mapping.SecondaryReduction?.Convert());
        }

        public static Domain.SymbolConfig Convert(this SymbolConfig symbol)
        {
            return new Domain.SymbolConfig { Name = symbol.Name, Origin = (Domain.SymbolConfig.Types.SymbolOrigin)symbol.Origin };
        }

        public static Domain.PluginPermissions Convert(this PluginPermissions permissions)
        {
            return new Domain.PluginPermissions { Isolated = permissions.Isolated, TradeAllowed = permissions.TradeAllowed };
        }

        public static Domain.Metadata.Types.MarkerSize Convert(this MarkerSizes markerSize)
        {
            switch (markerSize)
            {
                case MarkerSizes.Large: return Domain.Metadata.Types.MarkerSize.Large;
                case MarkerSizes.Medium: return Domain.Metadata.Types.MarkerSize.Medium;
                case MarkerSizes.Small: return Domain.Metadata.Types.MarkerSize.Small;

                default: throw new ArgumentException($"Unsupported marker size {markerSize}");
            }
        }

        public static Domain.IPropertyConfig Config(this Property property)
        {
            switch (property)
            {
                case BoolParameter par:
                    return new Domain.BoolParameterConfig { PropertyId = par.Id, Value = par.Value };
                case IntParameter par:
                    return new Domain.Int32ParameterConfig { PropertyId = par.Id, Value = par.Value };
                case NullableIntParameter par:
                    return new Domain.NullableInt32ParameterConfig { PropertyId = par.Id, Value = par.Value };
                case DoubleParameter par:
                    return new Domain.DoubleParameterConfig { PropertyId = par.Id, Value = par.Value };
                case NullableDoubleParameter par:
                    return new Domain.NullableDoubleParameterConfig { PropertyId = par.Id, Value = par.Value };
                case StringParameter par:
                    return new Domain.StringParameterConfig { PropertyId = par.Id, Value = par.Value };
                case EnumParameter par:
                    return new Domain.EnumParameterConfig { PropertyId = par.Id, Value = par.Value };
                case FileParameter par:
                    return new Domain.FileParameterConfig { PropertyId = par.Id, FileName = par.FileName };

                case QuoteInput input:
                    return new Domain.QuoteInputConfig { PropertyId = input.Id, UseL2 = input.UseL2, SelectedSymbol = input.SelectedSymbol.Convert(), };
                case QuoteToBarInput input:
                    return new Domain.QuoteToBarInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case QuoteToDoubleInput input:
                    return new Domain.QuoteToDoubleInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case BarToBarInput input:
                    return new Domain.BarToBarInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case BarToDoubleInput input:
                    return new Domain.BarToDoubleInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };

                case ColoredLineOutput output:
                    return new Domain.ColoredLineOutputConfig { PropertyId = output.Id, IsEnabled = output.IsEnabled, LineColorArgb = output.LineColor.ToArgb(), LineThickness = output.LineThickness, LineStyle = output.LineStyle.ToDomainEnum() };
                case MarkerSeriesOutput output:
                    return new Domain.MarkerSeriesOutputConfig { PropertyId = output.Id, IsEnabled = output.IsEnabled, LineColorArgb = output.LineColor.ToArgb(), LineThickness = output.LineThickness, MarkerSize = output.MarkerSize.Convert() };

                default:
                    return null;
            }
        }
    }
}
