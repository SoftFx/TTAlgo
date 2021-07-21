using System;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using Api = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl
{
    public static class Converters
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
                PrimaryReduction = key.PrimaryReduction.ToServer(),
                SecondaryReduction = key.SecondaryReduction.ToServer(),
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

            return serverConfig.PackProperties(config.Properties);
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
    }
}
