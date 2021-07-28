using System;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using Api = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public static class PublicApiConverters
    {
        public static Api.ClientClaims.Types.AccessLevel ToApi(this ClientClaims.Types.AccessLevel claims)
        {
            switch (claims)
            {
                case ClientClaims.Types.AccessLevel.Admin:
                    return Api.ClientClaims.Types.AccessLevel.Admin;

                case ClientClaims.Types.AccessLevel.Viewer:
                    return Api.ClientClaims.Types.AccessLevel.Viewer;

                case ClientClaims.Types.AccessLevel.Dealer:
                    return Api.ClientClaims.Types.AccessLevel.Dealer;

                default:
                    return Api.ClientClaims.Types.AccessLevel.Anonymous;
            }
        }

        public static Api.ConnectionErrorInfo.Types.ErrorCode ToApi(this ConnectionErrorInfo.Types.ErrorCode code)
        {
            switch (code)
            {
                case ConnectionErrorInfo.Types.ErrorCode.NoConnectionError:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;

                case ConnectionErrorInfo.Types.ErrorCode.NetworkError:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.NetworkError;

                case ConnectionErrorInfo.Types.ErrorCode.Timeout:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.Timeout;

                case ConnectionErrorInfo.Types.ErrorCode.BlockedAccount:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;

                case ConnectionErrorInfo.Types.ErrorCode.ClientInitiated:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.ClientInitiated;

                case ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;

                case ConnectionErrorInfo.Types.ErrorCode.SlowConnection:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.SlowConnection;

                case ConnectionErrorInfo.Types.ErrorCode.ServerError:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.ServerError;

                case ConnectionErrorInfo.Types.ErrorCode.LoginDeleted:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;

                case ConnectionErrorInfo.Types.ErrorCode.ServerLogout:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.ServerLogout;

                case ConnectionErrorInfo.Types.ErrorCode.Canceled:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.Canceled;

                case ConnectionErrorInfo.Types.ErrorCode.RejectedByServer:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.RejectedByServer;

                default:
                    return Api.ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }

        public static Api.ConnectionErrorInfo ToApi(this ConnectionErrorInfo info)
        {
            return new Api.ConnectionErrorInfo
            {
                Code = info.Code.ToApi(),
                TextMessage = info.TextMessage,
            };
        }

        public static Api.PluginFolderInfo.Types.PluginFolderId ToApi(this PluginFolderInfo.Types.PluginFolderId pluginFolderId)
        {
            switch (pluginFolderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return Api.PluginFolderInfo.Types.PluginFolderId.AlgoData;

                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return Api.PluginFolderInfo.Types.PluginFolderId.BotLogs;

                default:
                    throw new ArgumentException();
            }
        }

        public static Api.PluginFileInfo ToApi(this PluginFileInfo info)
        {
            return new Api.PluginFileInfo
            {
                Name = info.Name,
                Size = info.Size,
            };
        }

        public static Api.PluginFolderInfo ToApi(this PluginFolderInfo info)
        {
            var apiInfo = new Api.PluginFolderInfo
            {
                PluginId = info.PluginId,
                FolderId = info.FolderId.ToApi(),
                Path = info.Path
            };

            apiInfo.Files.AddRange(info.Files.Select(u => u.ToApi()));

            return apiInfo;
        }

        public static Api.PluginKey ToApi(this PluginKey key)
        {
            return new Api.PluginKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Api.Feed.Types.Timeframe ToApi(this Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Feed.Types.Timeframe.S1:
                    return Api.Feed.Types.Timeframe.S1;

                case Feed.Types.Timeframe.S10:
                    return Api.Feed.Types.Timeframe.S10;

                case Feed.Types.Timeframe.M5:
                    return Api.Feed.Types.Timeframe.M5;

                case Feed.Types.Timeframe.M15:
                    return Api.Feed.Types.Timeframe.M15;

                case Feed.Types.Timeframe.M30:
                    return Api.Feed.Types.Timeframe.M30;

                case Feed.Types.Timeframe.H1:
                    return Api.Feed.Types.Timeframe.H1;

                case Feed.Types.Timeframe.H4:
                    return Api.Feed.Types.Timeframe.H4;

                case Feed.Types.Timeframe.D:
                    return Api.Feed.Types.Timeframe.D;

                case Feed.Types.Timeframe.W:
                    return Api.Feed.Types.Timeframe.W;

                case Feed.Types.Timeframe.MN:
                    return Api.Feed.Types.Timeframe.MN;

                case Feed.Types.Timeframe.Ticks:
                    return Api.Feed.Types.Timeframe.Ticks;

                case Feed.Types.Timeframe.TicksLevel2:
                    return Api.Feed.Types.Timeframe.TicksLevel2;

                case Feed.Types.Timeframe.TicksVwap:
                    return Api.Feed.Types.Timeframe.TicksVwap;

                default:
                    return Api.Feed.Types.Timeframe.M1;
            }
        }

        public static Api.SymbolConfig.Types.SymbolOrigin ToApi(this SymbolConfig.Types.SymbolOrigin origin)
        {
            switch (origin)
            {
                case SymbolConfig.Types.SymbolOrigin.Online:
                    return Api.SymbolConfig.Types.SymbolOrigin.Online;

                case SymbolConfig.Types.SymbolOrigin.Token:
                    return Api.SymbolConfig.Types.SymbolOrigin.Token;

                case SymbolConfig.Types.SymbolOrigin.Custom:
                    return Api.SymbolConfig.Types.SymbolOrigin.Custom;

                default:
                    throw new ArgumentException();
            }
        }

        public static Api.SymbolConfig ToApi(this SymbolConfig config)
        {
            return new Api.SymbolConfig
            {
                Name = config.Name,
                Origin = config.Origin.ToApi(),
            };
        }

        public static Api.ReductionKey ToApi(this ReductionKey key)
        {
            return new Api.ReductionKey
            {
                PackageId = key.PackageId,
                DescriptorId = key.DescriptorId,
            };
        }

        public static Api.MappingKey ToApi(this MappingKey key)
        {
            return new Api.MappingKey
            {
                PrimaryReduction = key.PrimaryReduction?.ToApi(),
                SecondaryReduction = key.SecondaryReduction?.ToApi(),
            };
        }

        public static Api.PluginPermissions ToApi(this PluginPermissions permissions)
        {
            return new Api.PluginPermissions
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
            };
        }

        public static Api.PluginConfig ToApi(this PluginConfig config)
        {
            if (config == null)
                return new Api.PluginConfig();

            var apiConfig = new Api.PluginConfig
            {
                Key = config.Key.ToApi(),
                Timeframe = config.Timeframe.ToApi(),
                ModelTimeframe = config.ModelTimeframe.ToApi(),
                MainSymbol = config.MainSymbol.ToApi(),
                SelectedMapping = config.SelectedMapping.ToApi(),
                InstanceId = config.InstanceId,
                Permissions = config.Permissions.ToApi()
            };

            apiConfig.Properties.AddRange(config.Properties);

            return apiConfig;
        }

        public static Api.AccountCreds ToApi(this AccountCreds creds)
        {
            return new Api.AccountCreds(creds.AuthScheme, creds.Secret[AccountCreds.PasswordKey]);
        }

        public static Api.AddAccountRequest ToApi(this AddAccountRequest request)
        {
            return new Api.AddAccountRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToApi(),
                DisplayName = request.DisplayName,
            };
        }

        public static Api.RemoveAccountRequest ToApi(this RemoveAccountRequest request)
        {
            return new Api.RemoveAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static Api.ChangeAccountRequest ToApi(this ChangeAccountRequest request)
        {
            return new Api.ChangeAccountRequest
            {
                AccountId = request.AccountId,
                Creds = request.Creds.ToApi(),
                DisplayName = request.DisplayName,
            };
        }

        public static Api.TestAccountRequest ToApi(this TestAccountRequest request)
        {
            return new Api.TestAccountRequest
            {
                AccountId = request.AccountId,
            };
        }

        public static Api.TestAccountCredsRequest ToApi(this TestAccountCredsRequest request)
        {
            return new Api.TestAccountCredsRequest
            {
                Server = request.Server,
                UserId = request.UserId,
                Creds = request.Creds.ToApi(),
            };
        }

        public static Api.UpdateInfo ToApi(this UpdateInfo info)
        {
            return new Api.UpdateInfo
            {
                Payload = info.Payload,
            };
        }

        public static Api.Metadata.Types.LineStyle ToApi(this Metadata.Types.LineStyle type)
        {
            switch (type)
            {
                case Metadata.Types.LineStyle.Solid:
                    return Api.Metadata.Types.LineStyle.Solid;

                case Metadata.Types.LineStyle.Dots:
                    return Api.Metadata.Types.LineStyle.Dots;

                case Metadata.Types.LineStyle.DotsRare:
                    return Api.Metadata.Types.LineStyle.DotsRare;

                case Metadata.Types.LineStyle.DotsVeryRare:
                    return Api.Metadata.Types.LineStyle.DotsVeryRare;

                case Metadata.Types.LineStyle.Lines:
                    return Api.Metadata.Types.LineStyle.Lines;

                case Metadata.Types.LineStyle.LinesDots:
                    return Api.Metadata.Types.LineStyle.LinesDots;

                default:
                    return Api.Metadata.Types.LineStyle.UnknownLineStyle;
            }
        }

        public static Api.Metadata.Types.MarkerSize ToApi(this Metadata.Types.MarkerSize state)
        {
            switch (state)
            {
                case Metadata.Types.MarkerSize.Large:
                    return Api.Metadata.Types.MarkerSize.Large;

                case Metadata.Types.MarkerSize.Medium:
                    return Api.Metadata.Types.MarkerSize.Medium;

                case Metadata.Types.MarkerSize.Small:
                    return Api.Metadata.Types.MarkerSize.Small;

                default:
                    return Api.Metadata.Types.MarkerSize.UnknownMarkerSize;
            };
        }

        public static Api.ApiMetadataInfo ToApi(this ApiMetadataInfo info)
        {
            var apiMetadata = new Api.ApiMetadataInfo();

            apiMetadata.TimeFrames.AddRange(info.TimeFrames.Select(ToApi));
            apiMetadata.LineStyles.AddRange(info.LineStyles.Select(ToApi));
            apiMetadata.Thicknesses.AddRange(info.Thicknesses);
            apiMetadata.MarkerSizes.AddRange(info.MarkerSizes.Select(ToApi));

            return apiMetadata;
        }

        public static Api.MappingInfo ToApi(this MappingInfo info)
        {
            return new Api.MappingInfo
            {
                Key = info.Key.ToApi(),
                DisplayName = info.DisplayName,
            };
        }

        public static Api.MappingCollectionInfo ToApi(this MappingCollectionInfo info)
        {
            var collectionInfo = new Api.MappingCollectionInfo
            {
                DefaultBarToBarMapping = info.DefaultBarToBarMapping.ToApi(),
                DefaultBarToDoubleMapping = info.DefaultBarToDoubleMapping.ToApi(),
            };

            collectionInfo.BarToBarMappings.AddRange(info.BarToBarMappings.Select(ToApi));
            collectionInfo.BarToDoubleMappings.AddRange(info.BarToDoubleMappings.Select(ToApi));

            return collectionInfo;
        }

        public static Api.SetupContextInfo ToApi(this SetupContextInfo info)
        {
            return new Api.SetupContextInfo
            {
                DefaultTimeFrame = info.DefaultTimeFrame.ToApi(),
                DefaultSymbol = info.DefaultSymbol.ToApi(),
                DefaultMapping = info.DefaultMapping.ToApi(),
            };
        }

        public static Api.PackageIdentity ToApi(this PackageIdentity identity)
        {
            return new Api.PackageIdentity
            {
                FileName = identity.FileName,
                FilePath = identity.FilePath,
                CreatedUtc = identity.CreatedUtc,
                LastModifiedUtc = identity.LastModifiedUtc,
                Size = identity.Size,
                Hash = identity.Hash,
            };
        }

        public static Api.Metadata.Types.PluginType ToApi(this Metadata.Types.PluginType type)
        {
            switch (type)
            {
                case Metadata.Types.PluginType.Indicator:
                    return Api.Metadata.Types.PluginType.Indicator;

                case Metadata.Types.PluginType.TradeBot:
                    return Api.Metadata.Types.PluginType.TradeBot;

                default:
                    return Api.Metadata.Types.PluginType.UnknownPluginType;
            }
        }

        public static Api.Metadata.Types.MetadataErrorCode ToApi(this Metadata.Types.MetadataErrorCode type)
        {
            switch (type)
            {
                case Metadata.Types.MetadataErrorCode.NoMetadataError:
                    return Api.Metadata.Types.MetadataErrorCode.NoMetadataError;

                case Metadata.Types.MetadataErrorCode.HasInvalidProperties:
                    return Api.Metadata.Types.MetadataErrorCode.HasInvalidProperties;

                case Metadata.Types.MetadataErrorCode.UnknownBaseType:
                    return Api.Metadata.Types.MetadataErrorCode.UnknownBaseType;

                case Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion:
                    return Api.Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion;

                case Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion:
                    return Api.Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion;

                default:
                    return Api.Metadata.Types.MetadataErrorCode.UnknownMetadataError;
            }
        }

        public static Api.Metadata.Types.PropertyErrorCode ToApi(this Metadata.Types.PropertyErrorCode type)
        {
            switch (type)
            {
                case Metadata.Types.PropertyErrorCode.NoPropertyError:
                    return Api.Metadata.Types.PropertyErrorCode.NoPropertyError;

                case Metadata.Types.PropertyErrorCode.SetIsNotPublic:
                    return Api.Metadata.Types.PropertyErrorCode.SetIsNotPublic;

                case Metadata.Types.PropertyErrorCode.GetIsNotPublic:
                    return Api.Metadata.Types.PropertyErrorCode.GetIsNotPublic;

                case Metadata.Types.PropertyErrorCode.MultipleAttributes:
                    return Api.Metadata.Types.PropertyErrorCode.MultipleAttributes;

                case Metadata.Types.PropertyErrorCode.InputIsNotDataSeries:
                    return Api.Metadata.Types.PropertyErrorCode.InputIsNotDataSeries;

                case Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries:
                    return Api.Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries;

                case Metadata.Types.PropertyErrorCode.EmptyEnum:
                    return Api.Metadata.Types.PropertyErrorCode.EmptyEnum;

                default:
                    return Api.Metadata.Types.PropertyErrorCode.UnknownPropertyError;
            }
        }

        public static Api.FileFilterEntry ToApi(this FileFilterEntry entry)
        {
            return new Api.FileFilterEntry
            {
                FileTypeName = entry.FileTypeName,
                FileMask = entry.FileMask,
            };
        }

        public static Api.ParameterDescriptor ToApi(this ParameterDescriptor descriptor)
        {
            var apiDescriptor = new Api.ParameterDescriptor
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

        public static Api.InputDescriptor ToApi(this InputDescriptor descriptor)
        {
            return new Api.InputDescriptor
            {
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                ErrorCode = descriptor.ErrorCode.ToApi(),
                DataSeriesBaseTypeFullName = descriptor.DataSeriesBaseTypeFullName,
            };
        }

        public static Api.Metadata.Types.PlotType ToApi(this Metadata.Types.PlotType type)
        {
            switch (type)
            {
                case Metadata.Types.PlotType.Line:
                    return Api.Metadata.Types.PlotType.Line;

                case Metadata.Types.PlotType.Histogram:
                    return Api.Metadata.Types.PlotType.Histogram;

                case Metadata.Types.PlotType.Points:
                    return Api.Metadata.Types.PlotType.Points;

                case Metadata.Types.PlotType.DiscontinuousLine:
                    return Api.Metadata.Types.PlotType.DiscontinuousLine;

                default:
                    return Api.Metadata.Types.PlotType.UnknownPlotType;
            }
        }

        public static Api.Metadata.Types.OutputTarget ToApi(this Metadata.Types.OutputTarget type)
        {
            switch (type)
            {
                case Metadata.Types.OutputTarget.Overlay:
                    return Api.Metadata.Types.OutputTarget.Overlay;

                case Metadata.Types.OutputTarget.Window1:
                    return Api.Metadata.Types.OutputTarget.Window1;

                case Metadata.Types.OutputTarget.Window2:
                    return Api.Metadata.Types.OutputTarget.Window2;

                case Metadata.Types.OutputTarget.Window3:
                    return Api.Metadata.Types.OutputTarget.Window3;

                case Metadata.Types.OutputTarget.Window4:
                    return Api.Metadata.Types.OutputTarget.Window4;

                default:
                    return Api.Metadata.Types.OutputTarget.UnknownOutputTarget;
            }
        }

        public static Api.OutputDescriptor ToApi(this OutputDescriptor descriptor)
        {
            return new Api.OutputDescriptor
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

        public static Api.PluginDescriptor ToApi(this PluginDescriptor descriptor)
        {
            if (descriptor != null)
            {
                var apiDescriptor = new Api.PluginDescriptor
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

            return new Api.PluginDescriptor();
        }

        public static Api.PluginInfo ToApi(this PluginInfo info)
        {
            return new Api.PluginInfo
            {
                Key = info.Key.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static Api.Metadata.Types.ReductionType ToApi(this Metadata.Types.ReductionType type)
        {
            switch (type)
            {
                case Metadata.Types.ReductionType.BarToDouble:
                    return Api.Metadata.Types.ReductionType.BarToDouble;

                case Metadata.Types.ReductionType.FullBarToBar:
                    return Api.Metadata.Types.ReductionType.FullBarToBar;

                case Metadata.Types.ReductionType.FullBarToDouble:
                    return Api.Metadata.Types.ReductionType.FullBarToDouble;

                default:
                    return Api.Metadata.Types.ReductionType.UnknownReductionType;
            }
        }

        public static Api.ReductionDescriptor ToApi(this ReductionDescriptor descriptor)
        {
            return new Api.ReductionDescriptor
            {
                ApiVersionStr = descriptor.ApiVersionStr,
                Id = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToApi(),
            };
        }

        public static Api.ReductionInfo ToApi(this ReductionInfo info)
        {
            return new Api.ReductionInfo
            {
                Key = info.Key.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static Api.PackageInfo ToApi(this PackageInfo info)
        {
            var apiInfo = new Api.PackageInfo
            {
                PackageId = info.PackageId,
                Identity = info.Identity.ToApi(),
                IsValid = info.IsValid,
                IsLocked = info.IsLocked,
            };

            apiInfo.Plugins.AddRange(info.Plugins.Select(ToApi));
            apiInfo.Reductions.AddRange(info.Reductions.Select(ToApi));

            return apiInfo;
        }

        public static Api.AccountModelInfo.Types.ConnectionState ToApi(this AccountModelInfo.Types.ConnectionState state)
        {
            switch (state)
            {
                case AccountModelInfo.Types.ConnectionState.Offline:
                    return Api.AccountModelInfo.Types.ConnectionState.Offline;

                case AccountModelInfo.Types.ConnectionState.Connecting:
                    return Api.AccountModelInfo.Types.ConnectionState.Connecting;

                case AccountModelInfo.Types.ConnectionState.Online:
                    return Api.AccountModelInfo.Types.ConnectionState.Online;

                case AccountModelInfo.Types.ConnectionState.Disconnecting:
                    return Api.AccountModelInfo.Types.ConnectionState.Disconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static Api.AccountModelInfo ToApi(this AccountModelInfo info)
        {
            return new Api.AccountModelInfo
            {
                AccountId = info.AccountId,
                ConnectionState = info.ConnectionState.ToApi(),
                LastError = info.LastError?.ToApi(),
                DisplayName = info.DisplayName,
            };
        }

        public static Api.PluginModelInfo.Types.PluginState ToApi(this PluginModelInfo.Types.PluginState state)
        {
            switch (state)
            {
                case PluginModelInfo.Types.PluginState.Stopped:
                    return Api.PluginModelInfo.Types.PluginState.Stopped;

                case PluginModelInfo.Types.PluginState.Starting:
                    return Api.PluginModelInfo.Types.PluginState.Starting;

                case PluginModelInfo.Types.PluginState.Faulted:
                    return Api.PluginModelInfo.Types.PluginState.Faulted;

                case PluginModelInfo.Types.PluginState.Running:
                    return Api.PluginModelInfo.Types.PluginState.Running;

                case PluginModelInfo.Types.PluginState.Stopping:
                    return Api.PluginModelInfo.Types.PluginState.Stopping;

                case PluginModelInfo.Types.PluginState.Broken:
                    return Api.PluginModelInfo.Types.PluginState.Broken;

                case PluginModelInfo.Types.PluginState.Reconnecting:
                    return Api.PluginModelInfo.Types.PluginState.Reconnecting;

                default:
                    throw new ArgumentException();
            };
        }

        public static Api.PluginModelInfo ToApi(this PluginModelInfo info)
        {
            return new Api.PluginModelInfo
            {
                InstanceId = info.InstanceId,
                AccountId = info.AccountId,
                State = info.State.ToApi(),
                FaultMessage = info.FaultMessage,
                Config = info.Config.ToApi(),
                Descriptor_ = info.Descriptor_.ToApi(),
            };
        }

        public static Api.AccountMetadataInfo ToApi(this AccountMetadataInfo info)
        {
            var response = new Api.AccountMetadataInfo
            {
                AccountId = info.AccountId,
                DefaultSymbol = info.DefaultSymbol.ToApi(),
            };

            response.Symbols.AddRange(info.Symbols.Select(ToApi));

            return response;
        }

        public static Api.PackageStateUpdate ToApi(this PackageStateUpdate update)
        {
            return new Api.PackageStateUpdate
            {
                Id = update.Id,
                IsLocked = update.IsLocked,
            };
        }

        public static Api.AccountStateUpdate ToApi(this AccountStateUpdate update)
        {
            return new Api.AccountStateUpdate
            {
                Id = update.Id,
                ConnectionState = update.ConnectionState.ToApi(),
                LastError = update.LastError.ToApi(),
            };
        }

        public static Api.PluginStateUpdate ToApi(this PluginStateUpdate update)
        {
            return new Api.PluginStateUpdate
            {
                Id = update.Id,
                State = update.State.ToApi(),
                FaultMessage = update.FaultMessage,
            };
        }

        public static Api.Update.Types.Action ToApi(this UpdateInfo.Types.UpdateType type)
        {
            switch (type)
            {
                case UpdateInfo.Types.UpdateType.Added:
                    return Api.Update.Types.Action.Added;

                case UpdateInfo.Types.UpdateType.Replaced:
                    return Api.Update.Types.Action.Updated;

                case UpdateInfo.Types.UpdateType.Removed:
                    return Api.Update.Types.Action.Removed;

                default:
                    throw new ArgumentException();
            }
        }

        public static Api.PluginLogRecord.Types.LogSeverity ToApi(this PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case PluginLogRecord.Types.LogSeverity.Info:
                    return Api.PluginLogRecord.Types.LogSeverity.Info;

                case PluginLogRecord.Types.LogSeverity.Error:
                    return Api.PluginLogRecord.Types.LogSeverity.Error;

                case PluginLogRecord.Types.LogSeverity.Trade:
                    return Api.PluginLogRecord.Types.LogSeverity.Trade;

                case PluginLogRecord.Types.LogSeverity.TradeSuccess:
                    return Api.PluginLogRecord.Types.LogSeverity.TradeSuccess;

                case PluginLogRecord.Types.LogSeverity.TradeFail:
                    return Api.PluginLogRecord.Types.LogSeverity.TradeFail;

                case PluginLogRecord.Types.LogSeverity.Custom:
                    return Api.PluginLogRecord.Types.LogSeverity.Custom;

                case PluginLogRecord.Types.LogSeverity.Alert:
                    return Api.PluginLogRecord.Types.LogSeverity.Alert;

                default:
                    throw new ArgumentException();
            }
        }

        public static Api.LogRecordInfo ToApi(this LogRecordInfo info)
        {
            return new Api.LogRecordInfo
            {
                TimeUtc = info.TimeUtc,
                Severity = info.Severity.ToApi(),
                Message = info.Message,
            };
        }

        public static Api.AlertRecordInfo ToApi(this AlertRecordInfo info)
        {
            return new Api.AlertRecordInfo
            {
                Message = info.Message,
                PluginId = info.PluginId,
                TimeUtc = info.TimeUtc,
            };
        }
    }
}
