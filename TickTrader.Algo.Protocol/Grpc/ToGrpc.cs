using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Protocol.Grpc
{
    internal static class ToGrpc
    {
        public static string Convert(string s) // because grpc doesn't like null strings
        {
            return s ?? string.Empty;
        }

        public static ByteString Convert(this byte[] bytes)
        {
            return ByteString.CopyFrom(bytes);
        }

        public static ByteString Convert(this byte[] bytes, int offset, int count)
        {
            return ByteString.CopyFrom(bytes, offset, count);
        }

        public static Timestamp Convert(this DateTime date)
        {
            return Timestamp.FromDateTime(date);
        }


        #region descriptors.proto

        public static Lib.ApiDescriptor.Types.TimeFrame Convert(this TimeFrames timeFrame)
        {
            switch (timeFrame)
            {
                case TimeFrames.MN:
                    return Lib.ApiDescriptor.Types.TimeFrame.MN;
                case TimeFrames.D:
                    return Lib.ApiDescriptor.Types.TimeFrame.D;
                case TimeFrames.W:
                    return Lib.ApiDescriptor.Types.TimeFrame.W;
                case TimeFrames.H4:
                    return Lib.ApiDescriptor.Types.TimeFrame.H4;
                case TimeFrames.H1:
                    return Lib.ApiDescriptor.Types.TimeFrame.H1;
                case TimeFrames.M30:
                    return Lib.ApiDescriptor.Types.TimeFrame.M30;
                case TimeFrames.M15:
                    return Lib.ApiDescriptor.Types.TimeFrame.M15;
                case TimeFrames.M5:
                    return Lib.ApiDescriptor.Types.TimeFrame.M5;
                case TimeFrames.M1:
                    return Lib.ApiDescriptor.Types.TimeFrame.M1;
                case TimeFrames.S10:
                    return Lib.ApiDescriptor.Types.TimeFrame.S10;
                case TimeFrames.S1:
                    return Lib.ApiDescriptor.Types.TimeFrame.S1;
                case TimeFrames.Ticks:
                    return Lib.ApiDescriptor.Types.TimeFrame.Ticks;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ApiDescriptor.Types.LineStyle Convert(this LineStyles style)
        {
            switch (style)
            {
                case LineStyles.Solid:
                    return Lib.ApiDescriptor.Types.LineStyle.Solid;
                case LineStyles.Dots:
                    return Lib.ApiDescriptor.Types.LineStyle.Dots;
                case LineStyles.DotsRare:
                    return Lib.ApiDescriptor.Types.LineStyle.DotsRare;
                case LineStyles.DotsVeryRare:
                    return Lib.ApiDescriptor.Types.LineStyle.DotsVeryRare;
                case LineStyles.LinesDots:
                    return Lib.ApiDescriptor.Types.LineStyle.LinesDots;
                case LineStyles.Lines:
                    return Lib.ApiDescriptor.Types.LineStyle.Lines;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ApiDescriptor.Types.PlotType Convert(this PlotType type)
        {
            switch (type)
            {
                case PlotType.Line:
                    return Lib.ApiDescriptor.Types.PlotType.Line;
                case PlotType.Histogram:
                    return Lib.ApiDescriptor.Types.PlotType.Histogram;
                case PlotType.Points:
                    return Lib.ApiDescriptor.Types.PlotType.Points;
                case PlotType.DiscontinuousLine:
                    return Lib.ApiDescriptor.Types.PlotType.DiscontinuousLine;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ApiDescriptor.Types.OutputTarget Convert(this OutputTargets target)
        {
            switch (target)
            {
                case OutputTargets.Overlay:
                    return Lib.ApiDescriptor.Types.OutputTarget.Overlay;
                case OutputTargets.Window1:
                    return Lib.ApiDescriptor.Types.OutputTarget.Window1;
                case OutputTargets.Window2:
                    return Lib.ApiDescriptor.Types.OutputTarget.Window2;
                case OutputTargets.Window3:
                    return Lib.ApiDescriptor.Types.OutputTarget.Window3;
                case OutputTargets.Window4:
                    return Lib.ApiDescriptor.Types.OutputTarget.Window4;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ApiDescriptor.Types.MarkerSize Convert(this MarkerSizes markerSize)
        {
            switch (markerSize)
            {
                case MarkerSizes.Large:
                    return Lib.ApiDescriptor.Types.MarkerSize.Large;
                case MarkerSizes.Medium:
                    return Lib.ApiDescriptor.Types.MarkerSize.Medium;
                case MarkerSizes.Small:
                    return Lib.ApiDescriptor.Types.MarkerSize.Small;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PropertyDescriptor.Types.AlgoPropertyType Convert(this AlgoPropertyTypes type)
        {
            switch (type)
            {
                case AlgoPropertyTypes.Unknown:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyType.UnknownPropertyType;
                case AlgoPropertyTypes.Parameter:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyType.Parameter;
                case AlgoPropertyTypes.InputSeries:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyType.InputSeries;
                case AlgoPropertyTypes.OutputSeries:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyType.OutputSeries;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PropertyDescriptor.Types.AlgoPropertyError Convert(this AlgoPropertyErrors error)
        {
            switch (error)
            {
                case AlgoPropertyErrors.None:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.None;
                case AlgoPropertyErrors.Unknown:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.UnknownPropertyError;
                case AlgoPropertyErrors.SetIsNotPublic:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.SetIsNotPublic;
                case AlgoPropertyErrors.GetIsNotPublic:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.GetIsNotPublic;
                case AlgoPropertyErrors.MultipleAttributes:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.MultipleAttributes;
                case AlgoPropertyErrors.InputIsNotDataSeries:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.InputIsNotDataSeries;
                case AlgoPropertyErrors.OutputIsNotDataSeries:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.OutputIsNotDataSeries;
                case AlgoPropertyErrors.EmptyEnum:
                    return Lib.PropertyDescriptor.Types.AlgoPropertyError.EmptyEnum;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PropertyDescriptor Convert(this PropertyDescriptor property)
        {
            return new Lib.PropertyDescriptor
            {
                Id = Convert(property.Id),
                DisplayName = Convert(property.DisplayName),
                PropertyType = property.PropertyType.Convert(),
                Error = property.Error.Convert(),
            };
        }

        public static Lib.FileFilterEntry Convert(this FileFilterEntry entry)
        {
            return new Lib.FileFilterEntry
            {
                FileTypeName = Convert(entry.FileTypeName),
                FileMask = Convert(entry.FileMask),
            };
        }

        public static Lib.ParameterDescriptor Convert(this ParameterDescriptor parameter)
        {
            var res = new Lib.ParameterDescriptor
            {
                PropertyHeader = ((PropertyDescriptor)parameter).Convert(),
                DataType = Convert(parameter.DataType),
                DefaultValue = Convert(parameter.DefaultValue),
                IsRequired = parameter.IsRequired,
                IsEnum = parameter.IsEnum,
            };
            res.EnumValues.AddRange(parameter.EnumValues);
            res.FileFilters.AddRange(parameter.FileFilters.Select(Convert));
            return res;
        }

        public static Lib.InputDescriptor Convert(this InputDescriptor input)
        {
            return new Lib.InputDescriptor
            {
                PropertyHeader = ((PropertyDescriptor)input).Convert(),
                DataSeriesBaseTypeFullName = Convert(input.DataSeriesBaseTypeFullName),
            };
        }

        public static Lib.OutputDescriptor Convert(this OutputDescriptor output)
        {
            return new Lib.OutputDescriptor
            {
                PropertyHeader = ((PropertyDescriptor)output).Convert(),
                DataSeriesBaseTypeFullName = Convert(output.DataSeriesBaseTypeFullName),
                DefaultThickness = output.DefaultThickness,
                DefaultColor = (int)output.DefaultColor,
                DefaultLineStyle = output.DefaultLineStyle.Convert(),
                PlotType = output.PlotType.Convert(),
                Target = output.Target.Convert(),
                Precision = output.Precision,
                ZeroLine = output.ZeroLine,
                Visibility = output.Visibility,
            };
        }

        public static Lib.PluginDescriptor.Types.AlgoType Convert(this AlgoTypes type)
        {
            switch (type)
            {
                case AlgoTypes.Unknown:
                    return Lib.PluginDescriptor.Types.AlgoType.UnknownPluginType;
                case AlgoTypes.Indicator:
                    return Lib.PluginDescriptor.Types.AlgoType.Indicator;
                case AlgoTypes.Robot:
                    return Lib.PluginDescriptor.Types.AlgoType.Robot;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PluginDescriptor.Types.AlgoMetadataError Convert(this AlgoMetadataErrors error)
        {
            switch (error)
            {
                case AlgoMetadataErrors.None:
                    return Lib.PluginDescriptor.Types.AlgoMetadataError.None;
                case AlgoMetadataErrors.Unknown:
                    return Lib.PluginDescriptor.Types.AlgoMetadataError.UnknownMetadataError;
                case AlgoMetadataErrors.HasInvalidProperties:
                    return Lib.PluginDescriptor.Types.AlgoMetadataError.HasInvalidProperties;
                case AlgoMetadataErrors.UnknownBaseType:
                    return Lib.PluginDescriptor.Types.AlgoMetadataError.UnknownBaseType;
                case AlgoMetadataErrors.IncompatibleApiVersion:
                    return Lib.PluginDescriptor.Types.AlgoMetadataError.IncompatibleApiVersion;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PluginDescriptor ConvertLight(this PluginDescriptor plugin)
        {
            var res = new Lib.PluginDescriptor
            {
                ApiVersionStr = Convert(plugin.ApiVersionStr),
                Id = Convert(plugin.Id),
                DisplayName = Convert(plugin.DisplayName),
                Type = plugin.Type.Convert(),
                Error = plugin.Error.Convert(),
                UiDisplayName = Convert(plugin.UiDisplayName),
                Category = Convert(plugin.Category),
                Version = Convert(plugin.Version),
                Description = Convert(plugin.Description),
                Copyright = Convert(plugin.Copyright),
                SetupMainSymbol = plugin.SetupMainSymbol,
            };
            return res;
        }

        public static Lib.PluginDescriptor Convert(this PluginDescriptor plugin)
        {
            var res = plugin.ConvertLight();
            res.Parameters.AddRange(plugin.Parameters.Select(Convert));
            res.Inputs.AddRange(plugin.Inputs.Select(Convert));
            res.Outputs.AddRange(plugin.Outputs.Select(Convert));
            return res;
        }

        public static Lib.ReductionDescriptor.Types.ReductionType Convert(this ReductionType type)
        {
            switch (type)
            {
                case ReductionType.Unknown:
                    return Lib.ReductionDescriptor.Types.ReductionType.UnknownReductionType;
                case ReductionType.BarToDouble:
                    return Lib.ReductionDescriptor.Types.ReductionType.BarToDouble;
                case ReductionType.FullBarToDouble:
                    return Lib.ReductionDescriptor.Types.ReductionType.FullBarToDouble;
                case ReductionType.FullBarToBar:
                    return Lib.ReductionDescriptor.Types.ReductionType.FullBarToBar;
                case ReductionType.QuoteToDouble:
                    return Lib.ReductionDescriptor.Types.ReductionType.QuoteToDouble;
                case ReductionType.QuoteToBar:
                    return Lib.ReductionDescriptor.Types.ReductionType.QuoteToBar;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ReductionDescriptor Convert(this ReductionDescriptor reduction)
        {
            return new Lib.ReductionDescriptor
            {
                ApiVersionStr = Convert(reduction.ApiVersionStr),
                Id = Convert(reduction.Id),
                DisplayName = Convert(reduction.DisplayName),
                Type = reduction.Type.Convert(),
            };
        }

        #endregion descriptors.proto


        #region config.proto

        public static Lib.Property Convert(this Property property, string mainSymbol, VersionSpec version = null)
        {
            var res = new Lib.Property
            {
                PropertyId = Convert(property.Id),
            };
            switch (property)
            {
                case Parameter parameter:
                    res.Parameter = parameter.Convert();
                    break;
                case Input input:
                    res.Input = input.Convert(version, mainSymbol);
                    break;
                case Output output:
                    res.Output = output.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.Parameter Convert(this Parameter parameter)
        {
            var res = new Lib.Parameter();
            switch (parameter)
            {
                case BoolParameter boolParam:
                    res.Bool = boolParam.Convert();
                    break;
                case IntParameter intParam:
                    res.Int = intParam.Convert();
                    break;
                case NullableIntParameter nullIntParam:
                    res.NullInt = nullIntParam.Convert();
                    break;
                case DoubleParameter doubleParam:
                    res.Double = doubleParam.Convert();
                    break;
                case NullableDoubleParameter nullDoubleParam:
                    res.NullDouble = nullDoubleParam.Convert();
                    break;
                case StringParameter stringParam:
                    res.String = stringParam.Convert();
                    break;
                case EnumParameter enumParam:
                    res.Enum = enumParam.Convert();
                    break;
                case FileParameter fileParam:
                    res.File = fileParam.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.BoolParameter Convert(this BoolParameter param)
        {
            return new Lib.BoolParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.IntParameter Convert(this IntParameter param)
        {
            return new Lib.IntParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.NullableIntParameter Convert(this NullableIntParameter param)
        {
            return new Lib.NullableIntParameter
            {
                Value = param.Value.HasValue ? new Int32Value { Value = param.Value.Value } : null,
            };
        }

        public static Lib.DoubleParameter Convert(this DoubleParameter param)
        {
            return new Lib.DoubleParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.NullableDoubleParameter Convert(this NullableDoubleParameter param)
        {
            return new Lib.NullableDoubleParameter
            {
                Value = param.Value.HasValue ? new DoubleValue { Value = param.Value.Value } : null,
            };
        }

        public static Lib.StringParameter Convert(this StringParameter param)
        {
            return new Lib.StringParameter
            {
                Value = Convert(param.Value),
            };
        }

        public static Lib.EnumParameter Convert(this EnumParameter param)
        {
            return new Lib.EnumParameter
            {
                Value = Convert(param.Value),
            };
        }

        public static Lib.FileParameter Convert(this FileParameter param)
        {
            return new Lib.FileParameter
            {
                FileName = Convert(System.IO.Path.GetFileName(param.FileName)),
            };
        }

        public static Lib.SymbolConfig.Types.SymbolOrigin Convert(this SymbolOrigin markerSize)
        {
            switch (markerSize)
            {
                case SymbolOrigin.Online:
                    return Lib.SymbolConfig.Types.SymbolOrigin.Online;
                case SymbolOrigin.Custom:
                    return Lib.SymbolConfig.Types.SymbolOrigin.Custom;
                case SymbolOrigin.Token:
                    return Lib.SymbolConfig.Types.SymbolOrigin.Special;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.SymbolConfig Convert(this SymbolConfig config, VersionSpec version, string mainSymbol = "")
        {
            var name = config.Name;

            if (config.Origin == SymbolOrigin.Token)
            {
                if (version != null && version.SupportMainToken)
                    name = mainSymbol;
                else
                    throw new UnsupportedException($"\"{SpecialSymbols.MainSymbol}\" token not supported by current agent version");
            }

            return new Lib.SymbolConfig
            {
                Name = Convert(name),
                Origin = config.Origin.Convert(),
            };
        }

        public static Lib.Input Convert(this Input input, VersionSpec version, string mainSymbol = "")
        {
            var res = new Lib.Input
            {
                SelectedSymbol = input.SelectedSymbol.Convert(version, mainSymbol),
            };
            switch (input)
            {
                case QuoteInput quoteInput:
                    res.Quote = quoteInput.Convert();
                    break;
                case MappedInput mappedInput:
                    res.Mapped = mappedInput.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.QuoteInput Convert(this QuoteInput input)
        {
            return new Lib.QuoteInput
            {
                UseL2 = input.UseL2,
            };
        }

        public static Lib.MappedInput Convert(this MappedInput input)
        {
            var res = new Lib.MappedInput
            {
                SelectedMapping = input.SelectedMapping.Convert(),
            };
            switch (input)
            {
                case BarToBarInput barToBar:
                    res.BarToBar = barToBar.Convert();
                    break;
                case BarToDoubleInput barToDouble:
                    res.BarToDouble = barToDouble.Convert();
                    break;
                case QuoteToBarInput quoteToBar:
                    res.QuoteToBar = quoteToBar.Convert();
                    break;
                case QuoteToDoubleInput quoteToDouble:
                    res.QuoteToDouble = quoteToDouble.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.BarToBarInput Convert(this BarToBarInput input)
        {
            return new Lib.BarToBarInput();
        }

        public static Lib.BarToDoubleInput Convert(this BarToDoubleInput input)
        {
            return new Lib.BarToDoubleInput();
        }

        public static Lib.QuoteToBarInput Convert(this QuoteToBarInput input)
        {
            return new Lib.QuoteToBarInput();
        }

        public static Lib.QuoteToDoubleInput Convert(this QuoteToDoubleInput input)
        {
            return new Lib.QuoteToDoubleInput();
        }

        public static Lib.OutputColor Convert(this OutputColor color)
        {
            return new Lib.OutputColor
            {
                Alpha = color.Alpha,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
            };
        }

        public static Lib.Output Convert(this Output output)
        {
            var res = new Lib.Output
            {
                IsEnabled = output.IsEnabled,
                LineColor = output.LineColor.Convert(),
                LineThickness = output.LineThickness,
            };
            switch (output)
            {
                case ColoredLineOutput coloredLine:
                    res.ColoredLine = coloredLine.Convert();
                    break;
                case MarkerSeriesOutput markerSeries:
                    res.MarkerSeries = markerSeries.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.ColoredLineOutput Convert(this ColoredLineOutput output)
        {
            return new Lib.ColoredLineOutput
            {
                LineStyle = output.LineStyle.Convert(),
            };
        }

        public static Lib.MarkerSeriesOutput Convert(this MarkerSeriesOutput output)
        {
            return new Lib.MarkerSeriesOutput
            {
                MarkerSize = output.MarkerSize.Convert(),
            };
        }

        public static Lib.PluginPermissions Convert(this PluginPermissions permissions)
        {
            return new Lib.PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static Lib.PluginConfig Convert(this PluginConfig config, VersionSpec version = null)
        {
            var res = new Lib.PluginConfig
            {
                Key = config.Key.Convert(),
                TimeFrame = config.TimeFrame.Convert(),
                MainSymbol = config.MainSymbol.Convert(version),
                SelectedMapping = config.SelectedMapping.Convert(),
                InstanceId = Convert(config.InstanceId),
                Permissions = config.Permissions.Convert(),
            };
            res.Properties.AddRange(config.Properties.Select(u => u.Convert(res.MainSymbol.Name, version)));
            return res;
        }

        #endregion config.proto

        #region keys.proto

        public static Lib.AccountKey Convert(this AccountKey key)
        {
            return new Lib.AccountKey
            {
                Login = Convert(key.Login),
                Server = Convert(key.Server),
            };
        }

        public static Lib.RepositoryLocation Convert(this RepositoryLocation location)
        {
            switch (location)
            {
                case RepositoryLocation.Embedded:
                    return Lib.RepositoryLocation.Embedded;
                case RepositoryLocation.LocalRepository:
                    return Lib.RepositoryLocation.LocalRepository;
                case RepositoryLocation.LocalExtensions:
                    return Lib.RepositoryLocation.LocalExtensions;
                case RepositoryLocation.CommonRepository:
                    return Lib.RepositoryLocation.CommonRepository;
                case RepositoryLocation.CommonExtensions:
                    return Lib.RepositoryLocation.CommonExtensions;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.PackageKey Convert(this PackageKey key)
        {
            return new Lib.PackageKey
            {
                Name = Convert(key.Name),
                Location = key.Location.Convert(),
            };
        }

        public static Lib.PluginKey Convert(this PluginKey key)
        {
            return new Lib.PluginKey
            {
                PackageName = Convert(key.PackageName),
                PackageLocation = key.PackageLocation.Convert(),
                DescriptorId = Convert(key.DescriptorId),
            };
        }

        public static Lib.ReductionKey Convert(this ReductionKey key)
        {
            return new Lib.ReductionKey
            {
                PackageName = Convert(key.PackageName),
                PackageLocation = key.PackageLocation.Convert(),
                DescriptorId = Convert(key.DescriptorId),
            };
        }

        public static Lib.MappingKey Convert(this MappingKey key)
        {
            return new Lib.MappingKey
            {
                PrimaryReduction = key.PrimaryReduction.Convert(),
                SecondaryReduction = key.SecondaryReduction?.Convert(),
            };
        }

        #endregion keys.proto

        #region metadata.proto

        public static Lib.PluginInfo Convert(this PluginInfo plugin)
        {
            return new Lib.PluginInfo
            {
                Key = plugin.Key.Convert(),
                Descriptor_ = plugin.Descriptor.Convert(),
            };
        }

        public static Lib.PackageIdentity Convert(this PackageIdentity identity)
        {
            return new Lib.PackageIdentity
            {
                FileName = Convert(identity.FileName),
                FilePath = Convert(identity.FilePath),
                CreatedUtc = identity.CreatedUtc.Convert(),
                LastModifiedUtc = identity.LastModifiedUtc.Convert(),
                Size = identity.Size,
                Hash = Convert(identity.Hash),
            };
        }

        public static Lib.PackageInfo ConvertLight(this PackageInfo package)
        {
            var res = new Lib.PackageInfo
            {
                Key = package.Key.Convert(),
                IsValid = package.IsValid,
                IsLocked = package.IsLocked,
                IsObsolete = package.IsObsolete,
            };
            return res;
        }

        public static Lib.PackageInfo Convert(this PackageInfo package)
        {
            var res = package.ConvertLight();
            res.Identity = package.Identity.Convert();
            res.Plugins.AddRange(package.Plugins.Select(Convert));
            return res;
        }

        public static Lib.ReductionInfo Convert(this ReductionInfo reduction)
        {
            return new Lib.ReductionInfo
            {
                Key = reduction.Key.Convert(),
                Descriptor_ = reduction.Descriptor.Convert(),
            };
        }

        public static Lib.CurrencyInfo Convert(this CurrencyInfo currency)
        {
            return new Lib.CurrencyInfo
            {
                Name = Convert(currency.Name),
            };
        }

        public static Lib.SymbolInfo Convert(this SymbolKey symbol)
        {
            return new Lib.SymbolInfo
            {
                Name = Convert(symbol.Name),
                Origin = symbol.Origin.Convert(),
            };
        }

        public static Lib.AccountMetadataInfo Convert(this AccountMetadataInfo accountMetadata)
        {
            var res = new Lib.AccountMetadataInfo
            {
                Key = accountMetadata.Key.Convert(),
                DefaultSymbol = accountMetadata.DefaultSymbol.Convert(),
            };
            res.Symbols.AddRange(accountMetadata.Symbols.Select(Convert));
            return res;
        }

        public static Lib.MappingInfo Convert(this MappingInfo mapping)
        {
            return new Lib.MappingInfo
            {
                Key = mapping.Key.Convert(),
                DisplayName = Convert(mapping.DisplayName),
            };
        }

        public static Lib.MappingCollectionInfo Convert(this MappingCollectionInfo mappings)
        {
            var res = new Lib.MappingCollectionInfo
            {
                DefaultFullBarToBarReduction = mappings.DefaultFullBarToBarReduction.Convert(),
                DefaultFullBarToDoubleReduction = mappings.DefaultFullBarToDoubleReduction.Convert(),
                DefaultBarToDoubleReduction = mappings.DefaultBarToDoubleReduction.Convert(),
                DefaultQuoteToBarReduction = mappings.DefaultQuoteToBarReduction.Convert(),
                DefaultQuoteToDoubleReduction = mappings.DefaultQuoteToDoubleReduction.Convert(),
            };
            res.BarToBarMappings.AddRange(mappings.BarToBarMappings.Select(Convert));
            res.BarToDoubleMappings.AddRange(mappings.BarToDoubleMappings.Select(Convert));
            res.QuoteToBarMappings.AddRange(mappings.QuoteToBarMappings.Select(Convert));
            res.QuoteToDoubleMappings.AddRange(mappings.QuoteToDoubleMappings.Select(Convert));
            return res;
        }

        public static Lib.ApiMetadataInfo Convert(this ApiMetadataInfo apiMetadata)
        {
            var res = new Lib.ApiMetadataInfo();
            res.TimeFrames.AddRange(apiMetadata.TimeFrames.Select(Convert));
            res.LineStyles.AddRange(apiMetadata.LineStyles.Select(Convert));
            res.Thicknesses.AddRange(apiMetadata.Thicknesses);
            res.MarkerSizes.AddRange(apiMetadata.MarkerSizes.Select(Convert));
            return res;
        }

        public static Lib.SetupContextInfo Convert(this SetupContextInfo setupContext)
        {
            return new Lib.SetupContextInfo
            {
                DefaultTimeFrame = setupContext.DefaultTimeFrame.Convert(),
                DefaultSymbol = setupContext.DefaultSymbol.Convert(),
                DefaultMapping = setupContext.DefaultMapping.Convert(),
            };
        }

        public static Lib.ConnectionErrorInfo.Types.ConnectionErrorCode Convert(this ConnectionErrorCodes code)
        {
            switch (code)
            {
                case ConnectionErrorCodes.None:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.None;
                case ConnectionErrorCodes.Unknown:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.UnknownConnnectionError;
                case ConnectionErrorCodes.NetworkError:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.NetworkError;
                case ConnectionErrorCodes.Timeout:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.Timeout;
                case ConnectionErrorCodes.BlockedAccount:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.BlockedAccount;
                case ConnectionErrorCodes.ClientInitiated:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ClientInitiated;
                case ConnectionErrorCodes.InvalidCredentials:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.InvalidCredentials;
                case ConnectionErrorCodes.SlowConnection:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.SlowConnection;
                case ConnectionErrorCodes.ServerError:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ServerError;
                case ConnectionErrorCodes.LoginDeleted:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.LoginDeleted;
                case ConnectionErrorCodes.ServerLogout:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ServerLogout;
                case ConnectionErrorCodes.Canceled:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.Canceled;
                case ConnectionErrorCodes.RejectedByServer:
                    return Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.RejectedByServer;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.ConnectionErrorInfo Convert(this ConnectionErrorInfo error)
        {
            return new Lib.ConnectionErrorInfo
            {
                Code = error.Code.Convert(),
                TextMessage = Convert(error.TextMessage),
            };
        }

        public static Lib.AccountModelInfo.Types.ConnectionState Convert(this ConnectionStates state)
        {
            switch (state)
            {
                case ConnectionStates.Offline:
                    return Lib.AccountModelInfo.Types.ConnectionState.Offline;
                case ConnectionStates.Connecting:
                    return Lib.AccountModelInfo.Types.ConnectionState.Connecting;
                case ConnectionStates.Online:
                    return Lib.AccountModelInfo.Types.ConnectionState.Online;
                case ConnectionStates.Disconnecting:
                    return Lib.AccountModelInfo.Types.ConnectionState.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.AccountModelInfo Convert(this AccountModelInfo account)
        {
            return new Lib.AccountModelInfo
            {
                Key = account.Key.Convert(),
                UseNewProtocol = true,
                ConnectionState = account.ConnectionState.Convert(),
                LastError = account.LastError.Convert(),
            };
        }

        public static Lib.BotModelInfo.Types.PluginState Convert(this PluginStates state)
        {
            switch (state)
            {
                case PluginStates.Stopped:
                    return Lib.BotModelInfo.Types.PluginState.Stopped;
                case PluginStates.Starting:
                    return Lib.BotModelInfo.Types.PluginState.Starting;
                case PluginStates.Faulted:
                    return Lib.BotModelInfo.Types.PluginState.Faulted;
                case PluginStates.Running:
                    return Lib.BotModelInfo.Types.PluginState.Running;
                case PluginStates.Stopping:
                    return Lib.BotModelInfo.Types.PluginState.Stopping;
                case PluginStates.Broken:
                    return Lib.BotModelInfo.Types.PluginState.Broken;
                case PluginStates.Reconnecting:
                    return Lib.BotModelInfo.Types.PluginState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.BotModelInfo ConvertLight(this BotModelInfo bot)
        {
            return new Lib.BotModelInfo
            {
                InstanceId = Convert(bot.InstanceId),
                Account = bot.Account.Convert(),
                State = bot.State.Convert(),
                FaultMessage = Convert(bot.FaultMessage),
            };
        }

        public static Lib.BotModelInfo Convert(this BotModelInfo bot, VersionSpec version = null)
        {
            var res = bot.ConvertLight();
            res.Config = bot.Config.Convert(version);
            res.Descriptor_ = bot.Descriptor?.ConvertLight();
            return res;
        }

        public static Lib.LogRecordInfo.Types.LogSeverity Convert(this LogSeverity type)
        {
            switch (type)
            {
                case LogSeverity.Info:
                    return Lib.LogRecordInfo.Types.LogSeverity.Info;
                case LogSeverity.Error:
                    return Lib.LogRecordInfo.Types.LogSeverity.Error;
                case LogSeverity.Trade:
                    return Lib.LogRecordInfo.Types.LogSeverity.Trade;
                case LogSeverity.TradeSuccess:
                    return Lib.LogRecordInfo.Types.LogSeverity.TradeSuccess;
                case LogSeverity.TradeFail:
                    return Lib.LogRecordInfo.Types.LogSeverity.TradeFail;
                case LogSeverity.Custom:
                    return Lib.LogRecordInfo.Types.LogSeverity.Custom;
                case LogSeverity.Alert:
                    return Lib.LogRecordInfo.Types.LogSeverity.Alert;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.LogRecordInfo Convert(this LogRecordInfo logRecord)
        {
            return new Lib.LogRecordInfo
            {
                TimeUtc = Timestamp.FromDateTime(logRecord.TimeUtc.Timestamp),
                TimeShit = logRecord.TimeUtc.Shift,
                Severity = logRecord.Severity.Convert(),
                Message = Convert(logRecord.Message),
            };
        }

        public static Lib.AlertRecordInfo Convert(this AlertRecordInfo alertRecord)
        {
            return new Lib.AlertRecordInfo
            {
                TimeUtc = Timestamp.FromDateTime(alertRecord.TimeUtc.Timestamp),
                TimeShit = alertRecord.TimeUtc.Shift,
                Message = Convert(alertRecord.Message),
                BotId = Convert(alertRecord.BotId),
            };
        }

        public static Lib.BotFileInfo Convert(this BotFileInfo botFile)
        {
            return new Lib.BotFileInfo
            {
                Name = Convert(botFile.Name),
                Size = botFile.Size,
            };
        }

        public static Lib.BotFolderInfo.Types.BotFolderId Convert(this BotFolderId type)
        {
            switch (type)
            {
                case BotFolderId.AlgoData:
                    return Lib.BotFolderInfo.Types.BotFolderId.AlgoData;
                case BotFolderId.BotLogs:
                    return Lib.BotFolderInfo.Types.BotFolderId.BotLogs;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.BotFolderInfo Convert(this BotFolderInfo botFolder)
        {
            var res = new Lib.BotFolderInfo
            {
                BotId = Convert(botFolder.BotId),
                FolderId = botFolder.FolderId.Convert(),
                Path = Convert(botFolder.Path),
            };
            res.Files.AddRange(botFolder.Files.Select(Convert));
            return res;
        }

        #endregion metadata.proto


        public static Lib.LoginResponse.Types.AccessLevel Convert(this AccessLevels level)
        {
            switch (level)
            {
                case AccessLevels.Viewer:
                    return Lib.LoginResponse.Types.AccessLevel.Viewer;
                case AccessLevels.Dealer:
                    return Lib.LoginResponse.Types.AccessLevel.Dealer;
                case AccessLevels.Admin:
                    return Lib.LoginResponse.Types.AccessLevel.Admin;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.UpdateInfo.Types.UpdateType Convert(this UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Added:
                    return Lib.UpdateInfo.Types.UpdateType.Added;
                case UpdateType.Replaced:
                    return Lib.UpdateInfo.Types.UpdateType.Replaced;
                case UpdateType.Removed:
                    return Lib.UpdateInfo.Types.UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.UpdateInfo Convert(this UpdateInfo update)
        {
            return new Lib.UpdateInfo { Type = update.Type.Convert() };
        }

        public static Lib.UpdateInfo Convert(this UpdateInfo<PackageInfo> update)
        {
            var res = ((UpdateInfo)update).Convert();
            res.Package = new Lib.PackageUpdateInfo { Package = update.Value.Convert() };
            return res;
        }

        public static Lib.UpdateInfo ConvertStateUpdate(this UpdateInfo<PackageInfo> update)
        {
            var res = ((UpdateInfo)update).Convert();
            res.PackageState = new Lib.PackageStateUpdateInfo { Package = update.Value.ConvertLight() };
            return res;
        }

        public static Lib.UpdateInfo Convert(this UpdateInfo<AccountModelInfo> update)
        {
            var res = ((UpdateInfo)update).Convert();
            res.Account = new Lib.AccountUpdateInfo { Account = update.Value.Convert() };
            return res;
        }

        public static Lib.UpdateInfo ConvertStateUpdate(this UpdateInfo<AccountModelInfo> update)
        {
            var res = ((UpdateInfo)update).Convert();
            res.AccountState = new Lib.AccountStateUpdateInfo { Account = update.Value.Convert() };
            return res;
        }

        public static Lib.UpdateInfo Convert(this UpdateInfo<BotModelInfo> update, VersionSpec version = null)
        {
            var res = ((UpdateInfo)update).Convert();
            res.Bot = new Lib.BotUpdateInfo { Bot = update.Value.Convert(version) };
            return res;
        }

        public static Lib.UpdateInfo ConvertStateUpdate(this UpdateInfo<BotModelInfo> update)
        {
            var res = ((UpdateInfo)update).Convert();
            res.BotState = new Lib.BotStateUpdateInfo { Bot = update.Value.ConvertLight() };
            return res;
        }
    }
}
