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
                PrimaryReduction = key.PrimaryReduction.ToApi(),
                SecondaryReduction = key.SecondaryReduction.ToApi(),
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

            return apiConfig.PackProperties(config.Properties);
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
    }
}
