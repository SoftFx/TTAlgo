using Google.Protobuf;
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
    internal static class ToAlgo
    {
        public static byte[] Convert(this ByteString bytes)
        {
            return bytes.ToByteArray();
        }


        #region descriptors.proto

        public static TimeFrames Convert(this Lib.ApiDescriptor.Types.TimeFrame timeFrame)
        {
            switch (timeFrame)
            {
                case Lib.ApiDescriptor.Types.TimeFrame.MN:
                    return TimeFrames.MN;
                case Lib.ApiDescriptor.Types.TimeFrame.D:
                    return TimeFrames.D;
                case Lib.ApiDescriptor.Types.TimeFrame.W:
                    return TimeFrames.W;
                case Lib.ApiDescriptor.Types.TimeFrame.H4:
                    return TimeFrames.H4;
                case Lib.ApiDescriptor.Types.TimeFrame.H1:
                    return TimeFrames.H1;
                case Lib.ApiDescriptor.Types.TimeFrame.M30:
                    return TimeFrames.M30;
                case Lib.ApiDescriptor.Types.TimeFrame.M15:
                    return TimeFrames.M15;
                case Lib.ApiDescriptor.Types.TimeFrame.M5:
                    return TimeFrames.M5;
                case Lib.ApiDescriptor.Types.TimeFrame.M1:
                    return TimeFrames.M1;
                case Lib.ApiDescriptor.Types.TimeFrame.S10:
                    return TimeFrames.S10;
                case Lib.ApiDescriptor.Types.TimeFrame.S1:
                    return TimeFrames.S1;
                case Lib.ApiDescriptor.Types.TimeFrame.Ticks:
                    return TimeFrames.Ticks;
                default:
                    throw new ArgumentException();
            }
        }

        public static LineStyles Convert(this Lib.ApiDescriptor.Types.LineStyle style)
        {
            switch (style)
            {
                case Lib.ApiDescriptor.Types.LineStyle.Solid:
                    return LineStyles.Solid;
                case Lib.ApiDescriptor.Types.LineStyle.Dots:
                    return LineStyles.Dots;
                case Lib.ApiDescriptor.Types.LineStyle.DotsRare:
                    return LineStyles.DotsRare;
                case Lib.ApiDescriptor.Types.LineStyle.DotsVeryRare:
                    return LineStyles.DotsVeryRare;
                case Lib.ApiDescriptor.Types.LineStyle.LinesDots:
                    return LineStyles.LinesDots;
                case Lib.ApiDescriptor.Types.LineStyle.Lines:
                    return LineStyles.Lines;
                default:
                    throw new ArgumentException();
            }
        }

        public static PlotType Convert(this Lib.ApiDescriptor.Types.PlotType type)
        {
            switch (type)
            {
                case Lib.ApiDescriptor.Types.PlotType.Line:
                    return PlotType.Line;
                case Lib.ApiDescriptor.Types.PlotType.Histogram:
                    return PlotType.Histogram;
                case Lib.ApiDescriptor.Types.PlotType.Points:
                    return PlotType.Points;
                case Lib.ApiDescriptor.Types.PlotType.DiscontinuousLine:
                    return PlotType.DiscontinuousLine;
                default:
                    throw new ArgumentException();
            }
        }

        public static OutputTargets Convert(this Lib.ApiDescriptor.Types.OutputTarget target)
        {
            switch (target)
            {
                case Lib.ApiDescriptor.Types.OutputTarget.Overlay:
                    return OutputTargets.Overlay;
                case Lib.ApiDescriptor.Types.OutputTarget.Window1:
                    return OutputTargets.Window1;
                case Lib.ApiDescriptor.Types.OutputTarget.Window2:
                    return OutputTargets.Window2;
                case Lib.ApiDescriptor.Types.OutputTarget.Window3:
                    return OutputTargets.Window3;
                case Lib.ApiDescriptor.Types.OutputTarget.Window4:
                    return OutputTargets.Window4;
                default:
                    throw new ArgumentException();
            }
        }

        public static MarkerSizes Convert(this Lib.ApiDescriptor.Types.MarkerSize markerSize)
        {
            switch (markerSize)
            {
                case Lib.ApiDescriptor.Types.MarkerSize.Large:
                    return MarkerSizes.Large;
                case Lib.ApiDescriptor.Types.MarkerSize.Medium:
                    return MarkerSizes.Medium;
                case Lib.ApiDescriptor.Types.MarkerSize.Small:
                    return MarkerSizes.Small;
                default:
                    throw new ArgumentException();
            }
        }

        public static AlgoPropertyTypes Convert(this Lib.PropertyDescriptor.Types.AlgoPropertyType type)
        {
            switch (type)
            {
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.UnknownPropertyType:
                    return AlgoPropertyTypes.Unknown;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.Parameter:
                    return AlgoPropertyTypes.Parameter;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.InputSeries:
                    return AlgoPropertyTypes.InputSeries;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.OutputSeries:
                    return AlgoPropertyTypes.OutputSeries;
                default:
                    throw new ArgumentException();
            }
        }

        public static AlgoPropertyErrors Convert(this Lib.PropertyDescriptor.Types.AlgoPropertyError error)
        {
            switch (error)
            {
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.None:
                    return AlgoPropertyErrors.None;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.UnknownPropertyError:
                    return AlgoPropertyErrors.Unknown;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.SetIsNotPublic:
                    return AlgoPropertyErrors.SetIsNotPublic;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.GetIsNotPublic:
                    return AlgoPropertyErrors.GetIsNotPublic;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.MultipleAttributes:
                    return AlgoPropertyErrors.MultipleAttributes;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.InputIsNotDataSeries:
                    return AlgoPropertyErrors.InputIsNotDataSeries;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.OutputIsNotDataSeries:
                    return AlgoPropertyErrors.OutputIsNotDataSeries;
                case Lib.PropertyDescriptor.Types.AlgoPropertyError.EmptyEnum:
                    return AlgoPropertyErrors.EmptyEnum;
                default:
                    throw new ArgumentException();
            }
        }

        public static PropertyDescriptor Convert(this Lib.PropertyDescriptor descriptor)
        {
            PropertyDescriptor res = null;
            switch (descriptor.PropertyType)
            {
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.UnknownPropertyType:
                    res = new PropertyDescriptor();
                    break;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.Parameter:
                    res = new ParameterDescriptor();
                    break;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.InputSeries:
                    res = new InputDescriptor();
                    break;
                case Lib.PropertyDescriptor.Types.AlgoPropertyType.OutputSeries:
                    res = new OutputDescriptor();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.Id = descriptor.Id;
            res.DisplayName = descriptor.DisplayName;
            res.Error = descriptor.Error.Convert();
            return res;
        }

        public static FileFilterEntry Convert(this Lib.FileFilterEntry entry)
        {
            return new FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static ParameterDescriptor Convert(this Lib.ParameterDescriptor parameter)
        {
            var res = (ParameterDescriptor)parameter.PropertyHeader.Convert();
            res.DataType = parameter.DataType;
            res.DefaultValue = parameter.DefaultValue;
            res.IsRequired = parameter.IsRequired;
            res.IsEnum = parameter.IsEnum;
            res.EnumValues.AddRange(parameter.EnumValues);
            res.FileFilters.AddRange(parameter.FileFilters.Select(Convert));
            return res;
        }

        public static InputDescriptor Convert(this Lib.InputDescriptor input)
        {
            var res = (InputDescriptor)input.PropertyHeader.Convert();
            res.DataSeriesBaseTypeFullName = input.DataSeriesBaseTypeFullName;
            return res;
        }

        public static OutputDescriptor Convert(this Lib.OutputDescriptor output)
        {
            var res = (OutputDescriptor)output.PropertyHeader.Convert();
            res.DataSeriesBaseTypeFullName = output.DataSeriesBaseTypeFullName;
            res.DefaultThickness = output.DefaultThickness;
            res.DefaultColor = (Colors)output.DefaultColor;
            res.DefaultLineStyle = output.DefaultLineStyle.Convert();
            res.PlotType = output.PlotType.Convert();
            res.Target = output.Target.Convert();
            res.Precision = output.Precision;
            res.ZeroLine = output.ZeroLine;
            return res;
        }

        public static AlgoTypes Convert(this Lib.PluginDescriptor.Types.AlgoType type)
        {
            switch (type)
            {
                case Lib.PluginDescriptor.Types.AlgoType.UnknownPluginType:
                    return AlgoTypes.Unknown;
                case Lib.PluginDescriptor.Types.AlgoType.Indicator:
                    return AlgoTypes.Indicator;
                case Lib.PluginDescriptor.Types.AlgoType.Robot:
                    return AlgoTypes.Robot;
                default:
                    throw new ArgumentException();
            }
        }

        public static AlgoMetadataErrors Convert(this Lib.PluginDescriptor.Types.AlgoMetadataError error)
        {
            switch (error)
            {
                case Lib.PluginDescriptor.Types.AlgoMetadataError.None:
                    return AlgoMetadataErrors.None;
                case Lib.PluginDescriptor.Types.AlgoMetadataError.UnknownMetadataError:
                    return AlgoMetadataErrors.Unknown;
                case Lib.PluginDescriptor.Types.AlgoMetadataError.HasInvalidProperties:
                    return AlgoMetadataErrors.HasInvalidProperties;
                case Lib.PluginDescriptor.Types.AlgoMetadataError.UnknownBaseType:
                    return AlgoMetadataErrors.UnknownBaseType;
                case Lib.PluginDescriptor.Types.AlgoMetadataError.IncompatibleApiVersion:
                    return AlgoMetadataErrors.IncompatibleApiVersion;
                default:
                    throw new ArgumentException();
            }
        }

        public static PluginDescriptor ConvertLight(this Lib.PluginDescriptor plugin)
        {
            var res = new PluginDescriptor
            {
                ApiVersionStr = plugin.ApiVersionStr,
                Id = plugin.Id,
                DisplayName = plugin.DisplayName,
                Type = plugin.Type.Convert(),
                Error = plugin.Error.Convert(),
                UiDisplayName = plugin.UiDisplayName,
                Category = plugin.Category,
                Version = plugin.Version,
                Description = plugin.Description,
                Copyright = plugin.Copyright,
                SetupMainSymbol = plugin.SetupMainSymbol,
            };
            return res;
        }

        public static PluginDescriptor Convert(this Lib.PluginDescriptor plugin)
        {
            var res = plugin.ConvertLight();
            res.Parameters.AddRange(plugin.Parameters.Select(Convert));
            res.Inputs.AddRange(plugin.Inputs.Select(Convert));
            res.Outputs.AddRange(plugin.Outputs.Select(Convert));
            return res;
        }

        public static ReductionType Convert(this Lib.ReductionDescriptor.Types.ReductionType type)
        {
            switch (type)
            {
                case Lib.ReductionDescriptor.Types.ReductionType.UnknownReductionType:
                    return ReductionType.Unknown;
                case Lib.ReductionDescriptor.Types.ReductionType.BarToDouble:
                    return ReductionType.BarToDouble;
                case Lib.ReductionDescriptor.Types.ReductionType.FullBarToDouble:
                    return ReductionType.FullBarToDouble;
                case Lib.ReductionDescriptor.Types.ReductionType.FullBarToBar:
                    return ReductionType.FullBarToBar;
                case Lib.ReductionDescriptor.Types.ReductionType.QuoteToDouble:
                    return ReductionType.QuoteToDouble;
                case Lib.ReductionDescriptor.Types.ReductionType.QuoteToBar:
                    return ReductionType.QuoteToBar;
                default:
                    throw new ArgumentException();
            }
        }

        public static ReductionDescriptor Convert(this Lib.ReductionDescriptor reduction)
        {
            return new ReductionDescriptor
            {
                ApiVersionStr = reduction.ApiVersionStr,
                Id = reduction.Id,
                DisplayName = reduction.DisplayName,
                Type = reduction.Type.Convert(),
            };
        }

        #endregion descriptors.proto


        #region config.proto

        public static Property Convert(this Lib.Property property)
        {
            Property res;
            switch (property.PropertyCase)
            {
                case Lib.Property.PropertyOneofCase.Parameter:
                    res = property.Parameter.Convert();
                    break;
                case Lib.Property.PropertyOneofCase.Input:
                    res = property.Input.Convert();
                    break;
                case Lib.Property.PropertyOneofCase.Output:
                    res = property.Output.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.Id = property.PropertyId;
            return res;
        }

        public static Parameter Convert(this Lib.Parameter parameter)
        {
            Parameter res;
            switch (parameter.ParameterCase)
            {
                case Lib.Parameter.ParameterOneofCase.Bool:
                    res = parameter.Bool.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.Int:
                    res = parameter.Int.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.NullInt:
                    res = parameter.NullInt.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.Double:
                    res = parameter.Double.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.NullDouble:
                    res = parameter.NullDouble.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.String:
                    res = parameter.String.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.Enum:
                    res = parameter.Enum.Convert();
                    break;
                case Lib.Parameter.ParameterOneofCase.File:
                    res = parameter.File.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static BoolParameter Convert(this Lib.BoolParameter param)
        {
            return new BoolParameter
            {
                Value = param.Value,
            };
        }

        public static IntParameter Convert(this Lib.IntParameter param)
        {
            return new IntParameter
            {
                Value = param.Value,
            };
        }

        public static NullableIntParameter Convert(this Lib.NullableIntParameter param)
        {
            return new NullableIntParameter
            {
                Value = param.Value?.Value,
            };
        }

        public static DoubleParameter Convert(this Lib.DoubleParameter param)
        {
            return new DoubleParameter
            {
                Value = param.Value,
            };
        }

        public static NullableDoubleParameter Convert(this Lib.NullableDoubleParameter param)
        {
            return new NullableDoubleParameter
            {
                Value = param.Value?.Value,
            };
        }

        public static StringParameter Convert(this Lib.StringParameter param)
        {
            return new StringParameter
            {
                Value = param.Value,
            };
        }

        public static EnumParameter Convert(this Lib.EnumParameter param)
        {
            return new EnumParameter
            {
                Value = param.Value,
            };
        }

        public static FileParameter Convert(this Lib.FileParameter param)
        {
            return new FileParameter
            {
                FileName = param.FileName,
            };
        }

        public static SymbolOrigin Convert(this Lib.SymbolConfig.Types.SymbolOrigin markerSize)
        {
            switch (markerSize)
            {
                case Lib.SymbolConfig.Types.SymbolOrigin.Online:
                    return SymbolOrigin.Online;
                case Lib.SymbolConfig.Types.SymbolOrigin.Custom:
                    return SymbolOrigin.Custom;
                case Lib.SymbolConfig.Types.SymbolOrigin.Special:
                    return SymbolOrigin.Special;
                default:
                    throw new ArgumentException();
            }
        }

        public static SymbolConfig Convert(this Lib.SymbolConfig config)
        {
            return new SymbolConfig
            {
                Name = config.Name,
                Origin = config.Origin.Convert(),
            };
        }

        public static Input Convert(this Lib.Input input)
        {
            Input res;
            switch (input.InputCase)
            {
                case Lib.Input.InputOneofCase.Quote:
                    res = input.Quote.Convert();
                    break;
                case Lib.Input.InputOneofCase.Mapped:
                    res = input.Mapped.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.SelectedSymbol = input.SelectedSymbol.Convert();
            return res;
        }

        public static QuoteInput Convert(this Lib.QuoteInput input)
        {
            return new QuoteInput
            {
                UseL2 = input.UseL2,
            };
        }

        public static MappedInput Convert(this Lib.MappedInput input)
        {
            MappedInput res;
            switch (input.InputCase)
            {
                case Lib.MappedInput.InputOneofCase.BarToBar:
                    res = input.BarToBar.Convert();
                    break;
                case Lib.MappedInput.InputOneofCase.BarToDouble:
                    res = input.BarToDouble.Convert();
                    break;
                case Lib.MappedInput.InputOneofCase.QuoteToBar:
                    res = input.QuoteToBar.Convert();
                    break;
                case Lib.MappedInput.InputOneofCase.QuoteToDouble:
                    res = input.QuoteToDouble.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.SelectedMapping = input.SelectedMapping.Convert();
            return res;
        }

        public static BarToBarInput Convert(this Lib.BarToBarInput input)
        {
            return new BarToBarInput();
        }

        public static BarToDoubleInput Convert(this Lib.BarToDoubleInput input)
        {
            return new BarToDoubleInput();
        }

        public static QuoteToBarInput Convert(this Lib.QuoteToBarInput input)
        {
            return new QuoteToBarInput();
        }

        public static QuoteToDoubleInput Convert(this Lib.QuoteToDoubleInput input)
        {
            return new QuoteToDoubleInput();
        }

        public static OutputColor Convert(this Lib.OutputColor color)
        {
            return new OutputColor
            {
                Alpha = color.Alpha,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
            };
        }

        public static Output Convert(this Lib.Output output)
        {
            Output res;
            switch (output.OutputCase)
            {
                case Lib.Output.OutputOneofCase.ColoredLine:
                    res = output.ColoredLine.Convert();
                    break;
                case Lib.Output.OutputOneofCase.MarkerSeries:
                    res = output.MarkerSeries.Convert();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.IsEnabled = output.IsEnabled;
            res.LineColor = output.LineColor.Convert();
            res.LineThickness = output.LineThickness;
            return res;
        }

        public static ColoredLineOutput Convert(this Lib.ColoredLineOutput output)
        {
            return new ColoredLineOutput
            {
                LineStyle = output.LineStyle.Convert(),
            };
        }

        public static MarkerSeriesOutput Convert(this Lib.MarkerSeriesOutput output)
        {
            return new MarkerSeriesOutput
            {
                MarkerSize = output.MarkerSize.Convert(),
            };
        }

        public static PluginPermissions Convert(this Lib.PluginPermissions permissions)
        {
            return new PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static PluginConfig Convert(this Lib.PluginConfig config)
        {
            var res = new PluginConfig
            {
                Key = config.Key.Convert(),
                TimeFrame = config.TimeFrame.Convert(),
                MainSymbol = config.MainSymbol.Convert(),
                SelectedMapping = config.SelectedMapping.Convert(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.Convert()
            };
            res.Properties.AddRange(config.Properties.Select(Convert));
            return res;
        }

        #endregion config.proto

        #region keys.proto

        public static AccountKey Convert(this Lib.AccountKey key)
        {
            return new AccountKey
            {
                Login = key.Login,
                Server = key.Server,
            };
        }

        public static RepositoryLocation Convert(this Lib.RepositoryLocation location)
        {
            switch (location)
            {
                case Lib.RepositoryLocation.Embedded:
                    return RepositoryLocation.Embedded;
                case Lib.RepositoryLocation.LocalRepository:
                    return RepositoryLocation.LocalRepository;
                case Lib.RepositoryLocation.LocalExtensions:
                    return RepositoryLocation.LocalExtensions;
                case Lib.RepositoryLocation.CommonRepository:
                    return RepositoryLocation.CommonRepository;
                case Lib.RepositoryLocation.CommonExtensions:
                    return RepositoryLocation.CommonExtensions;
                default:
                    throw new ArgumentException();
            }
        }

        public static PackageKey Convert(this Lib.PackageKey key)
        {
            return new PackageKey
            {
                Name = key.Name,
                Location = key.Location.Convert(),
            };
        }

        public static PluginKey Convert(this Lib.PluginKey key)
        {
            return new PluginKey
            {
                PackageName = key.PackageName,
                PackageLocation = key.PackageLocation.Convert(),
                DescriptorId = key.DescriptorId,
            };
        }

        public static ReductionKey Convert(this Lib.ReductionKey key)
        {
            return new ReductionKey
            {
                PackageName = key.PackageName,
                PackageLocation = key.PackageLocation.Convert(),
                DescriptorId = key.DescriptorId,
            };
        }

        public static MappingKey Convert(this Lib.MappingKey key)
        {
            return new MappingKey
            {
                PrimaryReduction = key.PrimaryReduction.Convert(),
                SecondaryReduction = key.SecondaryReduction?.Convert(),
            };
        }

        #endregion keys.proto

        #region metadata.proto

        public static PluginInfo Convert(this Lib.PluginInfo plugin)
        {
            return new PluginInfo
            {
                Key = plugin.Key.Convert(),
                Descriptor = plugin.Descriptor_.Convert(),
            };
        }

        public static PackageIdentity Convert(this Lib.PackageIdentity identity)
        {
            return new PackageIdentity
            {
                FileName = identity.FileName,
                FilePath = identity.FilePath,
                CreatedUtc = identity.CreatedUtc.ToDateTime(),
                LastModifiedUtc = identity.LastModifiedUtc.ToDateTime(),
                Size = identity.Size,
                Hash = identity.Hash,
            };
        }

        public static PackageInfo ConvertLight(this Lib.PackageInfo package)
        {
            var res = new PackageInfo
            {
                Key = package.Key.Convert(),
                IsValid = package.IsValid,
                IsLocked = package.IsLocked,
                IsObsolete = package.IsObsolete,
            };
            return res;
        }

        public static PackageInfo Convert(this Lib.PackageInfo package)
        {
            var res = package.ConvertLight();
            res.Identity = package.Identity.Convert();
            res.Plugins.AddRange(package.Plugins.Select(Convert));
            return res;
        }

        public static ReductionInfo Convert(this Lib.ReductionInfo reduction)
        {
            return new ReductionInfo
            {
                Key = reduction.Key.Convert(),
                Descriptor = reduction.Descriptor_.Convert(),
            };
        }

        public static CurrencyInfo Convert(this Lib.CurrencyInfo currency)
        {
            return new CurrencyInfo
            {
                Name = currency.Name,
            };
        }

        public static SymbolInfo Convert(this Lib.SymbolInfo symbol)
        {
            return new SymbolInfo
            {
                Name = symbol.Name,
                Origin = symbol.Origin.Convert(),
            };
        }

        public static AccountMetadataInfo Convert(this Lib.AccountMetadataInfo accountMetadata)
        {
            var res = new AccountMetadataInfo
            {
                Key = accountMetadata.Key.Convert(),
            };
            res.Symbols.AddRange(accountMetadata.Symbols.Select(Convert));
            return res;
        }

        public static MappingInfo Convert(this Lib.MappingInfo mapping)
        {
            return new MappingInfo
            {
                Key = mapping.Key.Convert(),
                DisplayName = mapping.DisplayName,
            };
        }

        public static MappingCollectionInfo Convert(this Lib.MappingCollectionInfo mappings)
        {
            var res = new MappingCollectionInfo
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

        public static ApiMetadataInfo Convert(this Lib.ApiMetadataInfo apiMetadata)
        {
            var res = new ApiMetadataInfo();
            res.TimeFrames.AddRange(apiMetadata.TimeFrames.Select(Convert));
            res.LineStyles.AddRange(apiMetadata.LineStyles.Select(Convert));
            res.Thicknesses.AddRange(apiMetadata.Thicknesses);
            res.MarkerSizes.AddRange(apiMetadata.MarkerSizes.Select(Convert));
            return res;
        }

        public static SetupContextInfo Convert(this Lib.SetupContextInfo setupContext)
        {
            return new SetupContextInfo
            {
                DefaultTimeFrame = setupContext.DefaultTimeFrame.Convert(),
                DefaultSymbol = setupContext.DefaultSymbol.Convert(),
                DefaultMapping = setupContext.DefaultMapping.Convert(),
            };
        }

        public static ConnectionErrorCodes Convert(this Lib.ConnectionErrorInfo.Types.ConnectionErrorCode code)
        {
            switch (code)
            {
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.None:
                    return ConnectionErrorCodes.None;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.UnknownConnnectionError:
                    return ConnectionErrorCodes.Unknown;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.NetworkError:
                    return ConnectionErrorCodes.NetworkError;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.Timeout:
                    return ConnectionErrorCodes.Timeout;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.BlockedAccount:
                    return ConnectionErrorCodes.BlockedAccount;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ClientInitiated:
                    return ConnectionErrorCodes.ClientInitiated;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.InvalidCredentials:
                    return ConnectionErrorCodes.InvalidCredentials;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.SlowConnection:
                    return ConnectionErrorCodes.SlowConnection;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ServerError:
                    return ConnectionErrorCodes.ServerError;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.LoginDeleted:
                    return ConnectionErrorCodes.LoginDeleted;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.ServerLogout:
                    return ConnectionErrorCodes.ServerLogout;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.Canceled:
                    return ConnectionErrorCodes.Canceled;
                case Lib.ConnectionErrorInfo.Types.ConnectionErrorCode.RejectedByServer:
                    return ConnectionErrorCodes.RejectedByServer;
                default:
                    throw new ArgumentException();
            }
        }

        public static ConnectionErrorInfo Convert(this Lib.ConnectionErrorInfo error)
        {
            return new ConnectionErrorInfo(error.Code.Convert(), error.TextMessage);
        }

        public static ConnectionStates Convert(this Lib.AccountModelInfo.Types.ConnectionState state)
        {
            switch (state)
            {
                case Lib.AccountModelInfo.Types.ConnectionState.Offline:
                    return ConnectionStates.Offline;
                case Lib.AccountModelInfo.Types.ConnectionState.Connecting:
                    return ConnectionStates.Connecting;
                case Lib.AccountModelInfo.Types.ConnectionState.Online:
                    return ConnectionStates.Online;
                case Lib.AccountModelInfo.Types.ConnectionState.Disconnecting:
                    return ConnectionStates.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static AccountModelInfo Convert(this Lib.AccountModelInfo account)
        {
            return new AccountModelInfo
            {
                Key = account.Key.Convert(),
                UseNewProtocol = account.UseNewProtocol,
                ConnectionState = account.ConnectionState.Convert(),
                LastError = account.LastError.Convert(),
            };
        }

        public static PluginStates Convert(this Lib.BotModelInfo.Types.PluginState state)
        {
            switch (state)
            {
                case Lib.BotModelInfo.Types.PluginState.Stopped:
                    return PluginStates.Stopped;
                case Lib.BotModelInfo.Types.PluginState.Starting:
                    return PluginStates.Starting;
                case Lib.BotModelInfo.Types.PluginState.Faulted:
                    return PluginStates.Faulted;
                case Lib.BotModelInfo.Types.PluginState.Running:
                    return PluginStates.Running;
                case Lib.BotModelInfo.Types.PluginState.Stopping:
                    return PluginStates.Stopping;
                case Lib.BotModelInfo.Types.PluginState.Broken:
                    return PluginStates.Broken;
                case Lib.BotModelInfo.Types.PluginState.Reconnecting:
                    return PluginStates.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static BotModelInfo ConvertLight(this Lib.BotModelInfo bot)
        {
            return new BotModelInfo
            {
                InstanceId = bot.InstanceId,
                Account = bot.Account.Convert(),
                State = bot.State.Convert(),
                FaultMessage = bot.FaultMessage,
            };
        }

        public static BotModelInfo Convert(this Lib.BotModelInfo bot)
        {
            var res = bot.ConvertLight();
            res.Config = bot.Config.Convert();
            res.Descriptor = bot.Descriptor_?.ConvertLight();
            return res;
        }

        public static LogSeverity Convert(this Lib.LogRecordInfo.Types.LogSeverity type)
        {
            switch (type)
            {
                case Lib.LogRecordInfo.Types.LogSeverity.Info:
                    return LogSeverity.Info;
                case Lib.LogRecordInfo.Types.LogSeverity.Error:
                    return LogSeverity.Error;
                case Lib.LogRecordInfo.Types.LogSeverity.Trade:
                    return LogSeverity.Trade;
                case Lib.LogRecordInfo.Types.LogSeverity.TradeSuccess:
                    return LogSeverity.TradeSuccess;
                case Lib.LogRecordInfo.Types.LogSeverity.TradeFail:
                    return LogSeverity.TradeFail;
                case Lib.LogRecordInfo.Types.LogSeverity.Custom:
                    return LogSeverity.Custom;
                default:
                    throw new ArgumentException();
            }
        }

        public static LogRecordInfo Convert(this Lib.LogRecordInfo logRecord)
        {
            return new LogRecordInfo
            {
                TimeUtc = logRecord.TimeUtc.ToDateTime(),
                Severity = logRecord.Severity.Convert(),
                Message = logRecord.Message,
            };
        }

        public static BotFileInfo Convert(this Lib.BotFileInfo botFile)
        {
            return new BotFileInfo
            {
                Name = botFile.Name,
                Size = botFile.Size,
            };
        }

        public static BotFolderId Convert(this Lib.BotFolderInfo.Types.BotFolderId type)
        {
            switch (type)
            {
                case Lib.BotFolderInfo.Types.BotFolderId.AlgoData:
                    return BotFolderId.AlgoData;
                case Lib.BotFolderInfo.Types.BotFolderId.BotLogs:
                    return BotFolderId.BotLogs;
                default:
                    throw new ArgumentException();
            }
        }

        public static BotFolderInfo Convert(this Lib.BotFolderInfo botFolder)
        {
            var res = new BotFolderInfo
            {
                BotId = botFolder.BotId,
                FolderId = botFolder.FolderId.Convert(),
                Path = botFolder.Path,
            };
            res.Files.AddRange(botFolder.Files.Select(Convert));
            return res;
        }

        #endregion metadata.proto

        public static UpdateType Convert(this Lib.UpdateInfo.Types.UpdateType type)
        {
            switch (type)
            {
                case Lib.UpdateInfo.Types.UpdateType.Added:
                    return UpdateType.Added;
                case Lib.UpdateInfo.Types.UpdateType.Replaced:
                    return UpdateType.Replaced;
                case Lib.UpdateInfo.Types.UpdateType.Removed:
                    return UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        public static UpdateInfo Convert(this Lib.UpdateInfo update)
        {
            UpdateInfo res;
            switch (update.UpdateInfoCase)
            {
                case Lib.UpdateInfo.UpdateInfoOneofCase.Package:
                    res = update.Package.Convert();
                    break;
                case Lib.UpdateInfo.UpdateInfoOneofCase.PackageState:
                    res = update.PackageState.ConvertStateUpdate();
                    break;
                case Lib.UpdateInfo.UpdateInfoOneofCase.Account:
                    res = update.Account.Convert();
                    break;
                case Lib.UpdateInfo.UpdateInfoOneofCase.AccountState:
                    res = update.AccountState.ConvertStateUpdate();
                    break;
                case Lib.UpdateInfo.UpdateInfoOneofCase.Bot:
                    res = update.Bot.Convert();
                    break;
                case Lib.UpdateInfo.UpdateInfoOneofCase.BotState:
                    res = update.BotState.ConvertStateUpdate();
                    break;
                default:
                    throw new ArgumentException();
            }
            res.Type = update.Type.Convert();
            return res;
        }

        public static UpdateInfo<PackageInfo> Convert(this Lib.PackageUpdateInfo update)
        {
            return new UpdateInfo<PackageInfo> { Value = update.Package.Convert() };
        }

        public static UpdateInfo<PackageInfo> ConvertStateUpdate(this Lib.PackageStateUpdateInfo update)
        {
            return new UpdateInfo<PackageInfo> { Value = update.Package.ConvertLight() };
        }

        public static UpdateInfo<AccountModelInfo> Convert(this Lib.AccountUpdateInfo update)
        {
            return new UpdateInfo<AccountModelInfo> { Value = update.Account.Convert() };
        }

        public static UpdateInfo<AccountModelInfo> ConvertStateUpdate(this Lib.AccountStateUpdateInfo update)
        {
            return new UpdateInfo<AccountModelInfo> { Value = update.Account.Convert() };
        }

        public static UpdateInfo<BotModelInfo> Convert(this Lib.BotUpdateInfo update)
        {
            return new UpdateInfo<BotModelInfo> { Value = update.Bot.Convert() };
        }

        public static UpdateInfo<BotModelInfo> ConvertStateUpdate(this Lib.BotStateUpdateInfo update)
        {
            return new UpdateInfo<BotModelInfo> { Value = update.Bot.ConvertLight() };
        }
    }
}
