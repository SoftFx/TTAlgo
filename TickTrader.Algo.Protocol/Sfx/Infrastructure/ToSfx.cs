using System;
using SfxProtocol = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    internal static class ToSfx
    {
        internal static SfxProtocol.BotState Convert(BotState state)
        {
            switch (state)
            {
                case BotState.Offline:
                    return SfxProtocol.BotState.Offline;
                case BotState.Starting:
                    return SfxProtocol.BotState.Starting;
                case BotState.Faulted:
                    return SfxProtocol.BotState.Faulted;
                case BotState.Online:
                    return SfxProtocol.BotState.Online;
                case BotState.Stopping:
                    return SfxProtocol.BotState.Stopping;
                case BotState.Broken:
                    return SfxProtocol.BotState.Broken;
                case BotState.Reconnecting:
                    return SfxProtocol.BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.LoginRejectReason Convert(LoginRejectReason reason)
        {
            switch (reason)
            {
                case LoginRejectReason.InvalidCredentials:
                    return SfxProtocol.LoginRejectReason.InvalidCredentials;
                case LoginRejectReason.VersionMismatch:
                    return SfxProtocol.LoginRejectReason.VersionMismatch;
                case LoginRejectReason.InternalServerError:
                    return SfxProtocol.LoginRejectReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.LogoutReason Convert(LogoutReason reason)
        {
            switch (reason)
            {
                case LogoutReason.ClientRequest:
                    return SfxProtocol.LogoutReason.ClientRequest;
                case LogoutReason.ServerLogout:
                    return SfxProtocol.LogoutReason.ServerLogout;
                case LogoutReason.InternalServerError:
                    return SfxProtocol.LogoutReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.PluginType Convert(PluginType type)
        {
            switch (type)
            {
                case PluginType.Indicator:
                    return SfxProtocol.PluginType.Indicator;
                case PluginType.Robot:
                    return SfxProtocol.PluginType.Robot;
                default:
                    return SfxProtocol.PluginType.Unknown;
            }
        }

        internal static SfxProtocol.UpdateType Convert(UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Added:
                    return SfxProtocol.UpdateType.Added;
                case UpdateType.Updated:
                    return SfxProtocol.UpdateType.Updated;
                case UpdateType.Removed:
                    return SfxProtocol.UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.RequestExecState Convert(RequestExecState type)
        {
            switch (type)
            {
                case RequestExecState.Completed:
                    return SfxProtocol.RequestExecState.Completed;
                case RequestExecState.InternalServerError:
                    return SfxProtocol.RequestExecState.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.ConnectionState Convert(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Offline:
                    return SfxProtocol.ConnectionState.Offline;
                case ConnectionState.Connecting:
                    return SfxProtocol.ConnectionState.Connecting;
                case ConnectionState.Online:
                    return SfxProtocol.ConnectionState.Online;
                case ConnectionState.Disconnecting:
                    return SfxProtocol.ConnectionState.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static SfxProtocol.ConnectionErrorCode Convert(ConnectionErrorCode code)
        {
            switch (code)
            {
                case ConnectionErrorCode.None:
                    return SfxProtocol.ConnectionErrorCode.None;
                case ConnectionErrorCode.Unknown:
                    return SfxProtocol.ConnectionErrorCode.Unknown;
                case ConnectionErrorCode.NetworkError:
                    return SfxProtocol.ConnectionErrorCode.NetworkError;
                case ConnectionErrorCode.Timeout:
                    return SfxProtocol.ConnectionErrorCode.Timeout;
                case ConnectionErrorCode.BlockedAccount:
                    return SfxProtocol.ConnectionErrorCode.BlockedAccount;
                case ConnectionErrorCode.ClientInitiated:
                    return SfxProtocol.ConnectionErrorCode.ClientInitiated;
                case ConnectionErrorCode.InvalidCredentials:
                    return SfxProtocol.ConnectionErrorCode.InvalidCredentials;
                case ConnectionErrorCode.SlowConnection:
                    return SfxProtocol.ConnectionErrorCode.SlowConnection;
                case ConnectionErrorCode.ServerError:
                    return SfxProtocol.ConnectionErrorCode.ServerError;
                case ConnectionErrorCode.LoginDeleted:
                    return SfxProtocol.ConnectionErrorCode.LoginDeleted;
                case ConnectionErrorCode.ServerLogout:
                    return SfxProtocol.ConnectionErrorCode.ServerLogout;
                case ConnectionErrorCode.Canceled:
                    return SfxProtocol.ConnectionErrorCode.Canceled;
                default:
                    throw new ArgumentException();
            }
        }
    }
}