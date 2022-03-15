using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Config
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
                Key = Key?.Convert(),
                Timeframe = TimeFrame.Convert(),
                ModelTimeframe = ModelTimeFrame.Convert(),
                MainSymbol = MainSymbol?.Convert(),
                SelectedMapping = SelectedMapping?.Convert(),
                InstanceId = InstanceId,
                Permissions = Permissions?.Convert(),
            };
            res.PackProperties(Properties.Select(p => p.Convert()).Where(u => u != null));

            return res;
        }

        public static PluginConfig FromDomain(Domain.PluginConfig config)
        {
            var res = new PluginConfig
            {
                Key = config.Key.Convert(),
                TimeFrame = config.Timeframe.Convert(),
                ModelTimeFrame = config.ModelTimeframe.Convert(),
                MainSymbol = config.MainSymbol.Convert(),
                SelectedMapping = config.SelectedMapping.Convert(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.Convert(),
            };
            res.Properties.AddRange(config.UnpackProperties().Select(p => p.Convert()).Where(u => u != null));

            return res;
        }


        #region Export/Import

        private readonly XmlWriterSettings XmlSettings = new XmlWriterSettings() { Indent = true };


        public static PluginConfig LoadFromFile(string filePath)
        {
            var serializer = new DataContractSerializer(typeof(PluginConfig));
            using (var stream = new FileStream(filePath, FileMode.Open))
                return (PluginConfig)serializer.ReadObject(stream);
        }


        public void SaveToFile(string filePath)
        {
            var serializer = new DataContractSerializer(typeof(PluginConfig));
            using (var stream = XmlWriter.Create(filePath, XmlSettings))
                serializer.WriteObject(stream, this);
        }

        #endregion
    }

    public enum TimeFrames
    {
        MN,
        D,
        W,
        H4,
        H1,
        M30,
        M15,
        M5,
        M1,
        S10,
        S1,
        Ticks,
        TicksLevel2
    }


    internal static class ConfigExtensions
    {
        public static PackageKey ParsePackageKey(this string packageId, bool isReduction)
        {
            PackageId.Unpack(packageId, out var pkgId);

            string location = pkgId.LocationId; // support for custom locations
            switch (pkgId.LocationId) // pre 1.19 compatibility
            {
                case SharedConstants.EmbeddedRepositoryId:
                    location = nameof(RepositoryLocation.Embedded);
                    break;
                case SharedConstants.LocalRepositoryId:
                    location = isReduction ? nameof(RepositoryLocation.LocalExtensions) : nameof(RepositoryLocation.LocalRepository);
                    break;
                case SharedConstants.CommonRepositoryId:
                    location = isReduction ? nameof(RepositoryLocation.CommonExtensions) : nameof(RepositoryLocation.CommonExtensions);
                    break;
            }
            return new PackageKey { Location = location, Name = pkgId.PackageName };
        }

        public static PluginKey Convert(this Domain.PluginKey plugin)
        {
            return new PluginKey(plugin.PackageId.ParsePackageKey(false), plugin.DescriptorId);
        }

        public static ReductionKey Convert(this Domain.ReductionKey reduction)
        {
            return new ReductionKey(reduction.PackageId.ParsePackageKey(true), reduction.DescriptorId);
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

        public static TimeFrames Convert(this Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Feed.Types.Timeframe.S1: return TimeFrames.S1;
                case Feed.Types.Timeframe.S10: return TimeFrames.S10;
                case Feed.Types.Timeframe.M1: return TimeFrames.M1;
                case Feed.Types.Timeframe.M5: return TimeFrames.M5;
                case Feed.Types.Timeframe.M15: return TimeFrames.M15;
                case Feed.Types.Timeframe.M30: return TimeFrames.M30;
                case Feed.Types.Timeframe.H1: return TimeFrames.H1;
                case Feed.Types.Timeframe.H4: return TimeFrames.H4;
                case Feed.Types.Timeframe.D: return TimeFrames.D;
                case Feed.Types.Timeframe.W: return TimeFrames.W;
                case Feed.Types.Timeframe.MN: return TimeFrames.MN;
                case Feed.Types.Timeframe.Ticks: return TimeFrames.Ticks;
                case Feed.Types.Timeframe.TicksLevel2: return TimeFrames.TicksLevel2;

                default: throw new ArgumentException($"Unsupported timeframe {timeframe}");
            }
        }

        public static LineStyles Convert(this Metadata.Types.LineStyle lineStyle)
        {
            switch (lineStyle)
            {
                case Metadata.Types.LineStyle.Solid: return LineStyles.Solid;
                case Metadata.Types.LineStyle.Dots: return LineStyles.Dots;
                case Metadata.Types.LineStyle.DotsRare: return LineStyles.DotsRare;
                case Metadata.Types.LineStyle.DotsVeryRare: return LineStyles.DotsVeryRare;
                case Metadata.Types.LineStyle.LinesDots: return LineStyles.LinesDots;
                case Metadata.Types.LineStyle.Lines: return LineStyles.Lines;

                default: throw new ArgumentException($"Unsupported line style {lineStyle}");
            }
        }

        public static MarkerSizes Convert(this Metadata.Types.MarkerSize markerSize)
        {
            switch (markerSize)
            {
                case Metadata.Types.MarkerSize.Large: return MarkerSizes.Large;
                case Metadata.Types.MarkerSize.Medium: return MarkerSizes.Medium;
                case Metadata.Types.MarkerSize.Small: return MarkerSizes.Small;

                default: throw new ArgumentException($"Unsupported marker size {markerSize}");
            }
        }

        public static Property Convert(this IPropertyConfig property)
        {
            switch (property)
            {
                case BoolParameterConfig par:
                    return new BoolParameter { Id = par.PropertyId, Value = par.Value };
                case Int32ParameterConfig par:
                    return new IntParameter { Id = par.PropertyId, Value = par.Value };
                case NullableInt32ParameterConfig par:
                    return new NullableIntParameter { Id = par.PropertyId, Value = par.Value };
                case DoubleParameterConfig par:
                    return new DoubleParameter { Id = par.PropertyId, Value = par.Value };
                case NullableDoubleParameterConfig par:
                    return new NullableDoubleParameter { Id = par.PropertyId, Value = par.Value };
                case StringParameterConfig par:
                    return new StringParameter { Id = par.PropertyId, Value = par.Value };
                case EnumParameterConfig par:
                    return new EnumParameter { Id = par.PropertyId, Value = par.Value };
                case FileParameterConfig par:
                    return new FileParameter { Id = par.PropertyId, FileName = par.FileName };

                case BarToBarInputConfig input:
                    return new BarToBarInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case BarToDoubleInputConfig input:
                    return new BarToDoubleInput { Id = input.PropertyId, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };

                case ColoredLineOutputConfig output:
                    return new ColoredLineOutput { Id = output.PropertyId, IsEnabled = output.IsEnabled, LineColor = OutputColor.FromArgb(output.LineColorArgb), LineThickness = output.LineThickness, LineStyle = output.LineStyle.Convert() };
                case MarkerSeriesOutputConfig output:
                    return new MarkerSeriesOutput { Id = output.PropertyId, IsEnabled = output.IsEnabled, LineColor = OutputColor.FromArgb(output.LineColorArgb), LineThickness = output.LineThickness, MarkerSize = output.MarkerSize.Convert() };

                default:
                    return null;
            }
        }

        public static string Convert(this PackageKey package)
        {
            var locationId = package.Location; // support for custom locations
            switch (package.Location) // pre 1.19 compatibility
            {
                case nameof(RepositoryLocation.Embedded):
                    locationId = SharedConstants.EmbeddedRepositoryId;
                    break;
                case nameof(RepositoryLocation.LocalRepository):
                case nameof(RepositoryLocation.LocalExtensions):
                    locationId = SharedConstants.LocalRepositoryId;
                    break;
                case nameof(RepositoryLocation.CommonRepository):
                case nameof(RepositoryLocation.CommonExtensions):
                    locationId = SharedConstants.CommonRepositoryId;
                    break;
            }
            return PackageId.Pack(locationId, package.Name);
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

        public static Feed.Types.Timeframe Convert(this TimeFrames timeframe)
        {
            switch (timeframe)
            {
                case TimeFrames.MN: return Feed.Types.Timeframe.MN;
                case TimeFrames.D: return Feed.Types.Timeframe.D;
                case TimeFrames.W: return Feed.Types.Timeframe.W;
                case TimeFrames.H4: return Feed.Types.Timeframe.H4;
                case TimeFrames.H1: return Feed.Types.Timeframe.H1;
                case TimeFrames.M30: return Feed.Types.Timeframe.M30;
                case TimeFrames.M15: return Feed.Types.Timeframe.M15;
                case TimeFrames.M5: return Feed.Types.Timeframe.M5;
                case TimeFrames.M1: return Feed.Types.Timeframe.M1;
                case TimeFrames.S10: return Feed.Types.Timeframe.S10;
                case TimeFrames.S1: return Feed.Types.Timeframe.S1;
                case TimeFrames.Ticks: return Feed.Types.Timeframe.Ticks;
                case TimeFrames.TicksLevel2: return Feed.Types.Timeframe.TicksLevel2;

                default: throw new ArgumentException($"Unsupported timeframe {timeframe}");
            }
        }

        public static Metadata.Types.LineStyle Convert(this LineStyles lineStyle)
        {
            switch (lineStyle)
            {
                case LineStyles.Solid: return Metadata.Types.LineStyle.Solid;
                case LineStyles.Dots: return Metadata.Types.LineStyle.Dots;
                case LineStyles.DotsRare: return Metadata.Types.LineStyle.DotsRare;
                case LineStyles.DotsVeryRare: return Metadata.Types.LineStyle.DotsVeryRare;
                case LineStyles.LinesDots: return Metadata.Types.LineStyle.LinesDots;
                case LineStyles.Lines: return Metadata.Types.LineStyle.Lines;

                default: throw new ArgumentException($"Unsupported line style {lineStyle}");
            }
        }

        public static Metadata.Types.MarkerSize Convert(this MarkerSizes markerSize)
        {
            switch (markerSize)
            {
                case MarkerSizes.Large: return Metadata.Types.MarkerSize.Large;
                case MarkerSizes.Medium: return Metadata.Types.MarkerSize.Medium;
                case MarkerSizes.Small: return Metadata.Types.MarkerSize.Small;

                default: throw new ArgumentException($"Unsupported marker size {markerSize}");
            }
        }

        public static IPropertyConfig Convert(this Property property)
        {
            switch (property)
            {
                case BoolParameter par:
                    return new BoolParameterConfig { PropertyId = par.Id, Value = par.Value };
                case IntParameter par:
                    return new Int32ParameterConfig { PropertyId = par.Id, Value = par.Value };
                case NullableIntParameter par:
                    return new NullableInt32ParameterConfig { PropertyId = par.Id, Value = par.Value };
                case DoubleParameter par:
                    return new DoubleParameterConfig { PropertyId = par.Id, Value = par.Value };
                case NullableDoubleParameter par:
                    return new NullableDoubleParameterConfig { PropertyId = par.Id, Value = par.Value };
                case StringParameter par:
                    return new StringParameterConfig { PropertyId = par.Id, Value = par.Value };
                case EnumParameter par:
                    return new EnumParameterConfig { PropertyId = par.Id, Value = par.Value };
                case FileParameter par:
                    return new FileParameterConfig { PropertyId = par.Id, FileName = par.FileName };

                case BarToBarInput input:
                    return new BarToBarInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };
                case BarToDoubleInput input:
                    return new BarToDoubleInputConfig { PropertyId = input.Id, SelectedSymbol = input.SelectedSymbol.Convert(), SelectedMapping = input.SelectedMapping.Convert(), };

                case ColoredLineOutput output:
                    return new ColoredLineOutputConfig { PropertyId = output.Id, IsEnabled = output.IsEnabled, LineColorArgb = output.LineColor.ToArgb(), LineThickness = output.LineThickness, LineStyle = output.LineStyle.Convert() };
                case MarkerSeriesOutput output:
                    return new MarkerSeriesOutputConfig { PropertyId = output.Id, IsEnabled = output.IsEnabled, LineColorArgb = output.LineColor.ToArgb(), LineThickness = output.LineThickness, MarkerSize = output.MarkerSize.Convert() };

                default:
                    return null;
            }
        }
    }
}
