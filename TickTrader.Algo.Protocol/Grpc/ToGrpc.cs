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


        #region descriptors.proto

        public static Lib.ApiDescriptor.Types.TimeFrame Convert(TimeFrames timeFrame)
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

        public static Lib.ApiDescriptor.Types.LineStyle Convert(LineStyles style)
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

        public static Lib.ApiDescriptor.Types.PlotType Convert(PlotType type)
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

        public static Lib.ApiDescriptor.Types.OutputTarget Convert(OutputTargets target)
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

        public static Lib.ApiDescriptor.Types.MarkerSize Convert(MarkerSizes markerSize)
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

        public static Lib.PropertyDescriptor.Types.AlgoPropertyType Convert(AlgoPropertyTypes type)
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

        public static Lib.PropertyDescriptor.Types.AlgoPropertyError Convert(AlgoPropertyErrors error)
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

        public static Lib.PropertyDescriptor Convert(PropertyDescriptor property)
        {
            return new Lib.PropertyDescriptor
            {
                Id = Convert(property.Id),
                DisplayName = Convert(property.DisplayName),
                PropertyType = Convert(property.PropertyType),
                Error = Convert(property.Error),
            };
        }

        public static Lib.FileFilterEntry Convert(FileFilterEntry entry)
        {
            return new Lib.FileFilterEntry
            {
                FileTypeName = Convert(entry.FileTypeName),
                FileMask = Convert(entry.FileMask),
            };
        }

        public static Lib.ParameterDescriptor Convert(ParameterDescriptor parameter)
        {
            var res = new Lib.ParameterDescriptor
            {
                PropertyHeader = Convert((PropertyDescriptor)parameter),
                DataType = Convert(parameter.DataType),
                DefaultValue = Convert(parameter.DefaultValue),
                IsRequired = parameter.IsRequired,
                IsEnum = parameter.IsEnum,
            };
            res.EnumValues.AddRange(parameter.EnumValues);
            res.FileFilters.AddRange(parameter.FileFilters.Select(Convert));
            return res;
        }

        public static Lib.InputDescriptor Convert(InputDescriptor input)
        {
            return new Lib.InputDescriptor
            {
                PropertyHeader = Convert((PropertyDescriptor)input),
                DataSeriesBaseTypeFullName = Convert(input.DataSeriesBaseTypeFullName),
            };
        }

        public static Lib.OutputDescriptor Convert(OutputDescriptor output)
        {
            return new Lib.OutputDescriptor
            {
                PropertyHeader = Convert((PropertyDescriptor)output),
                DataSeriesBaseTypeFullName = Convert(output.DataSeriesBaseTypeFullName),
                DefaultThickness = output.DefaultThickness,
                DefaultColor = (int)output.DefaultColor,
                DefaultLineStyle = Convert(output.DefaultLineStyle),
                PlotType = Convert(output.PlotType),
                Target = Convert(output.Target),
                Precision = output.Precision,
                ZeroLine = output.ZeroLine,
            };
        }

        public static Lib.PluginDescriptor.Types.AlgoType Convert(AlgoTypes type)
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

        public static Lib.PluginDescriptor.Types.AlgoMetadataError Convert(AlgoMetadataErrors error)
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

        public static Lib.PluginDescriptor ConvertLight(PluginDescriptor plugin)
        {
            var res = new Lib.PluginDescriptor
            {
                ApiVersionStr = Convert(plugin.ApiVersionStr),
                Id = Convert(plugin.Id),
                DisplayName = Convert(plugin.DisplayName),
                Type = Convert(plugin.Type),
                Error = Convert(plugin.Error),
                UiDisplayName = Convert(plugin.UiDisplayName),
                Category = Convert(plugin.Category),
                Version = Convert(plugin.Version),
                Description = Convert(plugin.Description),
                Copyright = Convert(plugin.Copyright),
                SetupMainSymbol = plugin.SetupMainSymbol,
            };
            return res;
        }

        public static Lib.PluginDescriptor Convert(PluginDescriptor plugin)
        {
            var res = ConvertLight(plugin);
            res.Parameters.AddRange(plugin.Parameters.Select(Convert));
            res.Inputs.AddRange(plugin.Inputs.Select(Convert));
            res.Outputs.AddRange(plugin.Outputs.Select(Convert));
            return res;
        }

        public static Lib.ReductionDescriptor.Types.ReductionType Convert(ReductionType type)
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

        public static Lib.ReductionDescriptor Convert(ReductionDescriptor reduction)
        {
            return new Lib.ReductionDescriptor
            {
                ApiVersionStr = Convert(reduction.ApiVersionStr),
                Id = Convert(reduction.Id),
                DisplayName = Convert(reduction.DisplayName),
                Type = Convert(reduction.Type),
            };
        }

        #endregion descriptors.proto


        #region config.proto

        public static Lib.Property Convert(Property property)
        {
            var res = new Lib.Property
            {
                PropertyId = Convert(property.Id),
            };
            switch (property)
            {
                case Parameter parameter:
                    res.Parameter = Convert(parameter);
                    break;
                case Input input:
                    res.Input = Convert(input);
                    break;
                case Output output:
                    res.Output = Convert(output);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.Parameter Convert(Parameter parameter)
        {
            var res = new Lib.Parameter();
            switch (parameter)
            {
                case BoolParameter boolParam:
                    res.Bool = Convert(boolParam);
                    break;
                case IntParameter intParam:
                    res.Int = Convert(intParam);
                    break;
                case NullableIntParameter nullIntParam:
                    res.NullInt = Convert(nullIntParam);
                    break;
                case DoubleParameter doubleParam:
                    res.Double = Convert(doubleParam);
                    break;
                case NullableDoubleParameter nullDoubleParam:
                    res.NullDouble = Convert(nullDoubleParam);
                    break;
                case StringParameter stringParam:
                    res.String = Convert(stringParam);
                    break;
                case EnumParameter enumParam:
                    res.Enum = Convert(enumParam);
                    break;
                case FileParameter fileParam:
                    res.File = Convert(fileParam);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.BoolParameter Convert(BoolParameter param)
        {
            return new Lib.BoolParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.IntParameter Convert(IntParameter param)
        {
            return new Lib.IntParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.NullableIntParameter Convert(NullableIntParameter param)
        {
            return new Lib.NullableIntParameter
            {
                Value = param.Value.HasValue ? new Int32Value { Value = param.Value.Value } : null,
            };
        }

        public static Lib.DoubleParameter Convert(DoubleParameter param)
        {
            return new Lib.DoubleParameter
            {
                Value = param.Value,
            };
        }

        public static Lib.NullableDoubleParameter Convert(NullableDoubleParameter param)
        {
            return new Lib.NullableDoubleParameter
            {
                Value = param.Value.HasValue ? new DoubleValue { Value = param.Value.Value } : null,
            };
        }

        public static Lib.StringParameter Convert(StringParameter param)
        {
            return new Lib.StringParameter
            {
                Value = Convert(param.Value),
            };
        }

        public static Lib.EnumParameter Convert(EnumParameter param)
        {
            return new Lib.EnumParameter
            {
                Value = Convert(param.Value),
            };
        }

        public static Lib.FileParameter Convert(FileParameter param)
        {
            return new Lib.FileParameter
            {
                FileName = Convert(param.FileName),
            };
        }

        public static Lib.Input Convert(Input input)
        {
            var res = new Lib.Input
            {
                SelectedSymbol = Convert(input.SelectedSymbol),
            };
            switch (input)
            {
                case QuoteInput quoteInput:
                    res.Quote = Convert(quoteInput);
                    break;
                case MappedInput mappedInput:
                    res.Mapped = Convert(mappedInput);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.QuoteInput Convert(QuoteInput input)
        {
            return new Lib.QuoteInput
            {
                UseL2 = input.UseL2,
            };
        }

        public static Lib.MappedInput Convert(MappedInput input)
        {
            var res = new Lib.MappedInput
            {
                SelectedMapping = Convert(input.SelectedMapping),
            };
            switch (input)
            {
                case BarToBarInput barToBar:
                    res.BarToBar = Convert(barToBar);
                    break;
                case BarToDoubleInput barToDouble:
                    res.BarToDouble = Convert(barToDouble);
                    break;
                case QuoteToBarInput quoteToBar:
                    res.QuoteToBar = Convert(quoteToBar);
                    break;
                case QuoteToDoubleInput quoteToDouble:
                    res.QuoteToDouble = Convert(quoteToDouble);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.BarToBarInput Convert(BarToBarInput input)
        {
            return new Lib.BarToBarInput();
        }

        public static Lib.BarToDoubleInput Convert(BarToDoubleInput input)
        {
            return new Lib.BarToDoubleInput();
        }

        public static Lib.QuoteToBarInput Convert(QuoteToBarInput input)
        {
            return new Lib.QuoteToBarInput();
        }

        public static Lib.QuoteToDoubleInput Convert(QuoteToDoubleInput input)
        {
            return new Lib.QuoteToDoubleInput();
        }

        public static Lib.OutputColor Convert(OutputColor color)
        {
            return new Lib.OutputColor
            {
                Alpha = color.Alpha,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
            };
        }

        public static Lib.Output Convert(Output output)
        {
            var res = new Lib.Output
            {
                IsEnabled = output.IsEnabled,
                LineColor = Convert(output.LineColor),
                LineThickness = output.LineThickness,
            };
            switch (output)
            {
                case ColoredLineOutput coloredLine:
                    res.ColoredLine = Convert(coloredLine);
                    break;
                case MarkerSeriesOutput markerSeries:
                    res.MarkerSeries = Convert(markerSeries);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.ColoredLineOutput Convert(ColoredLineOutput output)
        {
            return new Lib.ColoredLineOutput
            {
                LineStyle = Convert(output.LineStyle),
            };
        }

        public static Lib.MarkerSeriesOutput Convert(MarkerSeriesOutput output)
        {
            return new Lib.MarkerSeriesOutput
            {
                MarkerSize = Convert(output.MarkerSize),
            };
        }

        public static Lib.PluginPermissions Convert(PluginPermissions permissions)
        {
            return new Lib.PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static Lib.PluginConfig Convert(PluginConfig config)
        {
            var res = new Lib.PluginConfig
            {
                Key = Convert(config.Key),
                TimeFrame = Convert(config.TimeFrame),
                MainSymbol = Convert(config.MainSymbol),
                SelectedMapping = Convert(config.SelectedMapping),
                InstanceId = Convert(config.InstanceId),
                Permissions = Convert(config.Permissions),
            };
            res.Properties.AddRange(config.Properties.Select(Convert));
            switch (config)
            {
                case IndicatorConfig indicatorConfig:
                    res.Indicator = Convert(indicatorConfig);
                    break;
                case TradeBotConfig tradeBotConfig:
                    res.TradeBot = Convert(tradeBotConfig);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static Lib.IndicatorConfig Convert(IndicatorConfig config)
        {
            return new Lib.IndicatorConfig();
        }

        public static Lib.TradeBotConfig Convert(TradeBotConfig config)
        {
            return new Lib.TradeBotConfig();
        }

        #endregion config.proto


        #region keys.proto

        public static Lib.AccountKey Convert(AccountKey key)
        {
            return new Lib.AccountKey
            {
                Login = Convert(key.Login),
                Server = Convert(key.Server),
            };
        }

        public static Lib.RepositoryLocation Convert(RepositoryLocation location)
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

        public static Lib.PackageKey Convert(PackageKey key)
        {
            return new Lib.PackageKey
            {
                Name = Convert(key.Name),
                Location = Convert(key.Location),
            };
        }

        public static Lib.PluginKey Convert(PluginKey key)
        {
            return new Lib.PluginKey
            {
                PackageName = Convert(key.PackageName),
                PackageLocation = Convert(key.PackageLocation),
                DescriptorId = Convert(key.DescriptorId),
            };
        }

        public static Lib.ReductionKey Convert(ReductionKey key)
        {
            return new Lib.ReductionKey
            {
                PackageName = Convert(key.PackageName),
                PackageLocation = Convert(key.PackageLocation),
                DescriptorId = Convert(key.DescriptorId),
            };
        }

        public static Lib.MappingKey Convert(MappingKey key)
        {
            return new Lib.MappingKey
            {
                PrimaryReduction = Convert(key.PrimaryReduction),
                SecondaryReduction = Convert(key.SecondaryReduction),
            };
        }

        #endregion keys.proto


        #region metadata.proto

        public static Lib.PluginInfo Convert(PluginInfo plugin)
        {
            return new Lib.PluginInfo
            {
                Key = Convert(plugin.Key),
                Descriptor_ = Convert(plugin.Descriptor),
            };
        }

        public static Lib.PackageInfo Convert(PackageInfo package)
        {
            var res = new Lib.PackageInfo
            {
                Key = Convert(package.Key),
                CreatedUtc = Timestamp.FromDateTime(package.CreatedUtc),
                IsValid = package.IsValid,
            };
            res.Plugins.AddRange(package.Plugins.Select(Convert));
            return res;
        }

        public static Lib.ReductionInfo Convert(ReductionInfo reduction)
        {
            return new Lib.ReductionInfo
            {
                Key = Convert(reduction.Key),
                Descriptor_ = Convert(reduction.Descriptor),
            };
        }

        public static Lib.CurrencyInfo Convert(CurrencyInfo currency)
        {
            return new Lib.CurrencyInfo
            {
                Name = Convert(currency.Name),
            };
        }

        public static Lib.SymbolInfo Convert(SymbolInfo symbol)
        {
            return new Lib.SymbolInfo
            {
                Name = Convert(symbol.Name),
            };
        }

        public static Lib.AccountMetadataInfo Convert(AccountMetadataInfo accountMetadata)
        {
            var res = new Lib.AccountMetadataInfo
            {
                Key = Convert(accountMetadata.Key),
            };
            res.Symbols.AddRange(accountMetadata.Symbols.Select(Convert));
            return res;
        }

        public static Lib.MappingInfo Convert(MappingInfo mapping)
        {
            return new Lib.MappingInfo
            {
                Key = Convert(mapping.Key),
                DisplayName = Convert(mapping.DisplayName),
            };
        }

        public static Lib.MappingCollectionInfo Convert(MappingCollectionInfo mappings)
        {
            var res = new Lib.MappingCollectionInfo
            {
                DefaultFullBarToBarReduction = Convert(mappings.DefaultFullBarToBarReduction),
                DefaultFullBarToDoubleReduction = Convert(mappings.DefaultFullBarToDoubleReduction),
                DefaultBarToDoubleReduction = Convert(mappings.DefaultBarToDoubleReduction),
                DefaultQuoteToBarReduction = Convert(mappings.DefaultQuoteToBarReduction),
                DefaultQuoteToDoubleReduction = Convert(mappings.DefaultQuoteToDoubleReduction),
            };
            res.BarToBarMappings.AddRange(mappings.BarToBarMappings.Select(Convert));
            res.BarToDoubleMappings.AddRange(mappings.BarToDoubleMappings.Select(Convert));
            res.QuoteToBarMappings.AddRange(mappings.QuoteToBarMappings.Select(Convert));
            res.QuoteToDoubleMappings.AddRange(mappings.QuoteToDoubleMappings.Select(Convert));
            return res;
        }

        public static Lib.ApiMetadataInfo Convert(ApiMetadataInfo apiMetadata)
        {
            var res = new Lib.ApiMetadataInfo();
            res.TimeFrames.AddRange(apiMetadata.TimeFrames.Select(Convert));
            res.LineStyles.AddRange(apiMetadata.LineStyles.Select(Convert));
            res.Thicknesses.AddRange(apiMetadata.Thicknesses);
            res.MarkerSizes.AddRange(apiMetadata.MarkerSizes.Select(Convert));
            return res;
        }

        public static Lib.SetupContextInfo Convert(SetupContextInfo setupContext)
        {
            return new Lib.SetupContextInfo
            {
                DefaultTimeFrame = Convert(setupContext.DefaultTimeFrame),
                DefaultSymbolCode = Convert(setupContext.DefaultSymbolCode),
                DefaultMapping = Convert(setupContext.DefaultMapping),
            };
        }

        public static Lib.SetupMetadataInfo Convert(SetupMetadataInfo setupMetadata)
        {
            return new Lib.SetupMetadataInfo
            {
                Api = Convert(setupMetadata.Api),
                Mappings = Convert(setupMetadata.Mappings),
            };
        }

        public static Lib.ConnectionErrorInfo.Types.ConnectionErrorCode Convert(ConnectionErrorCodes code)
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

        public static Lib.ConnectionErrorInfo Convert(ConnectionErrorInfo error)
        {
            return new Lib.ConnectionErrorInfo
            {
                Code = Convert(error.Code),
                TextMessage = Convert(error.TextMessage),
            };
        }

        public static Lib.AccountModelInfo.Types.ConnectionState Convert(ConnectionStates state)
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

        public static Lib.AccountModelInfo Convert(AccountModelInfo account)
        {
            return new Lib.AccountModelInfo
            {
                Key = Convert(account.Key),
                UseNewProtocol = account.UseNewProtocol,
                ConnectionState = Convert(account.ConnectionState),
                LastError = Convert(account.LastError),
            };
        }

        public static Lib.BotModelInfo.Types.BotState Convert(BotStates state)
        {
            switch (state)
            {
                case BotStates.Offline:
                    return Lib.BotModelInfo.Types.BotState.Offline;
                case BotStates.Starting:
                    return Lib.BotModelInfo.Types.BotState.Starting;
                case BotStates.Faulted:
                    return Lib.BotModelInfo.Types.BotState.Faulted;
                case BotStates.Online:
                    return Lib.BotModelInfo.Types.BotState.Online;
                case BotStates.Stopping:
                    return Lib.BotModelInfo.Types.BotState.Stopping;
                case BotStates.Broken:
                    return Lib.BotModelInfo.Types.BotState.Broken;
                case BotStates.Reconnecting:
                    return Lib.BotModelInfo.Types.BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static Lib.BotModelInfo ConvertLight(BotModelInfo bot)
        {
            return new Lib.BotModelInfo
            {
                InstanceId = Convert(bot.InstanceId),
                Account = Convert(bot.Account),
                State = Convert(bot.State),
                FaultMessage = Convert(bot.FaultMessage),
            };
        }

        public static Lib.BotModelInfo Convert(BotModelInfo bot)
        {
            var res = ConvertLight(bot);
            res.Config = Convert(bot.Config);
            res.Descriptor_ = ConvertLight(bot.Descriptor);
            return res;
        }

        #endregion metadata.proto


        public static Lib.UpdateInfo.Types.UpdateType Convert(UpdateType type)
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
    }
}
