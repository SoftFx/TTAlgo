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
        #region descriptors.proto

        public static TimeFrames Convert(Lib.ApiDescriptor.Types.TimeFrame timeFrame)
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

        public static LineStyles Convert(Lib.ApiDescriptor.Types.LineStyle style)
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

        public static PlotType Convert(Lib.ApiDescriptor.Types.PlotType type)
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

        public static OutputTargets Convert(Lib.ApiDescriptor.Types.OutputTarget target)
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

        public static MarkerSizes Convert(Lib.ApiDescriptor.Types.MarkerSize markerSize)
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

        public static AlgoPropertyTypes Convert(Lib.PropertyDescriptor.Types.AlgoPropertyType type)
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

        public static AlgoPropertyErrors Convert(Lib.PropertyDescriptor.Types.AlgoPropertyError error)
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

        public static PropertyDescriptor Convert(Lib.PropertyDescriptor descriptor)
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
            res.Error = Convert(descriptor.Error);
            return res;
        }

        public static FileFilterEntry Convert(Lib.FileFilterEntry entry)
        {
            return new FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static ParameterDescriptor Convert(Lib.ParameterDescriptor parameter)
        {
            var res = (ParameterDescriptor)Convert(parameter.PropertyHeader);
            res.DataType = parameter.DataType;
            res.DefaultValue = parameter.DefaultValue;
            res.IsRequired = parameter.IsRequired;
            res.IsEnum = parameter.IsEnum;
            res.EnumValues.AddRange(parameter.EnumValues);
            res.FileFilters.AddRange(parameter.FileFilters.Select(Convert));
            return res;
        }

        public static InputDescriptor Convert(Lib.InputDescriptor input)
        {
            var res = (InputDescriptor)Convert(input.PropertyHeader);
            res.DataSeriesBaseTypeFullName = input.DataSeriesBaseTypeFullName;
            return res;
        }

        public static OutputDescriptor Convert(Lib.OutputDescriptor output)
        {
            var res = (OutputDescriptor)Convert(output.PropertyHeader);
            res.DataSeriesBaseTypeFullName = output.DataSeriesBaseTypeFullName;
            res.DefaultThickness = output.DefaultThickness;
            res.DefaultColor = (Colors)output.DefaultColor;
            res.DefaultLineStyle = Convert(output.DefaultLineStyle);
            res.PlotType = Convert(output.PlotType);
            res.Target = Convert(output.Target);
            res.Precision = output.Precision;
            res.ZeroLine = output.ZeroLine;
            return res;
        }

        public static AlgoTypes Convert(Lib.PluginDescriptor.Types.AlgoType type)
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

        public static AlgoMetadataErrors Convert(Lib.PluginDescriptor.Types.AlgoMetadataError error)
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

        public static PluginDescriptor ConvertLight(Lib.PluginDescriptor plugin)
        {
            var res = new PluginDescriptor
            {
                ApiVersionStr = plugin.ApiVersionStr,
                Id = plugin.Id,
                DisplayName = plugin.DisplayName,
                Type = Convert(plugin.Type),
                Error = Convert(plugin.Error),
                UiDisplayName = plugin.UiDisplayName,
                Category = plugin.Category,
                Version = plugin.Version,
                Description = plugin.Description,
                Copyright = plugin.Copyright,
                SetupMainSymbol = plugin.SetupMainSymbol,
            };
            return res;
        }

        public static PluginDescriptor Convert(Lib.PluginDescriptor plugin)
        {
            var res = ConvertLight(plugin);
            res.Parameters.AddRange(plugin.Parameters.Select(Convert));
            res.Inputs.AddRange(plugin.Inputs.Select(Convert));
            res.Outputs.AddRange(plugin.Outputs.Select(Convert));
            return res;
        }

        public static ReductionType Convert(Lib.ReductionDescriptor.Types.ReductionType type)
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

        public static ReductionDescriptor Convert(Lib.ReductionDescriptor reduction)
        {
            return new ReductionDescriptor
            {
                ApiVersionStr = reduction.ApiVersionStr,
                Id = reduction.Id,
                DisplayName = reduction.DisplayName,
                Type = Convert(reduction.Type),
            };
        }

        #endregion descriptors.proto


        #region config.proto

        public static Property Convert(Lib.Property property)
        {
            Property res;
            switch (property.PropertyCase)
            {
                case Lib.Property.PropertyOneofCase.Parameter:
                    res = Convert(property.Parameter);
                    break;
                case Lib.Property.PropertyOneofCase.Input:
                    res = Convert(property.Input);
                    break;
                case Lib.Property.PropertyOneofCase.Output:
                    res = Convert(property.Output);
                    break;
                default:
                    throw new ArgumentException();
            }
            res.Id = property.PropertyId;
            return res;
        }

        public static Parameter Convert(Lib.Parameter parameter)
        {
            Parameter res;
            switch (parameter.ParameterCase)
            {
                case Lib.Parameter.ParameterOneofCase.Bool:
                    res = Convert(parameter.Bool);
                    break;
                case Lib.Parameter.ParameterOneofCase.Int:
                    res = Convert(parameter.Int);
                    break;
                case Lib.Parameter.ParameterOneofCase.NullInt:
                    res = Convert(parameter.NullInt);
                    break;
                case Lib.Parameter.ParameterOneofCase.Double:
                    res = Convert(parameter.Double);
                    break;
                case Lib.Parameter.ParameterOneofCase.NullDouble:
                    res = Convert(parameter.NullDouble);
                    break;
                case Lib.Parameter.ParameterOneofCase.String:
                    res = Convert(parameter.String);
                    break;
                case Lib.Parameter.ParameterOneofCase.Enum:
                    res = Convert(parameter.Enum);
                    break;
                case Lib.Parameter.ParameterOneofCase.File:
                    res = Convert(parameter.File);
                    break;
                default:
                    throw new ArgumentException();
            }
            return res;
        }

        public static BoolParameter Convert(Lib.BoolParameter param)
        {
            return new BoolParameter
            {
                Value = param.Value,
            };
        }

        public static IntParameter Convert(Lib.IntParameter param)
        {
            return new IntParameter
            {
                Value = param.Value,
            };
        }

        public static NullableIntParameter Convert(Lib.NullableIntParameter param)
        {
            return new NullableIntParameter
            {
                Value = param.Value?.Value,
            };
        }

        public static DoubleParameter Convert(Lib.DoubleParameter param)
        {
            return new DoubleParameter
            {
                Value = param.Value,
            };
        }

        public static NullableDoubleParameter Convert(Lib.NullableDoubleParameter param)
        {
            return new NullableDoubleParameter
            {
                Value = param.Value?.Value,
            };
        }

        public static StringParameter Convert(Lib.StringParameter param)
        {
            return new StringParameter
            {
                Value = param.Value,
            };
        }

        public static EnumParameter Convert(Lib.EnumParameter param)
        {
            return new EnumParameter
            {
                Value = param.Value,
            };
        }

        public static FileParameter Convert(Lib.FileParameter param)
        {
            return new FileParameter
            {
                FileName = param.FileName,
            };
        }

        public static Input Convert(Lib.Input input)
        {
            Input res;
            switch (input.InputCase)
            {
                case Lib.Input.InputOneofCase.Quote:
                    res = Convert(input.Quote);
                    break;
                case Lib.Input.InputOneofCase.Mapped:
                    res = Convert(input.Mapped);
                    break;
                default:
                    throw new ArgumentException();
            }
            res.SelectedSymbol = input.SelectedSymbol;
            return res;
        }

        public static QuoteInput Convert(Lib.QuoteInput input)
        {
            return new QuoteInput
            {
                UseL2 = input.UseL2,
            };
        }

        public static MappedInput Convert(Lib.MappedInput input)
        {
            MappedInput res;
            switch (input.InputCase)
            {
                case Lib.MappedInput.InputOneofCase.BarToBar:
                    res = Convert(input.BarToBar);
                    break;
                case Lib.MappedInput.InputOneofCase.BarToDouble:
                    res = Convert(input.BarToDouble);
                    break;
                case Lib.MappedInput.InputOneofCase.QuoteToBar:
                    res = Convert(input.QuoteToBar);
                    break;
                case Lib.MappedInput.InputOneofCase.QuoteToDouble:
                    res = Convert(input.QuoteToDouble);
                    break;
                default:
                    throw new ArgumentException();
            }
            res.SelectedMapping = Convert(input.SelectedMapping);
            return res;
        }

        public static BarToBarInput Convert(Lib.BarToBarInput input)
        {
            return new BarToBarInput();
        }

        public static BarToDoubleInput Convert(Lib.BarToDoubleInput input)
        {
            return new BarToDoubleInput();
        }

        public static QuoteToBarInput Convert(Lib.QuoteToBarInput input)
        {
            return new QuoteToBarInput();
        }

        public static QuoteToDoubleInput Convert(Lib.QuoteToDoubleInput input)
        {
            return new QuoteToDoubleInput();
        }

        public static OutputColor Convert(Lib.OutputColor color)
        {
            return new OutputColor
            {
                Alpha = color.Alpha,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
            };
        }

        public static Output Convert(Lib.Output output)
        {
            Output res;
            switch (output.OutputCase)
            {
                case Lib.Output.OutputOneofCase.ColoredLine:
                    res = Convert(output.ColoredLine);
                    break;
                case Lib.Output.OutputOneofCase.MarkerSeries:
                    res = Convert(output.MarkerSeries);
                    break;
                default:
                    throw new ArgumentException();
            }
            res.IsEnabled = output.IsEnabled;
            res.LineColor = Convert(output.LineColor);
            res.LineThickness = output.LineThickness;
            return res;
        }

        public static ColoredLineOutput Convert(Lib.ColoredLineOutput output)
        {
            return new ColoredLineOutput
            {
                LineStyle = Convert(output.LineStyle),
            };
        }

        public static MarkerSeriesOutput Convert(Lib.MarkerSeriesOutput output)
        {
            return new MarkerSeriesOutput
            {
                MarkerSize = Convert(output.MarkerSize),
            };
        }

        public static PluginPermissions Convert(Lib.PluginPermissions permissions)
        {
            return new PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static PluginConfig Convert(Lib.PluginConfig config)
        {
            PluginConfig res;
            switch (config.ConfigCase)
            {
                case Lib.PluginConfig.ConfigOneofCase.Indicator:
                    res = Convert(config.Indicator);
                    break;
                case Lib.PluginConfig.ConfigOneofCase.TradeBot:
                    res = Convert(config.TradeBot);
                    break;
                default:
                    throw new ArgumentException();
            }
            res.Key = Convert(config.Key);
            res.TimeFrame = Convert(config.TimeFrame);
            res.MainSymbol = config.MainSymbol;
            res.SelectedMapping = Convert(config.SelectedMapping);
            res.InstanceId = config.InstanceId;
            res.Permissions = Convert(config.Permissions);
            res.Properties.AddRange(config.Properties.Select(Convert));
            return res;
        }

        public static IndicatorConfig Convert(Lib.IndicatorConfig config)
        {
            return new IndicatorConfig();
        }

        public static TradeBotConfig Convert(Lib.TradeBotConfig config)
        {
            return new TradeBotConfig();
        }

        #endregion config.proto


        #region keys.proto

        public static AccountKey Convert(Lib.AccountKey key)
        {
            return new AccountKey
            {
                Login = key.Login,
                Server = key.Server,
            };
        }

        public static RepositoryLocation Convert(Lib.RepositoryLocation location)
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

        public static PackageKey Convert(Lib.PackageKey key)
        {
            return new PackageKey
            {
                Name = key.Name,
                Location = Convert(key.Location),
            };
        }

        public static PluginKey Convert(Lib.PluginKey key)
        {
            return new PluginKey
            {
                PackageName = key.PackageName,
                PackageLocation = Convert(key.PackageLocation),
                DescriptorId = key.DescriptorId,
            };
        }

        public static ReductionKey Convert(Lib.ReductionKey key)
        {
            return new ReductionKey
            {
                PackageName = key.PackageName,
                PackageLocation = Convert(key.PackageLocation),
                DescriptorId = key.DescriptorId,
            };
        }

        public static MappingKey Convert(Lib.MappingKey key)
        {
            return new MappingKey
            {
                PrimaryReduction = Convert(key.PrimaryReduction),
                SecondaryReduction = Convert(key.SecondaryReduction),
            };
        }

        #endregion keys.proto


        #region metadata.proto

        public static PluginInfo Convert(Lib.PluginInfo plugin)
        {
            return new PluginInfo
            {
                Key = Convert(plugin.Key),
                Descriptor = Convert(plugin.Descriptor_),
            };
        }

        public static PackageInfo Convert(Lib.PackageInfo package)
        {
            var res = new PackageInfo
            {
                Key = Convert(package.Key),
                CreatedUtc = package.CreatedUtc.ToDateTime(),
                IsValid = package.IsValid,
            };
            res.Plugins.AddRange(package.Plugins.Select(Convert));
            return res;
        }

        public static ReductionInfo Convert(Lib.ReductionInfo reduction)
        {
            return new ReductionInfo
            {
                Key = Convert(reduction.Key),
                Descriptor = Convert(reduction.Descriptor_),
            };
        }

        public static CurrencyInfo Convert(Lib.CurrencyInfo currency)
        {
            return new CurrencyInfo
            {
                Name = currency.Name,
            };
        }

        public static SymbolInfo Convert(Lib.SymbolInfo symbol)
        {
            return new SymbolInfo
            {
                Name = symbol.Name,
            };
        }

        public static AccountMetadataInfo Convert(Lib.AccountMetadataInfo accountMetadata)
        {
            var res = new AccountMetadataInfo
            {
                Key = Convert(accountMetadata.Key),
            };
            res.Symbols.AddRange(accountMetadata.Symbols.Select(Convert));
            return res;
        }

        public static MappingInfo Convert(Lib.MappingInfo mapping)
        {
            return new MappingInfo
            {
                Key = Convert(mapping.Key),
                DisplayName = mapping.DisplayName,
            };
        }

        public static MappingCollectionInfo Convert(Lib.MappingCollectionInfo mappings)
        {
            var res = new MappingCollectionInfo
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

        public static ApiMetadataInfo Convert(Lib.ApiMetadataInfo apiMetadata)
        {
            var res = new ApiMetadataInfo();
            res.TimeFrames.AddRange(apiMetadata.TimeFrames.Select(Convert));
            res.LineStyles.AddRange(apiMetadata.LineStyles.Select(Convert));
            res.Thicknesses.AddRange(apiMetadata.Thicknesses);
            res.MarkerSizes.AddRange(apiMetadata.MarkerSizes.Select(Convert));
            return res;
        }

        public static SetupContextInfo Convert(Lib.SetupContextInfo setupContext)
        {
            return new SetupContextInfo
            {
                DefaultTimeFrame = Convert(setupContext.DefaultTimeFrame),
                DefaultSymbolCode = setupContext.DefaultSymbolCode,
                DefaultMapping = Convert(setupContext.DefaultMapping),
            };
        }

        public static SetupMetadataInfo Convert(Lib.SetupMetadataInfo setupMetadata)
        {
            return new SetupMetadataInfo
            {
                Api = Convert(setupMetadata.Api),
                Mappings = Convert(setupMetadata.Mappings),
            };
        }

        public static ConnectionErrorCodes Convert(Lib.ConnectionErrorInfo.Types.ConnectionErrorCode code)
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

        public static ConnectionErrorInfo Convert(Lib.ConnectionErrorInfo error)
        {
            return new ConnectionErrorInfo(Convert(error.Code), error.TextMessage);
        }

        public static ConnectionStates Convert(Lib.AccountModelInfo.Types.ConnectionState state)
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

        public static AccountModelInfo Convert(Lib.AccountModelInfo account)
        {
            return new AccountModelInfo
            {
                Key = Convert(account.Key),
                UseNewProtocol = account.UseNewProtocol,
                ConnectionState = Convert(account.ConnectionState),
                LastError = Convert(account.LastError),
            };
        }

        public static BotStates Convert(Lib.BotModelInfo.Types.BotState state)
        {
            switch (state)
            {
                case Lib.BotModelInfo.Types.BotState.Offline:
                    return BotStates.Offline;
                case Lib.BotModelInfo.Types.BotState.Starting:
                    return BotStates.Starting;
                case Lib.BotModelInfo.Types.BotState.Faulted:
                    return BotStates.Faulted;
                case Lib.BotModelInfo.Types.BotState.Online:
                    return BotStates.Online;
                case Lib.BotModelInfo.Types.BotState.Stopping:
                    return BotStates.Stopping;
                case Lib.BotModelInfo.Types.BotState.Broken:
                    return BotStates.Broken;
                case Lib.BotModelInfo.Types.BotState.Reconnecting:
                    return BotStates.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static BotModelInfo ConvertLight(Lib.BotModelInfo bot)
        {
            return new BotModelInfo
            {
                InstanceId = bot.InstanceId,
                Account = Convert(bot.Account),
                State = Convert(bot.State),
                FaultMessage = bot.FaultMessage,
            };
        }

        public static BotModelInfo Convert(Lib.BotModelInfo bot)
        {
            var res = ConvertLight(bot);
            res.Config = Convert(bot.Config);
            res.Descriptor = ConvertLight(bot.Descriptor_);
            return res;
        }

        #endregion metadata.proto


        public static UpdateType Convert(Lib.UpdateInfo.Types.UpdateType type)
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
    }
}
