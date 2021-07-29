using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using Api = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public static class ServerConverters
    {
        public static AccountCreds ToServer(this Api.AccountCreds creds)
        {
            return new AccountCreds(creds.Secret[AccountCreds.PasswordKey]);
        }

        public static AddAccountRequest ToServer(this Api.AddAccountRequest request)
        {
            return new AddAccountRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToServer(),
                DisplayName = request.DisplayName,
            };
        }

        public static RemoveAccountRequest ToServer(this Api.RemoveAccountRequest request)
        {
            return new RemoveAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static ChangeAccountRequest ToServer(this Api.ChangeAccountRequest request)
        {
            return new ChangeAccountRequest
            {
                AccountId = request.AccountId,
                Creds = request.Creds.ToServer(),
                DisplayName = request.DisplayName,
            };
        }

        public static TestAccountRequest ToServer(this Api.TestAccountRequest request)
        {
            return new TestAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static TestAccountCredsRequest ToServer(this Api.TestAccountCredsRequest request)
        {
            return new TestAccountCredsRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToServer(),
            };
        }

        public static PluginKey ToServer(this Api.PluginKey key)
        {
            return new PluginKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Feed.Types.Timeframe ToServer(this Api.Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Api.Feed.Types.Timeframe.S1:
                    return Feed.Types.Timeframe.S1;

                case Api.Feed.Types.Timeframe.S10:
                    return Feed.Types.Timeframe.S10;

                case Api.Feed.Types.Timeframe.M5:
                    return Feed.Types.Timeframe.M5;

                case Api.Feed.Types.Timeframe.M15:
                    return Feed.Types.Timeframe.M15;

                case Api.Feed.Types.Timeframe.M30:
                    return Feed.Types.Timeframe.M30;

                case Api.Feed.Types.Timeframe.H1:
                    return Feed.Types.Timeframe.H1;

                case Api.Feed.Types.Timeframe.H4:
                    return Feed.Types.Timeframe.H4;

                case Api.Feed.Types.Timeframe.D:
                    return Feed.Types.Timeframe.D;

                case Api.Feed.Types.Timeframe.W:
                    return Feed.Types.Timeframe.W;

                case Api.Feed.Types.Timeframe.MN:
                    return Feed.Types.Timeframe.MN;

                case Api.Feed.Types.Timeframe.Ticks:
                    return Feed.Types.Timeframe.Ticks;

                case Api.Feed.Types.Timeframe.TicksLevel2:
                    return Feed.Types.Timeframe.TicksLevel2;

                case Api.Feed.Types.Timeframe.TicksVwap:
                    return Feed.Types.Timeframe.TicksVwap;

                default:
                    return Feed.Types.Timeframe.M1;
            }
        }

        public static SymbolConfig.Types.SymbolOrigin ToServer(this Api.SymbolConfig.Types.SymbolOrigin origin)
        {
            switch (origin)
            {
                case Api.SymbolConfig.Types.SymbolOrigin.Online:
                    return SymbolConfig.Types.SymbolOrigin.Online;

                case Api.SymbolConfig.Types.SymbolOrigin.Token:
                    return SymbolConfig.Types.SymbolOrigin.Token;

                case Api.SymbolConfig.Types.SymbolOrigin.Custom:
                    return SymbolConfig.Types.SymbolOrigin.Custom;

                default:
                    throw new ArgumentException();
            }
        }

        public static SymbolConfig ToServer(this Api.SymbolConfig config)
        {
            return new SymbolConfig
            {
                Name = config.Name,
                Origin = config.Origin.ToServer(),
            };
        }

        public static ReductionKey ToServer(this Api.ReductionKey key)
        {
            return new ReductionKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static MappingKey ToServer(this Api.MappingKey key)
        {
            return new MappingKey
            {
                PrimaryReduction = key.PrimaryReduction?.ToServer(),
                SecondaryReduction = key.SecondaryReduction?.ToServer(),
            };
        }

        public static PluginPermissions ToServer(this Api.PluginPermissions permissions)
        {
            return new PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static BoolParameterConfig ToServer(this Api.BoolParameterConfig config)
        {
            return new BoolParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Int32ParameterConfig ToServer(this Api.Int32ParameterConfig config)
        {
            return new Int32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static NullableInt32ParameterConfig ToServer(this Api.NullableInt32ParameterConfig config)
        {
            return new NullableInt32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static DoubleParameterConfig ToServer(this Api.DoubleParameterConfig config)
        {
            return new DoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static NullableDoubleParameterConfig ToServer(this Api.NullableDoubleParameterConfig config)
        {
            return new NullableDoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static StringParameterConfig ToServer(this Api.StringParameterConfig config)
        {
            return new StringParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static EnumParameterConfig ToServer(this Api.EnumParameterConfig config)
        {
            return new EnumParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static FileParameterConfig ToServer(this Api.FileParameterConfig config)
        {
            return new FileParameterConfig
            {
                PropertyId = config.PropertyId,
                FileName = config.FileName,
            };
        }

        public static BarToBarInputConfig ToServer(this Api.BarToBarInputConfig config)
        {
            return new BarToBarInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
            };
        }

        public static BarToDoubleInputConfig ToServer(this Api.BarToDoubleInputConfig config)
        {
            return new BarToDoubleInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
            };
        }

        public static ColoredLineOutputConfig ToServer(this Api.ColoredLineOutputConfig config)
        {
            return new ColoredLineOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                LineStyle = config.LineStyle.ToServer(),
            };
        }
        public static MarkerSeriesOutputConfig ToServer(this Api.MarkerSeriesOutputConfig config)
        {
            return new MarkerSeriesOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                MarkerSize = config.MarkerSize.ToServer(),
            };
        }

        public static Any ToServer(this Any payload)
        {
            IMessage message = payload;

            if (payload.Is(Api.BoolParameterConfig.Descriptor))
                message = payload.Unpack<Api.BoolParameterConfig>().ToServer();
            else if (payload.Is(Api.Int32ParameterConfig.Descriptor))
                message = payload.Unpack<Api.Int32ParameterConfig>().ToServer();
            else if (payload.Is(Api.NullableInt32ParameterConfig.Descriptor))
                message = payload.Unpack<Api.NullableInt32ParameterConfig>().ToServer();
            else if (payload.Is(Api.DoubleParameterConfig.Descriptor))
                message = payload.Unpack<Api.DoubleParameterConfig>().ToServer();
            else if (payload.Is(Api.NullableDoubleParameterConfig.Descriptor))
                message = payload.Unpack<Api.NullableDoubleParameterConfig>().ToServer();
            else if (payload.Is(Api.StringParameterConfig.Descriptor))
                message = payload.Unpack<Api.StringParameterConfig>().ToServer();
            else if (payload.Is(Api.EnumParameterConfig.Descriptor))
                message = payload.Unpack<Api.EnumParameterConfig>().ToServer();
            else if (payload.Is(Api.FileParameterConfig.Descriptor))
                message = payload.Unpack<Api.FileParameterConfig>().ToServer();
            else if (payload.Is(Api.BarToBarInputConfig.Descriptor))
                message = payload.Unpack<Api.BarToBarInputConfig>().ToServer();
            else if (payload.Is(Api.BarToDoubleInputConfig.Descriptor))
                message = payload.Unpack<Api.BarToDoubleInputConfig>().ToServer();
            else if (payload.Is(Api.ColoredLineOutputConfig.Descriptor))
                message = payload.Unpack<Api.ColoredLineOutputConfig>().ToServer();
            else if (payload.Is(Api.MarkerSeriesOutputConfig.Descriptor))
                message = payload.Unpack<Api.MarkerSeriesOutputConfig>().ToServer();

            return Any.Pack(message);
        }

        public static PluginConfig ToServer(this Api.PluginConfig config)
        {
            var serverConfig = new PluginConfig
            {
                Key = config.Key.ToServer(),
                Timeframe = config.Timeframe.ToServer(),
                ModelTimeframe = config.ModelTimeframe.ToServer(),
                MainSymbol = config.MainSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.ToServer()
            };

            serverConfig.Properties.AddRange(config.Properties.Select(ToServer));

            return serverConfig;
        }

        public static AddPluginRequest ToServer(this Api.AddPluginRequest request)
        {
            return new AddPluginRequest
            {
                AccountId = request.AccountId,
                Config = request.Config.ToServer(),
            };
        }

        public static RemovePluginRequest ToServer(this Api.RemovePluginRequest request)
        {
            return new RemovePluginRequest
            {
                PluginId = request.PluginId,
                CleanLog = request.CleanLog,
                CleanAlgoData = request.CleanAlgoData,
            };
        }

        public static StartPluginRequest ToServer(this Api.StartPluginRequest request)
        {
            return new StartPluginRequest
            {
                PluginId = request.PluginId,
            };
        }

        public static StopPluginRequest ToServer(this Api.StopPluginRequest request)
        {
            return new StopPluginRequest
            {
                PluginId = request.PluginId,
            };
        }

        public static ChangePluginConfigRequest ToServer(this Api.ChangePluginConfigRequest request)
        {
            return new ChangePluginConfigRequest
            {
                PluginId = request.PluginId,
                NewConfig = request.NewConfig.ToServer(),
            };
        }

        public static FileTransferSettings ToServer(this Api.FileTransferSettings settings)
        {
            return new FileTransferSettings
            {
                ChunkSize = settings.ChunkSize,
                ChunkOffset = settings.ChunkOffset,
            };
        }

        public static UploadPackageRequest ToServer(this Api.UploadPackageRequest request)
        {
            return new UploadPackageRequest
            {
                PackageId = request.PackageId,
                Filename = request.Filename,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static RemovePackageRequest ToServer(this Api.RemovePackageRequest request)
        {
            return new RemovePackageRequest
            {
                PackageId = request.PackageId,
            };
        }

        public static DownloadPackageRequest ToServer(this Api.DownloadPackageRequest request)
        {
            return new DownloadPackageRequest
            {
                PackageId = request.PackageId,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static PluginFolderInfo.Types.PluginFolderId ToServer(this Api.PluginFolderInfo.Types.PluginFolderId pluginFolderId)
        {
            switch (pluginFolderId)
            {
                case Api.PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return PluginFolderInfo.Types.PluginFolderId.AlgoData;

                case Api.PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return PluginFolderInfo.Types.PluginFolderId.BotLogs;

                default:
                    throw new ArgumentException();
            }
        }

        public static PluginFolderInfoRequest ToServer(this Api.PluginFolderInfoRequest request)
        {
            return new PluginFolderInfoRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
            };
        }

        public static ClearPluginFolderRequest ToServer(this Api.ClearPluginFolderRequest request)
        {
            return new ClearPluginFolderRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
            };
        }

        public static DeletePluginFileRequest ToServer(this Api.DeletePluginFileRequest request)
        {
            return new DeletePluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
            };
        }

        public static DownloadPluginFileRequest ToServer(this Api.DownloadPluginFileRequest request)
        {
            return new DownloadPluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static UploadPluginFileRequest ToServer(this Api.UploadPluginFileRequest request)
        {
            return new UploadPluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static ConnectionErrorInfo.Types.ErrorCode ToServer(this Api.ConnectionErrorInfo.Types.ErrorCode code)
        {
            switch (code)
            {
                case Api.ConnectionErrorInfo.Types.ErrorCode.NoConnectionError:
                    return ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;

                case Api.ConnectionErrorInfo.Types.ErrorCode.NetworkError:
                    return ConnectionErrorInfo.Types.ErrorCode.NetworkError;

                case Api.ConnectionErrorInfo.Types.ErrorCode.Timeout:
                    return ConnectionErrorInfo.Types.ErrorCode.Timeout;

                case Api.ConnectionErrorInfo.Types.ErrorCode.BlockedAccount:
                    return ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;

                case Api.ConnectionErrorInfo.Types.ErrorCode.ClientInitiated:
                    return ConnectionErrorInfo.Types.ErrorCode.ClientInitiated;

                case Api.ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials:
                    return ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;

                case Api.ConnectionErrorInfo.Types.ErrorCode.SlowConnection:
                    return ConnectionErrorInfo.Types.ErrorCode.SlowConnection;

                case Api.ConnectionErrorInfo.Types.ErrorCode.ServerError:
                    return ConnectionErrorInfo.Types.ErrorCode.ServerError;

                case Api.ConnectionErrorInfo.Types.ErrorCode.LoginDeleted:
                    return ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;

                case Api.ConnectionErrorInfo.Types.ErrorCode.ServerLogout:
                    return ConnectionErrorInfo.Types.ErrorCode.ServerLogout;

                case Api.ConnectionErrorInfo.Types.ErrorCode.Canceled:
                    return ConnectionErrorInfo.Types.ErrorCode.Canceled;

                case Api.ConnectionErrorInfo.Types.ErrorCode.RejectedByServer:
                    return ConnectionErrorInfo.Types.ErrorCode.RejectedByServer;

                default:
                    return ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }

        public static ConnectionErrorInfo ToServer(this Api.ConnectionErrorInfo info)
        {
            return new ConnectionErrorInfo
            {
                Code = info.Code.ToServer(),
                TextMessage = info.TextMessage,
            };
        }

        public static PluginFileInfo ToServer(this Api.PluginFileInfo info)
        {
            return new PluginFileInfo
            {
                Name = info.Name,
                Size = info.Size,
            };
        }

        public static PluginFolderInfo ToServer(this Api.PluginFolderInfo info)
        {
            var serverInfo = new PluginFolderInfo
            {
                PluginId = info.PluginId,
                FolderId = info.FolderId.ToServer(),
                Path = info.Path,
            };

            serverInfo.Files.AddRange(info.Files.Select(ToServer));

            return serverInfo;
        }

        public static AccountMetadataInfo ToServer(this Api.AccountMetadataInfo info)
        {
            var serverInfo = new AccountMetadataInfo
            {
                AccountId = info.AccountId,
                DefaultSymbol = info.DefaultSymbol.ToServer(),
            };

            serverInfo.Symbols.AddRange(info.Symbols.Select(ToServer));

            return serverInfo;
        }

        public static PackageIdentity ToServer(this Api.PackageIdentity identity)
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

        public static Metadata.Types.PluginType ToServer(this Api.Metadata.Types.PluginType type)
        {
            switch (type)
            {
                case Api.Metadata.Types.PluginType.Indicator:
                    return Metadata.Types.PluginType.Indicator;

                case Api.Metadata.Types.PluginType.TradeBot:
                    return Metadata.Types.PluginType.TradeBot;

                default:
                    return Metadata.Types.PluginType.UnknownPluginType;
            }
        }

        public static Metadata.Types.PropertyErrorCode ToServer(this Api.Metadata.Types.PropertyErrorCode type)
        {
            switch (type)
            {
                case Api.Metadata.Types.PropertyErrorCode.NoPropertyError:
                    return Metadata.Types.PropertyErrorCode.NoPropertyError;

                case Api.Metadata.Types.PropertyErrorCode.SetIsNotPublic:
                    return Metadata.Types.PropertyErrorCode.SetIsNotPublic;

                case Api.Metadata.Types.PropertyErrorCode.GetIsNotPublic:
                    return Metadata.Types.PropertyErrorCode.GetIsNotPublic;

                case Api.Metadata.Types.PropertyErrorCode.MultipleAttributes:
                    return Metadata.Types.PropertyErrorCode.MultipleAttributes;

                case Api.Metadata.Types.PropertyErrorCode.InputIsNotDataSeries:
                    return Metadata.Types.PropertyErrorCode.InputIsNotDataSeries;

                case Api.Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries:
                    return Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries;

                case Api.Metadata.Types.PropertyErrorCode.EmptyEnum:
                    return Metadata.Types.PropertyErrorCode.EmptyEnum;

                default:
                    return Metadata.Types.PropertyErrorCode.UnknownPropertyError;
            }
        }

        public static Metadata.Types.MetadataErrorCode ToServer(this Api.Metadata.Types.MetadataErrorCode type)
        {
            switch (type)
            {
                case Api.Metadata.Types.MetadataErrorCode.NoMetadataError:
                    return Metadata.Types.MetadataErrorCode.NoMetadataError;

                case Api.Metadata.Types.MetadataErrorCode.HasInvalidProperties:
                    return Metadata.Types.MetadataErrorCode.HasInvalidProperties;

                case Api.Metadata.Types.MetadataErrorCode.UnknownBaseType:
                    return Metadata.Types.MetadataErrorCode.UnknownBaseType;

                case Api.Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion:
                    return Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion;

                case Api.Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion:
                    return Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion;

                default:
                    return Metadata.Types.MetadataErrorCode.UnknownMetadataError;
            }
        }

        public static FileFilterEntry ToServer(this Api.FileFilterEntry entry)
        {
            return new FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static ParameterDescriptor ToServer(this Api.ParameterDescriptor descriptor)
        {
            var serverDescriptor = new ParameterDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToServer(),
                DataType = descriptor.DataType,
                DefaultValue = descriptor.DefaultValue,
                IsRequired = descriptor.IsRequired,
                IsEnum = descriptor.IsEnum
            };

            serverDescriptor.EnumValues.AddRange(descriptor.EnumValues);
            serverDescriptor.FileFilters.AddRange(descriptor.FileFilters.Select(ToServer));

            return serverDescriptor;
        }

        public static InputDescriptor ToServer(this Api.InputDescriptor descriptor)
        {
            return new InputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToServer(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
            };
        }

        public static Metadata.Types.LineStyle ToServer(this Api.Metadata.Types.LineStyle type)
        {
            switch (type)
            {
                case Api.Metadata.Types.LineStyle.Solid:
                    return Metadata.Types.LineStyle.Solid;

                case Api.Metadata.Types.LineStyle.Dots:
                    return Metadata.Types.LineStyle.Dots;

                case Api.Metadata.Types.LineStyle.DotsRare:
                    return Metadata.Types.LineStyle.DotsRare;

                case Api.Metadata.Types.LineStyle.DotsVeryRare:
                    return Metadata.Types.LineStyle.DotsVeryRare;

                case Api.Metadata.Types.LineStyle.Lines:
                    return Metadata.Types.LineStyle.Lines;

                case Api.Metadata.Types.LineStyle.LinesDots:
                    return Metadata.Types.LineStyle.LinesDots;

                default:
                    return Metadata.Types.LineStyle.UnknownLineStyle;
            }
        }

        public static Metadata.Types.PlotType ToServer(this Api.Metadata.Types.PlotType type)
        {
            switch (type)
            {
                case Api.Metadata.Types.PlotType.Line:
                    return Metadata.Types.PlotType.Line;

                case Api.Metadata.Types.PlotType.Histogram:
                    return Metadata.Types.PlotType.Histogram;

                case Api.Metadata.Types.PlotType.Points:
                    return Metadata.Types.PlotType.Points;

                case Api.Metadata.Types.PlotType.DiscontinuousLine:
                    return Metadata.Types.PlotType.DiscontinuousLine;

                default:
                    return Metadata.Types.PlotType.UnknownPlotType;
            }
        }

        public static Metadata.Types.OutputTarget ToServer(this Api.Metadata.Types.OutputTarget type)
        {
            switch (type)
            {
                case Api.Metadata.Types.OutputTarget.Overlay:
                    return Metadata.Types.OutputTarget.Overlay;

                case Api.Metadata.Types.OutputTarget.Window1:
                    return Metadata.Types.OutputTarget.Window1;

                case Api.Metadata.Types.OutputTarget.Window2:
                    return Metadata.Types.OutputTarget.Window2;

                case Api.Metadata.Types.OutputTarget.Window3:
                    return Metadata.Types.OutputTarget.Window3;

                case Api.Metadata.Types.OutputTarget.Window4:
                    return Metadata.Types.OutputTarget.Window4;

                default:
                    return Metadata.Types.OutputTarget.UnknownOutputTarget;
            }
        }

        public static OutputDescriptor ToServer(this Api.OutputDescriptor descriptor)
        {
            return new OutputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToServer(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
                DefaultThickness = descriptor.DefaultThickness,
                DefaultColorArgb = descriptor.DefaultColorArgb,
                DefaultLineStyle = descriptor.DefaultLineStyle.ToServer(),
                PlotType = descriptor.PlotType.ToServer(),
                Target = descriptor.Target.ToServer(),
                Precision = descriptor.Precision,
                ZeroLine = descriptor.ZeroLine,
                Visibility = descriptor.Visibility,
            };
        }

        public static PluginDescriptor ToServer(this Api.PluginDescriptor descriptor)
        {
            var serverDescriptor = new PluginDescriptor
            {
                ApiVersionStr = descriptor.ApiVersionStr,
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToServer(),
                Error = descriptor.Error.ToServer(),
                UiDisplayName = descriptor.UiDisplayName,
                Category = descriptor.Category,
                Version = descriptor.Version,
                Description = descriptor.Description,
                Copyright = descriptor.Copyright,
                SetupMainSymbol = descriptor.SetupMainSymbol,
            };

            serverDescriptor.Parameters.AddRange(descriptor.Parameters.Select(ToServer));
            serverDescriptor.Inputs.AddRange(descriptor.Inputs.Select(ToServer));
            serverDescriptor.Outputs.AddRange(descriptor.Outputs.Select(ToServer));

            return serverDescriptor;
        }

        public static PluginInfo ToServer(this Api.PluginInfo info)
        {
            return new PluginInfo
            {
                Key = info.Key.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static Metadata.Types.ReductionType ToServer(this Api.Metadata.Types.ReductionType type)
        {
            switch (type)
            {
                case Api.Metadata.Types.ReductionType.BarToDouble:
                    return Metadata.Types.ReductionType.BarToDouble;

                case Api.Metadata.Types.ReductionType.FullBarToBar:
                    return Metadata.Types.ReductionType.FullBarToBar;

                case Api.Metadata.Types.ReductionType.FullBarToDouble:
                    return Metadata.Types.ReductionType.FullBarToDouble;

                default:
                    return Metadata.Types.ReductionType.UnknownReductionType;
            }
        }

        public static ReductionDescriptor ToServer(this Api.ReductionDescriptor descriptor)
        {
            return new ReductionDescriptor
            {
                ApiVersionStr = descriptor.ApiVersionStr,
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToServer(),
            };
        }

        public static ReductionInfo ToServer(this Api.ReductionInfo info)
        {
            return new ReductionInfo
            {
                Key = info.Key.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static PackageInfo ToServer(this Api.PackageInfo info)
        {
            var serverInfo = new PackageInfo
            {
                PackageId = info.PackageId,
                Identity = info.Identity.ToServer(),
                IsValid = info.IsValid,
                IsLocked = info.IsLocked,
            };

            serverInfo.Plugins.AddRange(info.Plugins.Select(ToServer));
            serverInfo.Reductions.AddRange(info.Reductions.Select(ToServer));

            return serverInfo;
        }

        public static AccountModelInfo.Types.ConnectionState ToServer(this Api.AccountModelInfo.Types.ConnectionState state)
        {
            switch(state)
            {
                case Api.AccountModelInfo.Types.ConnectionState.Offline:
                    return AccountModelInfo.Types.ConnectionState.Offline;

                case Api.AccountModelInfo.Types.ConnectionState.Connecting:
                    return AccountModelInfo.Types.ConnectionState.Connecting;

                case Api.AccountModelInfo.Types.ConnectionState.Online:
                    return AccountModelInfo.Types.ConnectionState.Online;

                case Api.AccountModelInfo.Types.ConnectionState.Disconnecting:
                    return AccountModelInfo.Types.ConnectionState.Disconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static AccountModelInfo ToServer(this Api.AccountModelInfo info)
        {
            return new AccountModelInfo
            {
                AccountId = info.AccountId,
                ConnectionState = info.ConnectionState.ToServer(),
                LastError = info.LastError?.ToServer(),
                DisplayName = info.DisplayName,
            };
        }

        public static PluginModelInfo.Types.PluginState ToServer(this Api.PluginModelInfo.Types.PluginState state)
        {
            switch (state)
            {
                case Api.PluginModelInfo.Types.PluginState.Stopped:
                    return PluginModelInfo.Types.PluginState.Stopped;

                case Api.PluginModelInfo.Types.PluginState.Starting:
                    return PluginModelInfo.Types.PluginState.Starting;

                case Api.PluginModelInfo.Types.PluginState.Faulted:
                    return PluginModelInfo.Types.PluginState.Faulted;

                case Api.PluginModelInfo.Types.PluginState.Running:
                    return PluginModelInfo.Types.PluginState.Running;

                case Api.PluginModelInfo.Types.PluginState.Stopping:
                    return PluginModelInfo.Types.PluginState.Stopping;

                case Api.PluginModelInfo.Types.PluginState.Broken:
                    return PluginModelInfo.Types.PluginState.Broken;

                case Api.PluginModelInfo.Types.PluginState.Reconnecting:
                    return PluginModelInfo.Types.PluginState.Reconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static PluginModelInfo ToServer(this Api.PluginModelInfo info)
        {
            return new PluginModelInfo
            {
                InstanceId = info.InstanceId,
                AccountId = info.AccountId,
                State = info.State.ToServer(),
                FaultMessage = info.FaultMessage,
                Config = info.Config.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static Metadata.Types.MarkerSize ToServer(this Api.Metadata.Types.MarkerSize state)
        {
            switch (state)
            {
                case Api.Metadata.Types.MarkerSize.Large:
                    return Metadata.Types.MarkerSize.Large;

                case Api.Metadata.Types.MarkerSize.Medium:
                    return Metadata.Types.MarkerSize.Medium;

                case Api.Metadata.Types.MarkerSize.Small:
                    return Metadata.Types.MarkerSize.Small;

                default:
                    return Metadata.Types.MarkerSize.UnknownMarkerSize;
            };
        }

        public static ApiMetadataInfo ToServer(this Api.ApiMetadataInfo info)
        {
            var serverMetadata = new ApiMetadataInfo();

            serverMetadata.TimeFrames.AddRange(info.TimeFrames.Select(ToServer));
            serverMetadata.LineStyles.AddRange(info.LineStyles.Select(ToServer));
            serverMetadata.Thicknesses.AddRange(info.Thicknesses);
            serverMetadata.MarkerSizes.AddRange(info.MarkerSizes.Select(ToServer));

            return serverMetadata;
        }

        public static MappingInfo ToServer(this Api.MappingInfo info)
        {
            return new MappingInfo
            {
                Key = info.Key.ToServer(),
                DisplayName = info.DisplayName,
            };
        }

        public static MappingCollectionInfo ToServer(this Api.MappingCollectionInfo info)
        {
            var collectionInfo = new MappingCollectionInfo
            {
                DefaultBarToBarMapping = info.DefaultBarToBarMapping.ToServer(),
                DefaultBarToDoubleMapping = info.DefaultBarToDoubleMapping.ToServer(),
            };

            collectionInfo.BarToBarMappings.AddRange(info.BarToBarMappings.Select(ToServer));
            collectionInfo.BarToDoubleMappings.AddRange(info.BarToDoubleMappings.Select(ToServer));

            return collectionInfo;
        }

        public static SetupContextInfo ToServer(this Api.SetupContextInfo info)
        {
            return new SetupContextInfo
            {
                DefaultTimeFrame = info.DefaultTimeFrame.ToServer(),
                DefaultSymbol = info.DefaultSymbol.ToServer(),
                DefaultMapping = info.DefaultMapping.ToServer(),
            };
        }

        public static PluginStateUpdate ToServer(this Api.PluginStateUpdate update)
        {
            return new PluginStateUpdate
            {
                Id = update.Id,
                State = update.State.ToServer(),
                FaultMessage = update.FaultMessage,
            };
        }

        public static AccountMetadataRequest ToServer(this Api.AccountMetadataRequest request)
        {
            return new AccountMetadataRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static PluginLogRecord.Types.LogSeverity ToServer(this Api.PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case Api.PluginLogRecord.Types.LogSeverity.Info:
                    return PluginLogRecord.Types.LogSeverity.Info;

                case Api.PluginLogRecord.Types.LogSeverity.Error:
                    return PluginLogRecord.Types.LogSeverity.Error;

                case Api.PluginLogRecord.Types.LogSeverity.Trade:
                    return PluginLogRecord.Types.LogSeverity.Trade;

                case Api.PluginLogRecord.Types.LogSeverity.TradeSuccess:
                    return PluginLogRecord.Types.LogSeverity.TradeSuccess;

                case Api.PluginLogRecord.Types.LogSeverity.TradeFail:
                    return PluginLogRecord.Types.LogSeverity.TradeFail;

                case Api.PluginLogRecord.Types.LogSeverity.Custom:
                    return PluginLogRecord.Types.LogSeverity.Custom;

                case Api.PluginLogRecord.Types.LogSeverity.Alert:
                    return PluginLogRecord.Types.LogSeverity.Alert;

                default:
                    throw new ArgumentException();
            }
        }

        public static LogRecordInfo ToServer(this Api.LogRecordInfo info)
        {
            return new LogRecordInfo
            {
                TimeUtc = info.TimeUtc,
                Severity = info.Severity.ToServer(),
                Message = info.Message,
            };
        }

        public static AlertRecordInfo ToServer(this Api.AlertRecordInfo info)
        {
            return new AlertRecordInfo
            {
                Message = info.Message,
                PluginId = info.PluginId,
                TimeUtc = info.TimeUtc,
            };
        }
    }
}
