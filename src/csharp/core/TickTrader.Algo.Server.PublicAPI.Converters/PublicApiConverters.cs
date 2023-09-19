using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerApi = TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server.PublicAPI.Converters
{
    public static class PublicApiConverters
    {
        public static ConnectionErrorInfo.Types.ErrorCode ToApi(this Domain.ConnectionErrorInfo.Types.ErrorCode code)
        {
            switch (code)
            {
                case Domain.ConnectionErrorInfo.Types.ErrorCode.NoConnectionError:
                    return ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.NetworkError:
                    return ConnectionErrorInfo.Types.ErrorCode.NetworkError;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.Timeout:
                    return ConnectionErrorInfo.Types.ErrorCode.Timeout;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.BlockedAccount:
                    return ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.ClientInitiated:
                    return ConnectionErrorInfo.Types.ErrorCode.ClientInitiated;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials:
                    return ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.SlowConnection:
                    return ConnectionErrorInfo.Types.ErrorCode.SlowConnection;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.ServerError:
                    return ConnectionErrorInfo.Types.ErrorCode.ServerError;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.LoginDeleted:
                    return ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.ServerLogout:
                    return ConnectionErrorInfo.Types.ErrorCode.ServerLogout;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.Canceled:
                    return ConnectionErrorInfo.Types.ErrorCode.Canceled;

                case Domain.ConnectionErrorInfo.Types.ErrorCode.RejectedByServer:
                    return ConnectionErrorInfo.Types.ErrorCode.RejectedByServer;

                default:
                    return ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }

        public static ConnectionErrorInfo ToApi(this Domain.ConnectionErrorInfo info)
        {
            return new ConnectionErrorInfo
            {
                Code = info.Code.ToApi(),
                TextMessage = info.TextMessage,
            };
        }

        public static PluginFolderInfo.Types.PluginFolderId ToApi(this Domain.PluginFolderInfo.Types.PluginFolderId pluginFolderId)
        {
            switch (pluginFolderId)
            {
                case Domain.PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return PluginFolderInfo.Types.PluginFolderId.AlgoData;

                case Domain.PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return PluginFolderInfo.Types.PluginFolderId.BotLogs;

                default:
                    throw new ArgumentException();
            }
        }

        public static PluginFileInfo ToApi(this Domain.PluginFileInfo info)
        {
            return new PluginFileInfo
            {
                Name = info.Name,
                Size = info.Size,
            };
        }

        public static PluginFolderInfo ToApi(this Domain.PluginFolderInfo info)
        {
            var apiInfo = new PluginFolderInfo
            {
                PluginId = info.PluginId,
                FolderId = info.FolderId.ToApi(),
                Path = info.Path
            };

            apiInfo.Files.AddRange(info.Files.Select(u => u.ToApi()));

            return apiInfo;
        }

        public static PluginKey ToApi(this Domain.PluginKey key)
        {
            return new PluginKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Feed.Types.Timeframe ToApi(this Domain.Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Domain.Feed.Types.Timeframe.S1:
                    return Feed.Types.Timeframe.S1;

                case Domain.Feed.Types.Timeframe.S10:
                    return Feed.Types.Timeframe.S10;

                case Domain.Feed.Types.Timeframe.M5:
                    return Feed.Types.Timeframe.M5;

                case Domain.Feed.Types.Timeframe.M15:
                    return Feed.Types.Timeframe.M15;

                case Domain.Feed.Types.Timeframe.M30:
                    return Feed.Types.Timeframe.M30;

                case Domain.Feed.Types.Timeframe.H1:
                    return Feed.Types.Timeframe.H1;

                case Domain.Feed.Types.Timeframe.H4:
                    return Feed.Types.Timeframe.H4;

                case Domain.Feed.Types.Timeframe.D:
                    return Feed.Types.Timeframe.D;

                case Domain.Feed.Types.Timeframe.W:
                    return Feed.Types.Timeframe.W;

                case Domain.Feed.Types.Timeframe.MN:
                    return Feed.Types.Timeframe.MN;

                case Domain.Feed.Types.Timeframe.Ticks:
                    return Feed.Types.Timeframe.Ticks;

                case Domain.Feed.Types.Timeframe.TicksLevel2:
                    return Feed.Types.Timeframe.TicksLevel2;

                case Domain.Feed.Types.Timeframe.TicksVwap:
                    return Feed.Types.Timeframe.TicksVwap;

                default:
                    return Feed.Types.Timeframe.M1;
            }
        }

        public static SymbolConfig.Types.SymbolOrigin ToApi(this Domain.SymbolConfig.Types.SymbolOrigin origin)
        {
            switch (origin)
            {
                case Domain.SymbolConfig.Types.SymbolOrigin.Online:
                    return SymbolConfig.Types.SymbolOrigin.Online;

                case Domain.SymbolConfig.Types.SymbolOrigin.Token:
                    return SymbolConfig.Types.SymbolOrigin.Token;

                case Domain.SymbolConfig.Types.SymbolOrigin.Custom:
                    return SymbolConfig.Types.SymbolOrigin.Custom;

                default:
                    throw new ArgumentException();
            }
        }

        public static SymbolConfig ToApi(this Domain.SymbolConfig config)
        {
            return new SymbolConfig
            {
                Name = config.Name,
                Origin = config.Origin.ToApi(),
            };
        }

        public static ReductionKey ToApi(this Domain.ReductionKey key)
        {
            return new ReductionKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static MappingKey ToApi(this Domain.MappingKey key)
        {
            return new MappingKey
            {
                PrimaryReduction = key.PrimaryReduction?.ToApi(),
                SecondaryReduction = key.SecondaryReduction?.ToApi(),
            };
        }

        public static PluginPermissions ToApi(this Domain.PluginPermissions permissions)
        {
            return new PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static BoolParameterConfig ToApi(this Domain.BoolParameterConfig config)
        {
            return new BoolParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Int32ParameterConfig ToApi(this Domain.Int32ParameterConfig config)
        {
            return new Int32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static NullableInt32ParameterConfig ToApi(this Domain.NullableInt32ParameterConfig config)
        {
            return new NullableInt32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static DoubleParameterConfig ToApi(this Domain.DoubleParameterConfig config)
        {
            return new DoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static NullableDoubleParameterConfig ToApi(this Domain.NullableDoubleParameterConfig config)
        {
            return new NullableDoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static StringParameterConfig ToApi(this Domain.StringParameterConfig config)
        {
            return new StringParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static EnumParameterConfig ToApi(this Domain.EnumParameterConfig config)
        {
            return new EnumParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static FileParameterConfig ToApi(this Domain.FileParameterConfig config)
        {
            return new FileParameterConfig
            {
                PropertyId = config.PropertyId,
                FileName = config.FileName,
            };
        }

        public static BarToBarInputConfig ToApi(this Domain.BarToBarInputConfig config)
        {
            return new BarToBarInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToApi(),
                SelectedMapping = config.SelectedMapping.ToApi(),
            };
        }

        public static BarToDoubleInputConfig ToApi(this Domain.BarToDoubleInputConfig config)
        {
            return new BarToDoubleInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToApi(),
                SelectedMapping = config.SelectedMapping.ToApi(),
            };
        }

        public static ColoredLineOutputConfig ToApi(this Domain.ColoredLineOutputConfig config)
        {
            return new ColoredLineOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                LineStyle = config.LineStyle.ToApi(),
            };
        }
        public static MarkerSeriesOutputConfig ToApi(this Domain.MarkerSeriesOutputConfig config)
        {
            return new MarkerSeriesOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                MarkerSize = config.MarkerSize.ToApi(),
            };
        }

        public static Any ToApi(this Any payload)
        {
            IMessage message;

            if (payload.Is(Domain.BoolParameterConfig.Descriptor))
                message = payload.Unpack<Domain.BoolParameterConfig>().ToApi();
            else if (payload.Is(Domain.Int32ParameterConfig.Descriptor))
                message = payload.Unpack<Domain.Int32ParameterConfig>().ToApi();
            else if (payload.Is(Domain.NullableInt32ParameterConfig.Descriptor))
                message = payload.Unpack<Domain.NullableInt32ParameterConfig>().ToApi();
            else if (payload.Is(Domain.DoubleParameterConfig.Descriptor))
                message = payload.Unpack<Domain.DoubleParameterConfig>().ToApi();
            else if (payload.Is(Domain.NullableDoubleParameterConfig.Descriptor))
                message = payload.Unpack<Domain.NullableDoubleParameterConfig>().ToApi();
            else if (payload.Is(Domain.StringParameterConfig.Descriptor))
                message = payload.Unpack<Domain.StringParameterConfig>().ToApi();
            else if (payload.Is(Domain.EnumParameterConfig.Descriptor))
                message = payload.Unpack<Domain.EnumParameterConfig>().ToApi();
            else if (payload.Is(Domain.FileParameterConfig.Descriptor))
                message = payload.Unpack<Domain.FileParameterConfig>().ToApi();
            else if (payload.Is(Domain.BarToBarInputConfig.Descriptor))
                message = payload.Unpack<Domain.BarToBarInputConfig>().ToApi();
            else if (payload.Is(Domain.BarToDoubleInputConfig.Descriptor))
                message = payload.Unpack<Domain.BarToDoubleInputConfig>().ToApi();
            else if (payload.Is(Domain.ColoredLineOutputConfig.Descriptor))
                message = payload.Unpack<Domain.ColoredLineOutputConfig>().ToApi();
            else if (payload.Is(Domain.MarkerSeriesOutputConfig.Descriptor))
                message = payload.Unpack<Domain.MarkerSeriesOutputConfig>().ToApi();
            else
                throw new ArgumentException($"Unsupported type {payload}");

            return Any.Pack(message);
        }

        public static PluginConfig ToApi(this Domain.PluginConfig config)
        {
            if (config == null)
                return new PluginConfig();

            var apiConfig = new PluginConfig
            {
                Key = config.Key.ToApi(),
                Timeframe = config.Timeframe.ToApi(),
                ModelTimeframe = config.ModelTimeframe.ToApi(),
                MainSymbol = config.MainSymbol.ToApi(),
                SelectedMapping = config.SelectedMapping.ToApi(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.ToApi()
            };

            apiConfig.Properties.AddRange(config.Properties.Select(ToApi));

            return apiConfig;
        }

        public static AccountCreds ToApi(this Domain.AccountCreds creds)
        {
            return new AccountCreds(creds.AuthScheme, creds.Secret);
        }

        public static AddAccountRequest ToApi(this ServerApi.AddAccountRequest request)
        {
            return new AddAccountRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToApi(),
                DisplayName = request.DisplayName,
            };
        }

        public static RemoveAccountRequest ToApi(this ServerApi.RemoveAccountRequest request)
        {
            return new RemoveAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static ChangeAccountRequest ToApi(this ServerApi.ChangeAccountRequest request)
        {
            return new ChangeAccountRequest
            {
                AccountId = request.AccountId,
                Creds = request.Creds.ToApi(),
                DisplayName = request.DisplayName,
            };
        }

        public static TestAccountRequest ToApi(this ServerApi.TestAccountRequest request)
        {
            return new TestAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static TestAccountCredsRequest ToApi(this ServerApi.TestAccountCredsRequest request)
        {
            return new TestAccountCredsRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToApi(),
            };
        }

        public static Metadata.Types.LineStyle ToApi(this Domain.Metadata.Types.LineStyle type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.LineStyle.Solid:
                    return Metadata.Types.LineStyle.Solid;

                case Domain.Metadata.Types.LineStyle.Dots:
                    return Metadata.Types.LineStyle.Dots;

                case Domain.Metadata.Types.LineStyle.DotsRare:
                    return Metadata.Types.LineStyle.DotsRare;

                case Domain.Metadata.Types.LineStyle.DotsVeryRare:
                    return Metadata.Types.LineStyle.DotsVeryRare;

                case Domain.Metadata.Types.LineStyle.Lines:
                    return Metadata.Types.LineStyle.Lines;

                case Domain.Metadata.Types.LineStyle.LinesDots:
                    return Metadata.Types.LineStyle.LinesDots;

                default:
                    return Metadata.Types.LineStyle.UnknownLineStyle;
            }
        }

        public static Metadata.Types.MarkerSize ToApi(this Domain.Metadata.Types.MarkerSize state)
        {
            switch (state)
            {
                case Domain.Metadata.Types.MarkerSize.Large:
                    return Metadata.Types.MarkerSize.Large;

                case Domain.Metadata.Types.MarkerSize.Medium:
                    return Metadata.Types.MarkerSize.Medium;

                case Domain.Metadata.Types.MarkerSize.Small:
                    return Metadata.Types.MarkerSize.Small;

                default:
                    return Metadata.Types.MarkerSize.UnknownMarkerSize;
            };
        }

        public static ApiMetadataInfo ToApi(this Domain.ApiMetadataInfo info)
        {
            var apiMetadata = new ApiMetadataInfo();

            apiMetadata.TimeFrames.AddRange(info.TimeFrames.Select(ToApi));
            apiMetadata.LineStyles.AddRange(info.LineStyles.Select(ToApi));
            apiMetadata.Thicknesses.AddRange(info.Thicknesses);
            apiMetadata.MarkerSizes.AddRange(info.MarkerSizes.Select(ToApi));

            return apiMetadata;
        }

        public static MappingInfo ToApi(this Domain.MappingInfo info)
        {
            return new MappingInfo
            {
                Key = info.Key.ToApi(),
                DisplayName = info.DisplayName,
            };
        }

        public static MappingCollectionInfo ToApi(this Domain.MappingCollectionInfo info)
        {
            var collectionInfo = new MappingCollectionInfo
            {
                DefaultBarToBarMapping = info.DefaultBarToBarMapping.ToApi(),
                DefaultBarToDoubleMapping = info.DefaultBarToDoubleMapping.ToApi(),
            };

            collectionInfo.BarToBarMappings.AddRange(info.BarToBarMappings.Select(ToApi));
            collectionInfo.BarToDoubleMappings.AddRange(info.BarToDoubleMappings.Select(ToApi));

            return collectionInfo;
        }

        public static SetupContextInfo ToApi(this Domain.SetupContextInfo info)
        {
            return new SetupContextInfo
            {
                DefaultTimeFrame = info.DefaultTimeFrame.ToApi(),
                DefaultSymbol = info.DefaultSymbol.ToApi(),
                DefaultMapping = info.DefaultMapping.ToApi(),
            };
        }

        public static PackageIdentity ToApi(this Domain.PackageIdentity identity)
        {
            return new PackageIdentity
            {
                FileName = identity.FileName,
                FilePath = identity.FilePath,
                CreatedUtc = identity.CreatedUtc,
                LastModifiedUtc = identity.LastModifiedUtc,
                Size = identity.Size,
                Hash = identity.Hash,
            };
        }

        public static PluginDescriptor.Types.PluginType ToApi(this Domain.Metadata.Types.PluginType type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.PluginType.Indicator:
                    return PluginDescriptor.Types.PluginType.Indicator;

                case Domain.Metadata.Types.PluginType.TradeBot:
                    return PluginDescriptor.Types.PluginType.TradeBot;

                default:
                    return PluginDescriptor.Types.PluginType.UnknownPluginType;
            }
        }

        public static PluginDescriptor.Types.PluginErrorCode ToApi(this Domain.Metadata.Types.MetadataErrorCode type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.MetadataErrorCode.NoMetadataError:
                    return PluginDescriptor.Types.PluginErrorCode.NoMetadataError;

                case Domain.Metadata.Types.MetadataErrorCode.HasInvalidProperties:
                    return PluginDescriptor.Types.PluginErrorCode.HasInvalidProperties;

                case Domain.Metadata.Types.MetadataErrorCode.UnknownBaseType:
                    return PluginDescriptor.Types.PluginErrorCode.UnknownBaseType;

                case Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion:
                    return PluginDescriptor.Types.PluginErrorCode.IncompatibleApiNewerVersion;

                case Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion:
                    return PluginDescriptor.Types.PluginErrorCode.IncompatibleApiOlderVersion;

                default:
                    return PluginDescriptor.Types.PluginErrorCode.UnknownMetadataError;
            }
        }

        public static ParameterDescriptor.Types.ParameterErrorCode ToApi(this Domain.Metadata.Types.PropertyErrorCode type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.PropertyErrorCode.NoPropertyError:
                    return ParameterDescriptor.Types.ParameterErrorCode.NoPropertyError;

                case Domain.Metadata.Types.PropertyErrorCode.SetIsNotPublic:
                    return ParameterDescriptor.Types.ParameterErrorCode.SetIsNotPublic;

                case Domain.Metadata.Types.PropertyErrorCode.GetIsNotPublic:
                    return ParameterDescriptor.Types.ParameterErrorCode.GetIsNotPublic;

                case Domain.Metadata.Types.PropertyErrorCode.MultipleAttributes:
                    return ParameterDescriptor.Types.ParameterErrorCode.MultipleAttributes;

                case Domain.Metadata.Types.PropertyErrorCode.InputIsNotDataSeries:
                    return ParameterDescriptor.Types.ParameterErrorCode.InputIsNotDataSeries;

                case Domain.Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries:
                    return ParameterDescriptor.Types.ParameterErrorCode.OutputIsNotDataSeries;

                case Domain.Metadata.Types.PropertyErrorCode.EmptyEnum:
                    return ParameterDescriptor.Types.ParameterErrorCode.EmptyEnum;

                default:
                    return ParameterDescriptor.Types.ParameterErrorCode.UnknownPropertyError;
            }
        }

        public static FileFilterEntry ToApi(this Domain.FileFilterEntry entry)
        {
            return new FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static ParameterDescriptor ToApi(this Domain.ParameterDescriptor descriptor)
        {
            var apiDescriptor = new ParameterDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToApi(),
                DataType = descriptor.DataType,
                DefaultValue = descriptor.DefaultValue,
                IsRequired = descriptor.IsRequired,
                IsEnum = descriptor.IsEnum
            };

            apiDescriptor.EnumValues.AddRange(descriptor.EnumValues);
            apiDescriptor.FileFilters.AddRange(descriptor.FileFilters.Select(ToApi));

            return apiDescriptor;
        }

        public static InputDescriptor ToApi(this Domain.InputDescriptor descriptor)
        {
            return new InputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToApi(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
            };
        }

        public static Metadata.Types.PlotType ToApi(this Domain.Metadata.Types.PlotType type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.PlotType.Line:
                    return Metadata.Types.PlotType.Line;

                case Domain.Metadata.Types.PlotType.Histogram:
                    return Metadata.Types.PlotType.Histogram;

                case Domain.Metadata.Types.PlotType.Points:
                    return Metadata.Types.PlotType.Points;

                case Domain.Metadata.Types.PlotType.DiscontinuousLine:
                    return Metadata.Types.PlotType.DiscontinuousLine;

                default:
                    return Metadata.Types.PlotType.UnknownPlotType;
            }
        }

        public static Metadata.Types.OutputTarget ToApi(this Domain.Metadata.Types.OutputTarget type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.OutputTarget.Overlay:
                    return Metadata.Types.OutputTarget.Overlay;

                case Domain.Metadata.Types.OutputTarget.Window1:
                    return Metadata.Types.OutputTarget.Window1;

                case Domain.Metadata.Types.OutputTarget.Window2:
                    return Metadata.Types.OutputTarget.Window2;

                case Domain.Metadata.Types.OutputTarget.Window3:
                    return Metadata.Types.OutputTarget.Window3;

                case Domain.Metadata.Types.OutputTarget.Window4:
                    return Metadata.Types.OutputTarget.Window4;

                default:
                    return Metadata.Types.OutputTarget.UnknownOutputTarget;
            }
        }

        public static OutputDescriptor ToApi(this Domain.OutputDescriptor descriptor)
        {
            return new OutputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToApi(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
                DefaultThickness = descriptor.DefaultThickness,
                DefaultColorArgb = descriptor.DefaultColorArgb,
                DefaultLineStyle = descriptor.DefaultLineStyle.ToApi(),
                PlotType = descriptor.PlotType.ToApi(),
                Target = descriptor.Target.ToApi(),
                Precision = descriptor.Precision,
                ZeroLine = descriptor.ZeroLine,
                Visibility = descriptor.Visibility,
            };
        }

        public static PluginDescriptor ToApi(this Domain.PluginDescriptor descriptor)
        {
            if (descriptor != null)
            {
                var apiDescriptor = new PluginDescriptor
                {
                    ApiVersionStr = descriptor.ApiVersionStr,
                    Id = descriptor.Id,
                    DisplayName = descriptor.DisplayName,
                    Type = descriptor.Type.ToApi(),
                    Error = descriptor.Error.ToApi(),
                    UiDisplayName = descriptor.UiDisplayName,
                    Category = descriptor.Category,
                    Version = descriptor.Version,
                    Description = descriptor.Description,
                    Copyright = descriptor.Copyright,
                    SetupMainSymbol = descriptor.SetupMainSymbol,
                };

                apiDescriptor.Parameters.AddRange(descriptor.Parameters.Select(ToApi));
                apiDescriptor.Inputs.AddRange(descriptor.Inputs.Select(ToApi));
                apiDescriptor.Outputs.AddRange(descriptor.Outputs.Select(ToApi));

                return apiDescriptor;
            }

            return new PluginDescriptor();
        }

        public static PluginInfo ToApi(this Domain.PluginInfo info)
        {
            return new PluginInfo
            {
                Key = info.Key.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static ReductionDescriptor.Types.ReductionType ToApi(this Domain.Metadata.Types.ReductionType type)
        {
            switch (type)
            {
                case Domain.Metadata.Types.ReductionType.BarToDouble:
                    return ReductionDescriptor.Types.ReductionType.BarToDouble;

                case Domain.Metadata.Types.ReductionType.FullBarToBar:
                    return ReductionDescriptor.Types.ReductionType.FullBarToBar;

                case Domain.Metadata.Types.ReductionType.FullBarToDouble:
                    return ReductionDescriptor.Types.ReductionType.FullBarToDouble;

                default:
                    return ReductionDescriptor.Types.ReductionType.UnknownReductionType;
            }
        }

        public static ReductionDescriptor ToApi(this Domain.ReductionDescriptor descriptor)
        {
            return new ReductionDescriptor
            {
                ApiVersionStr = descriptor.ApiVersionStr,
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToApi(),
            };
        }

        public static ReductionInfo ToApi(this Domain.ReductionInfo info)
        {
            return new ReductionInfo
            {
                Key = info.Key.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static PackageInfo ToApi(this Domain.PackageInfo info)
        {
            var apiInfo = new PackageInfo
            {
                PackageId = info.PackageId,
                Identity = info.Identity?.ToApi(),
                IsValid = info.IsValid,
                IsLocked = info.IsLocked,
            };

            apiInfo.Plugins.AddRange(info.Plugins.Select(ToApi));
            apiInfo.Reductions.AddRange(info.Reductions.Select(ToApi));

            return apiInfo;
        }

        public static AccountModelInfo.Types.ConnectionState ToApi(this Domain.AccountModelInfo.Types.ConnectionState state)
        {
            switch (state)
            {
                case Domain.AccountModelInfo.Types.ConnectionState.Offline:
                    return AccountModelInfo.Types.ConnectionState.Offline;

                case Domain.AccountModelInfo.Types.ConnectionState.Connecting:
                    return AccountModelInfo.Types.ConnectionState.Connecting;

                case Domain.AccountModelInfo.Types.ConnectionState.Online:
                    return AccountModelInfo.Types.ConnectionState.Online;

                case Domain.AccountModelInfo.Types.ConnectionState.Disconnecting:
                    return AccountModelInfo.Types.ConnectionState.Disconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static AccountModelInfo ToApi(this Domain.AccountModelInfo info)
        {
            return new AccountModelInfo
            {
                AccountId = info.AccountId,
                ConnectionState = info.ConnectionState.ToApi(),
                LastError = info.LastError?.ToApi(),
                DisplayName = info.DisplayName,
            };
        }

        public static PluginModelInfo.Types.PluginState ToApi(this Domain.PluginModelInfo.Types.PluginState state)
        {
            switch (state)
            {
                case Domain.PluginModelInfo.Types.PluginState.Stopped:
                    return PluginModelInfo.Types.PluginState.Stopped;

                case Domain.PluginModelInfo.Types.PluginState.Starting:
                    return PluginModelInfo.Types.PluginState.Starting;

                case Domain.PluginModelInfo.Types.PluginState.Faulted:
                    return PluginModelInfo.Types.PluginState.Faulted;

                case Domain.PluginModelInfo.Types.PluginState.Running:
                    return PluginModelInfo.Types.PluginState.Running;

                case Domain.PluginModelInfo.Types.PluginState.Stopping:
                    return PluginModelInfo.Types.PluginState.Stopping;

                case Domain.PluginModelInfo.Types.PluginState.Broken:
                    return PluginModelInfo.Types.PluginState.Broken;

                case Domain.PluginModelInfo.Types.PluginState.Reconnecting:
                    return PluginModelInfo.Types.PluginState.Reconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static PluginModelInfo ToApi(this Domain.PluginModelInfo info)
        {
            return new PluginModelInfo
            {
                InstanceId = info.InstanceId,
                AccountId = info.AccountId,
                State = info.State.ToApi(),
                FaultMessage = info.FaultMessage,
                Config = info.Config.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static AccountMetadataInfo ToApi(this Domain.AccountMetadataInfo info)
        {
            var response = new AccountMetadataInfo
            {
                AccountId = info.AccountId,
                DefaultSymbol = info.DefaultSymbol.ToApi(),
            };

            response.Symbols.AddRange(info.Symbols.Select(ToApi));

            return response;
        }

        public static PackageUpdate ToApi(this Domain.PackageUpdate update)
        {
            return new PackageUpdate
            {
                Id = update.Id,
                Action = update.Action.ToApi(),
                Package = update.Package?.ToApi(),
            };
        }

        public static AccountModelUpdate ToApi(this Domain.AccountModelUpdate update)
        {
            return new AccountModelUpdate
            {
                Id = update.Id,
                Action = update.Action.ToApi(),
                Account = update.Account?.ToApi(),
            };
        }

        public static PluginModelUpdate ToApi(this Domain.PluginModelUpdate update)
        {
            return new PluginModelUpdate
            {
                Id = update.Id,
                Action = update.Action.ToApi(),
                Plugin = update.Plugin?.ToApi(),
            };
        }

        public static PackageStateUpdate ToApi(this Domain.PackageStateUpdate update)
        {
            return new PackageStateUpdate
            {
                Id = update.Id,
                IsLocked = update.IsLocked,
            };
        }

        public static AccountStateUpdate ToApi(this Domain.AccountStateUpdate update)
        {
            return new AccountStateUpdate
            {
                Id = update.Id,
                ConnectionState = update.ConnectionState.ToApi(),
                LastError = update.LastError.ToApi(),
            };
        }

        public static PluginStateUpdate ToApi(this Domain.PluginStateUpdate update)
        {
            return new PluginStateUpdate
            {
                Id = update.Id,
                State = update.State.ToApi(),
                FaultMessage = update.FaultMessage,
            };
        }

        public static Update.Types.Action ToApi(this Domain.Update.Types.Action type)
        {
            switch (type)
            {
                case Domain.Update.Types.Action.Added:
                    return Update.Types.Action.Added;

                case Domain.Update.Types.Action.Updated:
                    return Update.Types.Action.Updated;

                case Domain.Update.Types.Action.Removed:
                    return Update.Types.Action.Removed;

                default:
                    throw new ArgumentException();
            }
        }

        public static LogRecordInfo.Types.LogSeverity ToApi(this Domain.PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case Domain.PluginLogRecord.Types.LogSeverity.Info:
                    return LogRecordInfo.Types.LogSeverity.Info;

                case Domain.PluginLogRecord.Types.LogSeverity.Error:
                    return LogRecordInfo.Types.LogSeverity.Error;

                case Domain.PluginLogRecord.Types.LogSeverity.Trade:
                    return LogRecordInfo.Types.LogSeverity.Trade;

                case Domain.PluginLogRecord.Types.LogSeverity.TradeSuccess:
                    return LogRecordInfo.Types.LogSeverity.TradeSuccess;

                case Domain.PluginLogRecord.Types.LogSeverity.TradeFail:
                    return LogRecordInfo.Types.LogSeverity.TradeFail;

                case Domain.PluginLogRecord.Types.LogSeverity.Custom:
                    return LogRecordInfo.Types.LogSeverity.Custom;

                case Domain.PluginLogRecord.Types.LogSeverity.Alert:
                    return LogRecordInfo.Types.LogSeverity.Alert;

                default:
                    throw new ArgumentException();
            }
        }

        public static LogRecordInfo ToApi(this Domain.LogRecordInfo info)
        {
            return new LogRecordInfo
            {
                TimeUtc = info.TimeUtc,
                Severity = info.Severity.ToApi(),
                Message = info.Message,
            };
        }

        public static AlertRecordInfo ToApi(this Domain.AlertRecordInfo info)
        {
            return new AlertRecordInfo
            {
                Message = info.Message,
                PluginId = info.PluginId,
                TimeUtc = info.TimeUtc,
                Type = info.Type.ToApi(),
            };
        }

        public static AlertRecordInfo.Types.AlertType ToApi(this Domain.AlertRecordInfo.Types.AlertType type)
        {
            switch (type)
            {
                case Domain.AlertRecordInfo.Types.AlertType.Plugin:
                    return AlertRecordInfo.Types.AlertType.Plugin;
                case Domain.AlertRecordInfo.Types.AlertType.Server:
                    return AlertRecordInfo.Types.AlertType.Server;
                case Domain.AlertRecordInfo.Types.AlertType.Monitoring:
                    return AlertRecordInfo.Types.AlertType.Monitoring;
                default:
                    throw new ArgumentException($"Unsupported alert type {type}");
            }
        }

        public static ServerVersionRequest ToApi(this ServerApi.ServerVersionRequest request)
        {
            return new ServerVersionRequest();
        }

        public static ServerVersionInfo ToApi(this ServerApi.ServerVersionInfo info)
        {
            return new ServerVersionInfo
            {
                Version = info.Version,
                ReleaseDate = info.ReleaseDate,
            };
        }

        public static ServerUpdateListRequest ToApi(this ServerApi.ServerUpdateListRequest request)
        {
            return new ServerUpdateListRequest { Forced = request.Forced };
        }

        public static ServerUpdateList ToApi(this ServerApi.ServerUpdateList request)
        {
            var res = new ServerUpdateList();
            res.Updates.AddRange(request.Updates.Select(u => u.ToApi()));
            res.Errors.Add(request.Errors);
            return res;
        }

        public static ServerUpdateInfo ToApi(this ServerApi.ServerUpdateInfo info)
        {
            return new ServerUpdateInfo
            {
                ReleaseId = info.ReleaseId,
                Version = info.Version,
                ReleaseDate = info.ReleaseDate,
                MinVersion = info.MinVersion,
                Changelog = info.Changelog,
                IsStable = info.IsStable,
            };
        }

        public static AutoUpdateEnums.Types.ServiceStatus ToApi(this ServerApi.AutoUpdateEnums.Types.ServiceStatus status)
        {
            switch (status)
            {
                case ServerApi.AutoUpdateEnums.Types.ServiceStatus.Idle:
                    return AutoUpdateEnums.Types.ServiceStatus.Idle;
                case ServerApi.AutoUpdateEnums.Types.ServiceStatus.Updating:
                    return AutoUpdateEnums.Types.ServiceStatus.Updating;
                case ServerApi.AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess:
                    return AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess;
                case ServerApi.AutoUpdateEnums.Types.ServiceStatus.UpdateFailed:
                    return AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;
                default:
                    throw new ArgumentException($"Unsupported service status {status}");
            }
        }

        public static UpdateServiceInfo ToApi(this ServerApi.UpdateServiceInfo info)
        {
            return new UpdateServiceInfo
            {
                Status = info.Status.ToApi(),
                StatusDetails = info.StatusDetails,
                UpdateLog = info.UpdateLog,
                HasNewVersion = info.HasNewVersion,
                NewVersion = info.NewVersion,
            };
        }

        public static StartUpdateResult ToApi(this ServerApi.StartServerUpdateResponse response)
        {
            return new StartUpdateResult
            {
                Started = response.Started,
                ErrorMsg = response.ErrorMsg,
            };
        }

        public static UpdateServiceStateUpdate ToApi(this ServerApi.UpdateServiceStateUpdate update)
        {
            return new UpdateServiceStateUpdate
            {
                Snapshot = update.Snapshot.ToApi(),
            };
        }
    }
}
