using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using TickTrader.Algo.Domain;
using ServerApi = TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server.PublicAPI.Converters
{
    public static class ServerConverters
    {
        public static Domain.AccountCreds ToServer(this AccountCreds creds)
        {
            return new Domain.AccountCreds(creds.Secret[Domain.AccountCreds.PasswordKey]);
        }

        public static ServerApi.AddAccountRequest ToServer(this AddAccountRequest request)
        {
            return new ServerApi.AddAccountRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToServer(),
                DisplayName = request.DisplayName,
            };
        }

        public static ServerApi.RemoveAccountRequest ToServer(this RemoveAccountRequest request)
        {
            return new ServerApi.RemoveAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static ServerApi.ChangeAccountRequest ToServer(this ChangeAccountRequest request)
        {
            return new ServerApi.ChangeAccountRequest
            {
                AccountId = request.AccountId,
                Creds = request.Creds.ToServer(),
                DisplayName = request.DisplayName,
            };
        }

        public static ServerApi.TestAccountRequest ToServer(this TestAccountRequest request)
        {
            return new ServerApi.TestAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static ServerApi.TestAccountCredsRequest ToServer(this TestAccountCredsRequest request)
        {
            return new ServerApi.TestAccountCredsRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToServer(),
            };
        }

        public static Domain.PluginKey ToServer(this PluginKey key)
        {
            return new Domain.PluginKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Domain.Feed.Types.Timeframe ToServer(this Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Feed.Types.Timeframe.S1:
                    return Domain.Feed.Types.Timeframe.S1;

                case Feed.Types.Timeframe.S10:
                    return Domain.Feed.Types.Timeframe.S10;

                case Feed.Types.Timeframe.M5:
                    return Domain.Feed.Types.Timeframe.M5;

                case Feed.Types.Timeframe.M15:
                    return Domain.Feed.Types.Timeframe.M15;

                case Feed.Types.Timeframe.M30:
                    return Domain.Feed.Types.Timeframe.M30;

                case Feed.Types.Timeframe.H1:
                    return Domain.Feed.Types.Timeframe.H1;

                case Feed.Types.Timeframe.H4:
                    return Domain.Feed.Types.Timeframe.H4;

                case Feed.Types.Timeframe.D:
                    return Domain.Feed.Types.Timeframe.D;

                case Feed.Types.Timeframe.W:
                    return Domain.Feed.Types.Timeframe.W;

                case Feed.Types.Timeframe.MN:
                    return Domain.Feed.Types.Timeframe.MN;

                case Feed.Types.Timeframe.Ticks:
                    return Domain.Feed.Types.Timeframe.Ticks;

                case Feed.Types.Timeframe.TicksLevel2:
                    return Domain.Feed.Types.Timeframe.TicksLevel2;

                case Feed.Types.Timeframe.TicksVwap:
                    return Domain.Feed.Types.Timeframe.TicksVwap;

                default: return Domain.Feed.Types.Timeframe.M1;
            }
        }

        public static Domain.SymbolConfig.Types.SymbolOrigin ToServer(this SymbolConfig.Types.SymbolOrigin origin)
        {
            switch (origin)
            {
                case SymbolConfig.Types.SymbolOrigin.Online:
                    return Domain.SymbolConfig.Types.SymbolOrigin.Online;

                case SymbolConfig.Types.SymbolOrigin.Token:
                    return Domain.SymbolConfig.Types.SymbolOrigin.Token;

                case SymbolConfig.Types.SymbolOrigin.Custom:
                    return Domain.SymbolConfig.Types.SymbolOrigin.Custom;

                default:
                    throw new ArgumentException();
            }
        }

        public static Domain.SymbolConfig ToServer(this SymbolConfig config)
        {
            return new Domain.SymbolConfig
            {
                Name = config.Name,
                Origin = config.Origin.ToServer(),
            };
        }

        public static Domain.ReductionKey ToServer(this ReductionKey key)
        {
            return new Domain.ReductionKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Domain.MappingKey ToServer(this MappingKey key)
        {
            return new Domain.MappingKey
            {
                PrimaryReduction = key.PrimaryReduction?.ToServer(),
                SecondaryReduction = key.SecondaryReduction?.ToServer(),
            };
        }

        public static Domain.PluginPermissions ToServer(this PluginPermissions permissions)
        {
            return new Domain.PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static Domain.BoolParameterConfig ToServer(this BoolParameterConfig config)
        {
            return new Domain.BoolParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.Int32ParameterConfig ToServer(this Int32ParameterConfig config)
        {
            return new Domain.Int32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.NullableInt32ParameterConfig ToServer(this NullableInt32ParameterConfig config)
        {
            return new Domain.NullableInt32ParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.DoubleParameterConfig ToServer(this DoubleParameterConfig config)
        {
            return new Domain.DoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.NullableDoubleParameterConfig ToServer(this NullableDoubleParameterConfig config)
        {
            return new Domain.NullableDoubleParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.StringParameterConfig ToServer(this StringParameterConfig config)
        {
            return new Domain.StringParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.EnumParameterConfig ToServer(this EnumParameterConfig config)
        {
            return new Domain.EnumParameterConfig
            {
                PropertyId = config.PropertyId,
                Value = config.Value,
            };
        }

        public static Domain.FileParameterConfig ToServer(this FileParameterConfig config)
        {
            return new Domain.FileParameterConfig
            {
                PropertyId = config.PropertyId,
                FileName = config.FileName,
            };
        }

        public static Domain.BarToBarInputConfig ToServer(this BarToBarInputConfig config)
        {
            return new Domain.BarToBarInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
            };
        }

        public static Domain.BarToDoubleInputConfig ToServer(this BarToDoubleInputConfig config)
        {
            return new Domain.BarToDoubleInputConfig
            {
                PropertyId = config.PropertyId,
                SelectedSymbol = config.SelectedSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
            };
        }

        public static Domain.ColoredLineOutputConfig ToServer(this ColoredLineOutputConfig config)
        {
            return new Domain.ColoredLineOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                LineStyle = config.LineStyle.ToServer(),
            };
        }
        public static Domain.MarkerSeriesOutputConfig ToServer(this MarkerSeriesOutputConfig config)
        {
            return new Domain.MarkerSeriesOutputConfig
            {
                PropertyId = config.PropertyId,
                IsEnabled = config.IsEnabled,
                LineColorArgb = config.LineColorArgb,
                LineThickness = config.LineThickness,
                MarkerSize = config.MarkerSize.ToServer(),
            };
        }

        public static IPropertyConfig ToServer(this Any payload)
        {
            IPropertyConfig message = default;

            if (payload.Is(BoolParameterConfig.Descriptor))
                message = payload.Unpack<BoolParameterConfig>().ToServer();
            else if (payload.Is(Int32ParameterConfig.Descriptor))
                message = payload.Unpack<Int32ParameterConfig>().ToServer();
            else if (payload.Is(NullableInt32ParameterConfig.Descriptor))
                message = payload.Unpack<NullableInt32ParameterConfig>().ToServer();
            else if (payload.Is(DoubleParameterConfig.Descriptor))
                message = payload.Unpack<DoubleParameterConfig>().ToServer();
            else if (payload.Is(NullableDoubleParameterConfig.Descriptor))
                message = payload.Unpack<NullableDoubleParameterConfig>().ToServer();
            else if (payload.Is(StringParameterConfig.Descriptor))
                message = payload.Unpack<StringParameterConfig>().ToServer();
            else if (payload.Is(EnumParameterConfig.Descriptor))
                message = payload.Unpack<EnumParameterConfig>().ToServer();
            else if (payload.Is(FileParameterConfig.Descriptor))
                message = payload.Unpack<FileParameterConfig>().ToServer();
            else if (payload.Is(BarToBarInputConfig.Descriptor))
                message = payload.Unpack<BarToBarInputConfig>().ToServer();
            else if (payload.Is(BarToDoubleInputConfig.Descriptor))
                message = payload.Unpack<BarToDoubleInputConfig>().ToServer();
            else if (payload.Is(ColoredLineOutputConfig.Descriptor))
                message = payload.Unpack<ColoredLineOutputConfig>().ToServer();
            else if (payload.Is(MarkerSeriesOutputConfig.Descriptor))
                message = payload.Unpack<MarkerSeriesOutputConfig>().ToServer();

            return message;
        }

        public static Domain.PluginConfig ToServer(this PluginConfig config)
        {
            var serverConfig = new Domain.PluginConfig
            {
                Key = config.Key.ToServer(),
                Timeframe = config.Timeframe.ToServer(),
                ModelTimeframe = config.ModelTimeframe.ToServer(),
                MainSymbol = config.MainSymbol.ToServer(),
                SelectedMapping = config.SelectedMapping.ToServer(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.ToServer()
            };

            serverConfig.PackProperties(config.Properties.Select(ToServer));

            return serverConfig;
        }

        public static ServerApi.AddPluginRequest ToServer(this AddPluginRequest request)
        {
            return new ServerApi.AddPluginRequest
            {
                AccountId = request.AccountId,
                Config = request.Config.ToServer(),
            };
        }

        public static ServerApi.RemovePluginRequest ToServer(this RemovePluginRequest request)
        {
            return new ServerApi.RemovePluginRequest
            {
                PluginId = request.PluginId,
                CleanLog = request.CleanLog,
                CleanAlgoData = request.CleanAlgoData,
            };
        }

        public static ServerApi.StartPluginRequest ToServer(this StartPluginRequest request)
        {
            return new ServerApi.StartPluginRequest
            {
                PluginId = request.PluginId,
            };
        }

        public static ServerApi.StopPluginRequest ToServer(this StopPluginRequest request)
        {
            return new ServerApi.StopPluginRequest
            {
                PluginId = request.PluginId,
            };
        }

        public static ServerApi.ChangePluginConfigRequest ToServer(this ChangePluginConfigRequest request)
        {
            return new ServerApi.ChangePluginConfigRequest
            {
                PluginId = request.PluginId,
                NewConfig = request.NewConfig.ToServer(),
            };
        }

        public static ServerApi.FileTransferSettings ToServer(this FileTransferSettings settings)
        {
            return new ServerApi.FileTransferSettings
            {
                ChunkSize = settings.ChunkSize,
                ChunkOffset = settings.ChunkOffset,
            };
        }

        public static ServerApi.UploadPackageRequest ToServer(this UploadPackageRequest request)
        {
            return new ServerApi.UploadPackageRequest
            {
                PackageId = request.PackageId,
                Filename = request.Filename,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static ServerApi.RemovePackageRequest ToServer(this RemovePackageRequest request)
        {
            return new ServerApi.RemovePackageRequest
            {
                PackageId = request.PackageId,
            };
        }

        public static ServerApi.DownloadPackageRequest ToServer(this DownloadPackageRequest request)
        {
            return new ServerApi.DownloadPackageRequest
            {
                PackageId = request.PackageId,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static Domain.PluginFolderInfo.Types.PluginFolderId ToServer(this PluginFolderInfo.Types.PluginFolderId pluginFolderId)
        {
            switch (pluginFolderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return Domain.PluginFolderInfo.Types.PluginFolderId.AlgoData;

                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return Domain.PluginFolderInfo.Types.PluginFolderId.BotLogs;

                default:
                    throw new ArgumentException();
            }
        }

        public static ServerApi.PluginFolderInfoRequest ToServer(this PluginFolderInfoRequest request)
        {
            return new ServerApi.PluginFolderInfoRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
            };
        }

        public static ServerApi.ClearPluginFolderRequest ToServer(this ClearPluginFolderRequest request)
        {
            return new ServerApi.ClearPluginFolderRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
            };
        }

        public static ServerApi.DeletePluginFileRequest ToServer(this DeletePluginFileRequest request)
        {
            return new ServerApi.DeletePluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
            };
        }

        public static ServerApi.DownloadPluginFileRequest ToServer(this DownloadPluginFileRequest request)
        {
            return new ServerApi.DownloadPluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static ServerApi.UploadPluginFileRequest ToServer(this UploadPluginFileRequest request)
        {
            return new ServerApi.UploadPluginFileRequest
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId.ToServer(),
                FileName = request.FileName,
                TransferSettings = request.TransferSettings.ToServer(),
            };
        }

        public static Domain.ConnectionErrorInfo.Types.ErrorCode ToServer(this ConnectionErrorInfo.Types.ErrorCode code)
        {
            switch (code)
            {
                case ConnectionErrorInfo.Types.ErrorCode.NoConnectionError:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;

                case ConnectionErrorInfo.Types.ErrorCode.NetworkError:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.NetworkError;

                case ConnectionErrorInfo.Types.ErrorCode.Timeout:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.Timeout;

                case ConnectionErrorInfo.Types.ErrorCode.BlockedAccount:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;

                case ConnectionErrorInfo.Types.ErrorCode.ClientInitiated:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.ClientInitiated;

                case ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;

                case ConnectionErrorInfo.Types.ErrorCode.SlowConnection:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.SlowConnection;

                case ConnectionErrorInfo.Types.ErrorCode.ServerError:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.ServerError;

                case ConnectionErrorInfo.Types.ErrorCode.LoginDeleted:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;

                case ConnectionErrorInfo.Types.ErrorCode.ServerLogout:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.ServerLogout;

                case ConnectionErrorInfo.Types.ErrorCode.Canceled:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.Canceled;

                case ConnectionErrorInfo.Types.ErrorCode.RejectedByServer:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.RejectedByServer;

                default:
                    return Domain.ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }

        public static Domain.ConnectionErrorInfo ToServer(this ConnectionErrorInfo info)
        {
            return new Domain.ConnectionErrorInfo
            {
                Code = info.Code.ToServer(),
                TextMessage = info.TextMessage,
            };
        }

        public static Domain.PluginFileInfo ToServer(this PluginFileInfo info)
        {
            return new Domain.PluginFileInfo
            {
                Name = info.Name,
                Size = info.Size,
            };
        }

        public static Domain.PluginFolderInfo ToServer(this PluginFolderInfo info)
        {
            var serverInfo = new Domain.PluginFolderInfo
            {
                PluginId = info.PluginId,
                FolderId = info.FolderId.ToServer(),
                Path = info.Path,
            };

            serverInfo.Files.AddRange(info.Files.Select(ToServer));

            return serverInfo;
        }

        public static Domain.AccountMetadataInfo ToServer(this AccountMetadataInfo info)
        {
            var serverInfo = new Domain.AccountMetadataInfo
            {
                AccountId = info.AccountId,
                DefaultSymbol = info.DefaultSymbol.ToServer(),
            };

            serverInfo.Symbols.AddRange(info.Symbols.Select(ToServer));

            return serverInfo;
        }

        public static Domain.PackageIdentity ToServer(this PackageIdentity identity)
        {
            return new Domain.PackageIdentity
            {
                FileName = identity.FileName,
                FilePath = identity.FilePath,
                CreatedUtc = identity.CreatedUtc,
                LastModifiedUtc = identity.LastModifiedUtc,
                Size = identity.Size,
                Hash = identity.Hash,
            };
        }

        public static Domain.Metadata.Types.PluginType ToServer(this PluginDescriptor.Types.PluginType type)
        {
            switch (type)
            {
                case PluginDescriptor.Types.PluginType.Indicator:
                    return Domain.Metadata.Types.PluginType.Indicator;

                case PluginDescriptor.Types.PluginType.TradeBot:
                    return Domain.Metadata.Types.PluginType.TradeBot;

                default:
                    return Domain.Metadata.Types.PluginType.UnknownPluginType;
            }
        }

        public static Domain.Metadata.Types.PropertyErrorCode ToServer(this ParameterDescriptor.Types.ParameterErrorCode type)
        {
            switch (type)
            {
                case ParameterDescriptor.Types.ParameterErrorCode.NoPropertyError:
                    return Domain.Metadata.Types.PropertyErrorCode.NoPropertyError;

                case ParameterDescriptor.Types.ParameterErrorCode.SetIsNotPublic:
                    return Domain.Metadata.Types.PropertyErrorCode.SetIsNotPublic;

                case ParameterDescriptor.Types.ParameterErrorCode.GetIsNotPublic:
                    return Domain.Metadata.Types.PropertyErrorCode.GetIsNotPublic;

                case ParameterDescriptor.Types.ParameterErrorCode.MultipleAttributes:
                    return Domain.Metadata.Types.PropertyErrorCode.MultipleAttributes;

                case ParameterDescriptor.Types.ParameterErrorCode.InputIsNotDataSeries:
                    return Domain.Metadata.Types.PropertyErrorCode.InputIsNotDataSeries;

                case ParameterDescriptor.Types.ParameterErrorCode.OutputIsNotDataSeries:
                    return Domain.Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries;

                case ParameterDescriptor.Types.ParameterErrorCode.EmptyEnum:
                    return Domain.Metadata.Types.PropertyErrorCode.EmptyEnum;

                default:
                    return Domain.Metadata.Types.PropertyErrorCode.UnknownPropertyError;
            }
        }

        public static Domain.Metadata.Types.MetadataErrorCode ToServer(this PluginDescriptor.Types.PluginErrorCode type)
        {
            switch (type)
            {
                case PluginDescriptor.Types.PluginErrorCode.NoMetadataError:
                    return Domain.Metadata.Types.MetadataErrorCode.NoMetadataError;

                case PluginDescriptor.Types.PluginErrorCode.HasInvalidProperties:
                    return Domain.Metadata.Types.MetadataErrorCode.HasInvalidProperties;

                case PluginDescriptor.Types.PluginErrorCode.UnknownBaseType:
                    return Domain.Metadata.Types.MetadataErrorCode.UnknownBaseType;

                case PluginDescriptor.Types.PluginErrorCode.IncompatibleApiNewerVersion:
                    return Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion;

                case PluginDescriptor.Types.PluginErrorCode.IncompatibleApiOlderVersion:
                    return Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion;

                default:
                    return Domain.Metadata.Types.MetadataErrorCode.UnknownMetadataError;
            }
        }

        public static Domain.FileFilterEntry ToServer(this FileFilterEntry entry)
        {
            return new Domain.FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static Domain.ParameterDescriptor ToServer(this ParameterDescriptor descriptor)
        {
            var serverDescriptor = new Domain.ParameterDescriptor
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

        public static Domain.InputDescriptor ToServer(this InputDescriptor descriptor)
        {
            return new Domain.InputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToServer(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
            };
        }

        public static Domain.Metadata.Types.LineStyle ToServer(this Metadata.Types.LineStyle type)
        {
            switch (type)
            {
                case Metadata.Types.LineStyle.Solid:
                    return Domain.Metadata.Types.LineStyle.Solid;

                case Metadata.Types.LineStyle.Dots:
                    return Domain.Metadata.Types.LineStyle.Dots;

                case Metadata.Types.LineStyle.DotsRare:
                    return Domain.Metadata.Types.LineStyle.DotsRare;

                case Metadata.Types.LineStyle.DotsVeryRare:
                    return Domain.Metadata.Types.LineStyle.DotsVeryRare;

                case Metadata.Types.LineStyle.Lines:
                    return Domain.Metadata.Types.LineStyle.Lines;

                case Metadata.Types.LineStyle.LinesDots:
                    return Domain.Metadata.Types.LineStyle.LinesDots;

                default:
                    return Domain.Metadata.Types.LineStyle.UnknownLineStyle;
            }
        }

        public static Domain.Metadata.Types.PlotType ToServer(this Metadata.Types.PlotType type)
        {
            switch (type)
            {
                case Metadata.Types.PlotType.Line:
                    return Domain.Metadata.Types.PlotType.Line;

                case Metadata.Types.PlotType.Histogram:
                    return Domain.Metadata.Types.PlotType.Histogram;

                case Metadata.Types.PlotType.Points:
                    return Domain.Metadata.Types.PlotType.Points;

                case Metadata.Types.PlotType.DiscontinuousLine:
                    return Domain.Metadata.Types.PlotType.DiscontinuousLine;

                default:
                    return Domain.Metadata.Types.PlotType.UnknownPlotType;
            }
        }

        public static Domain.Metadata.Types.OutputTarget ToServer(this Metadata.Types.OutputTarget type)
        {
            switch (type)
            {
                case Metadata.Types.OutputTarget.Overlay:
                    return Domain.Metadata.Types.OutputTarget.Overlay;

                case Metadata.Types.OutputTarget.Window1:
                    return Domain.Metadata.Types.OutputTarget.Window1;

                case Metadata.Types.OutputTarget.Window2:
                    return Domain.Metadata.Types.OutputTarget.Window2;

                case Metadata.Types.OutputTarget.Window3:
                    return Domain.Metadata.Types.OutputTarget.Window3;

                case Metadata.Types.OutputTarget.Window4:
                    return Domain.Metadata.Types.OutputTarget.Window4;

                default:
                    return Domain.Metadata.Types.OutputTarget.UnknownOutputTarget;
            }
        }

        public static Domain.OutputDescriptor ToServer(this OutputDescriptor descriptor)
        {
            return new Domain.OutputDescriptor
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

        public static Domain.PluginDescriptor ToServer(this PluginDescriptor descriptor)
        {
            var serverDescriptor = new Domain.PluginDescriptor
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

        public static Domain.PluginInfo ToServer(this PluginInfo info)
        {
            return new Domain.PluginInfo
            {
                Key = info.Key.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static Domain.Metadata.Types.ReductionType ToServer(this ReductionDescriptor.Types.ReductionType type)
        {
            switch (type)
            {
                case ReductionDescriptor.Types.ReductionType.BarToDouble:
                    return Domain.Metadata.Types.ReductionType.BarToDouble;

                case ReductionDescriptor.Types.ReductionType.FullBarToBar:
                    return Domain.Metadata.Types.ReductionType.FullBarToBar;

                case ReductionDescriptor.Types.ReductionType.FullBarToDouble:
                    return Domain.Metadata.Types.ReductionType.FullBarToDouble;

                default:
                    return Domain.Metadata.Types.ReductionType.UnknownReductionType;
            }
        }

        public static Domain.ReductionDescriptor ToServer(this ReductionDescriptor descriptor)
        {
            return new Domain.ReductionDescriptor
            {
                ApiVersionStr = descriptor.ApiVersionStr,
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToServer(),
            };
        }

        public static Domain.ReductionInfo ToServer(this ReductionInfo info)
        {
            return new Domain.ReductionInfo
            {
                Key = info.Key.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static Domain.PackageInfo ToServer(this PackageInfo info)
        {
            var serverInfo = new Domain.PackageInfo
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

        public static Domain.AccountModelInfo.Types.ConnectionState ToServer(this AccountModelInfo.Types.ConnectionState state)
        {
            switch (state)
            {
                case AccountModelInfo.Types.ConnectionState.Offline:
                    return Domain.AccountModelInfo.Types.ConnectionState.Offline;

                case AccountModelInfo.Types.ConnectionState.Connecting:
                    return Domain.AccountModelInfo.Types.ConnectionState.Connecting;

                case AccountModelInfo.Types.ConnectionState.Online:
                    return Domain.AccountModelInfo.Types.ConnectionState.Online;

                case AccountModelInfo.Types.ConnectionState.Disconnecting:
                    return Domain.AccountModelInfo.Types.ConnectionState.Disconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static Domain.AccountModelInfo ToServer(this AccountModelInfo info)
        {
            return new Domain.AccountModelInfo
            {
                AccountId = info.AccountId,
                ConnectionState = info.ConnectionState.ToServer(),
                LastError = info.LastError?.ToServer(),
                DisplayName = info.DisplayName,
            };
        }

        public static Domain.PluginModelInfo.Types.PluginState ToServer(this PluginModelInfo.Types.PluginState state)
        {
            switch (state)
            {
                case PluginModelInfo.Types.PluginState.Stopped:
                    return Domain.PluginModelInfo.Types.PluginState.Stopped;

                case PluginModelInfo.Types.PluginState.Starting:
                    return Domain.PluginModelInfo.Types.PluginState.Starting;

                case PluginModelInfo.Types.PluginState.Faulted:
                    return Domain.PluginModelInfo.Types.PluginState.Faulted;

                case PluginModelInfo.Types.PluginState.Running:
                    return Domain.PluginModelInfo.Types.PluginState.Running;

                case PluginModelInfo.Types.PluginState.Stopping:
                    return Domain.PluginModelInfo.Types.PluginState.Stopping;

                case PluginModelInfo.Types.PluginState.Broken:
                    return Domain.PluginModelInfo.Types.PluginState.Broken;

                case PluginModelInfo.Types.PluginState.Reconnecting:
                    return Domain.PluginModelInfo.Types.PluginState.Reconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static Domain.PluginModelInfo ToServer(this PluginModelInfo info)
        {
            return new Domain.PluginModelInfo
            {
                InstanceId = info.InstanceId,
                AccountId = info.AccountId,
                State = info.State.ToServer(),
                FaultMessage = info.FaultMessage,
                Config = info.Config.ToServer(),
                Descriptor_ = info.Descriptor_.ToServer(),
            };
        }

        public static Domain.Metadata.Types.MarkerSize ToServer(this Metadata.Types.MarkerSize state)
        {
            switch (state)
            {
                case Metadata.Types.MarkerSize.Large:
                    return Domain.Metadata.Types.MarkerSize.Large;

                case Metadata.Types.MarkerSize.Medium:
                    return Domain.Metadata.Types.MarkerSize.Medium;

                case Metadata.Types.MarkerSize.Small:
                    return Domain.Metadata.Types.MarkerSize.Small;

                default:
                    return Domain.Metadata.Types.MarkerSize.UnknownMarkerSize;
            };
        }

        public static Domain.ApiMetadataInfo ToServer(this ApiMetadataInfo info)
        {
            var serverMetadata = new Domain.ApiMetadataInfo();

            serverMetadata.TimeFrames.AddRange(info.TimeFrames.Select(ToServer));
            serverMetadata.LineStyles.AddRange(info.LineStyles.Select(ToServer));
            serverMetadata.Thicknesses.AddRange(info.Thicknesses);
            serverMetadata.MarkerSizes.AddRange(info.MarkerSizes.Select(ToServer));

            return serverMetadata;
        }

        public static Domain.MappingInfo ToServer(this MappingInfo info)
        {
            return new Domain.MappingInfo
            {
                Key = info.Key.ToServer(),
                DisplayName = info.DisplayName,
            };
        }

        public static Domain.MappingCollectionInfo ToServer(this MappingCollectionInfo info)
        {
            var collectionInfo = new Domain.MappingCollectionInfo
            {
                DefaultBarToBarMapping = info.DefaultBarToBarMapping.ToServer(),
                DefaultBarToDoubleMapping = info.DefaultBarToDoubleMapping.ToServer(),
            };

            collectionInfo.BarToBarMappings.AddRange(info.BarToBarMappings.Select(ToServer));
            collectionInfo.BarToDoubleMappings.AddRange(info.BarToDoubleMappings.Select(ToServer));

            return collectionInfo;
        }

        public static Domain.SetupContextInfo ToServer(this SetupContextInfo info)
        {
            return new Domain.SetupContextInfo
            {
                DefaultTimeFrame = info.DefaultTimeFrame.ToServer(),
                DefaultSymbol = info.DefaultSymbol.ToServer(),
                DefaultMapping = info.DefaultMapping.ToServer(),
            };
        }

        public static Domain.PluginStateUpdate ToServer(this PluginStateUpdate update)
        {
            return new Domain.PluginStateUpdate
            {
                Id = update.Id,
                State = update.State.ToServer(),
                FaultMessage = update.FaultMessage,
            };
        }

        public static ServerApi.AccountMetadataRequest ToServer(this AccountMetadataRequest request)
        {
            return new ServerApi.AccountMetadataRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static Domain.PluginLogRecord.Types.LogSeverity ToServer(this LogRecordInfo.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case LogRecordInfo.Types.LogSeverity.Info:
                    return Domain.PluginLogRecord.Types.LogSeverity.Info;

                case LogRecordInfo.Types.LogSeverity.Error:
                    return Domain.PluginLogRecord.Types.LogSeverity.Error;

                case LogRecordInfo.Types.LogSeverity.Trade:
                    return Domain.PluginLogRecord.Types.LogSeverity.Trade;

                case LogRecordInfo.Types.LogSeverity.TradeSuccess:
                    return Domain.PluginLogRecord.Types.LogSeverity.TradeSuccess;

                case LogRecordInfo.Types.LogSeverity.TradeFail:
                    return Domain.PluginLogRecord.Types.LogSeverity.TradeFail;

                case LogRecordInfo.Types.LogSeverity.Custom:
                    return Domain.PluginLogRecord.Types.LogSeverity.Custom;

                case LogRecordInfo.Types.LogSeverity.Alert:
                    return Domain.PluginLogRecord.Types.LogSeverity.Alert;

                default:
                    throw new ArgumentException();
            }
        }

        public static Domain.LogRecordInfo ToServer(this LogRecordInfo info)
        {
            return new Domain.LogRecordInfo
            {
                TimeUtc = info.TimeUtc,
                Severity = info.Severity.ToServer(),
                Message = info.Message,
            };
        }

        public static Domain.AlertRecordInfo ToServer(this AlertRecordInfo info)
        {
            return new Domain.AlertRecordInfo
            {
                Message = info.Message,
                PluginId = info.PluginId,
                TimeUtc = info.TimeUtc,
                Type = info.Type.ToServer(),
            };
        }

        public static Domain.AlertRecordInfo.Types.AlertType ToServer(this AlertRecordInfo.Types.AlertType type)
        {
            switch (type)
            {
                case AlertRecordInfo.Types.AlertType.Plugin:
                    return Domain.AlertRecordInfo.Types.AlertType.Plugin;
                case AlertRecordInfo.Types.AlertType.Server:
                    return Domain.AlertRecordInfo.Types.AlertType.Server;
                case AlertRecordInfo.Types.AlertType.Monitoring:
                    return Domain.AlertRecordInfo.Types.AlertType.Monitoring;
                default:
                    throw new ArgumentException($"Unsupported alert type {type}");
            }
        }

        public static ServerApi.ServerUpdateInfo ToServer(this ServerUpdateInfo info)
        {
            return new ServerApi.ServerUpdateInfo
            {
                ReleaseId = info.ReleaseId,
                Version = info.Version,
                ReleaseDate = info.ReleaseDate,
                MinVersion = info.MinVersion,
                Changelog = info.Changelog,
                IsStable = info.IsStable,
            };
        }

        public static ServerApi.ServerUpdateListRequest ToServer(this ServerUpdateListRequest request)
        {
            return ServerApi.ServerUpdateListRequest.Get(request.Forced);
        }

        public static ServerApi.StartServerUpdateRequest ToServer(this StartServerUpdateRequest request)
        {
            return new ServerApi.StartServerUpdateRequest
            {
                ReleaseId = request.ReleaseId,
            };
        }

        public static ServerApi.ServerVersionInfo ToServer(this ServerVersionInfo info)
        {
            return new ServerApi.ServerVersionInfo
            {
                Version = info.Version,
                ReleaseDate = info.ReleaseDate,
            };
        }

        public static ServerApi.ServerUpdateList ToServer(this ServerUpdateList list)
        {
            var res = new ServerApi.ServerUpdateList();
            res.Updates.AddRange(list.Updates.Select(u => u.ToServer()));
            res.Errors.Add(list.Errors);
            return res;
        }

        public static ServerApi.UpdateServiceInfo ToServer(this UpdateServiceInfo info)
        {
            return new ServerApi.UpdateServiceInfo
            {
                Status = info.Status.ToServer(),
                StatusDetails = info.StatusDetails,
                UpdateLog = info.UpdateLog,
                HasNewVersion = info.HasNewVersion,
                NewVersion = info.NewVersion,
            };
        }

        public static ServerApi.AutoUpdateEnums.Types.ServiceStatus ToServer(this AutoUpdateEnums.Types.ServiceStatus status)
        {
            switch (status)
            {
                case AutoUpdateEnums.Types.ServiceStatus.Idle:
                    return ServerApi.AutoUpdateEnums.Types.ServiceStatus.Idle;
                case AutoUpdateEnums.Types.ServiceStatus.Updating:
                    return ServerApi.AutoUpdateEnums.Types.ServiceStatus.Updating;
                case AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess:
                    return ServerApi.AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess;
                case AutoUpdateEnums.Types.ServiceStatus.UpdateFailed:
                    return ServerApi.AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;
                default:
                    throw new ArgumentException($"Unsupported update service status {status}");
            }
        }

        public static ServerApi.StartServerUpdateResponse ToServer(this StartUpdateResult result)
        {
            return new ServerApi.StartServerUpdateResponse
            {
                Started = result.Started,
                ErrorMsg = result.ErrorMsg,
            };
        }
    }
}
