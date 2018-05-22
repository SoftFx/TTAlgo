using System;
using SfxProtocol = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    internal static class ToAlgo
    {
        internal static BotState Convert(SfxProtocol.BotState state)
        {
            switch (state)
            {
                case SfxProtocol.BotState.Offline:
                    return BotState.Offline;
                case SfxProtocol.BotState.Starting:
                    return BotState.Starting;
                case SfxProtocol.BotState.Faulted:
                    return BotState.Faulted;
                case SfxProtocol.BotState.Online:
                    return BotState.Online;
                case SfxProtocol.BotState.Stopping:
                    return BotState.Stopping;
                case SfxProtocol.BotState.Broken:
                    return BotState.Broken;
                case SfxProtocol.BotState.Reconnecting:
                    return BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static LoginRejectReason Convert(SfxProtocol.LoginRejectReason reason)
        {
            switch (reason)
            {
                case SfxProtocol.LoginRejectReason.InvalidCredentials:
                    return LoginRejectReason.InvalidCredentials;
                case SfxProtocol.LoginRejectReason.VersionMismatch:
                    return LoginRejectReason.VersionMismatch;
                case SfxProtocol.LoginRejectReason.InternalServerError:
                    return LoginRejectReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static LogoutReason Convert(SfxProtocol.LogoutReason reason)
        {
            switch (reason)
            {
                case SfxProtocol.LogoutReason.ClientRequest:
                    return LogoutReason.ClientRequest;
                case SfxProtocol.LogoutReason.ServerLogout:
                    return LogoutReason.ServerLogout;
                case SfxProtocol.LogoutReason.InternalServerError:
                    return LogoutReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static PluginType Convert(SfxProtocol.PluginType type)
        {
            switch (type)
            {
                case SfxProtocol.PluginType.Indicator:
                    return PluginType.Indicator;
                case SfxProtocol.PluginType.Robot:
                    return PluginType.Robot;
                default:
                    return PluginType.Unknown;
            }
        }

        internal static UpdateType Convert(SfxProtocol.UpdateType type)
        {
            switch (type)
            {
                case SfxProtocol.UpdateType.Added:
                    return UpdateType.Added;
                case SfxProtocol.UpdateType.Updated:
                    return UpdateType.Updated;
                case SfxProtocol.UpdateType.Removed:
                    return UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        internal static RequestExecState Convert(SfxProtocol.RequestExecState type)
        {
            switch (type)
            {
                case SfxProtocol.RequestExecState.Completed:
                    return RequestExecState.Completed;
                case SfxProtocol.RequestExecState.InternalServerError:
                    return RequestExecState.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static ConnectionState Convert(SfxProtocol.ConnectionState state)
        {
            switch (state)
            {
                case SfxProtocol.ConnectionState.Offline:
                    return ConnectionState.Offline;
                case SfxProtocol.ConnectionState.Connecting:
                    return ConnectionState.Connecting;
                case SfxProtocol.ConnectionState.Online:
                    return ConnectionState.Online;
                case SfxProtocol.ConnectionState.Disconnecting:
                    return ConnectionState.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static ConnectionErrorCode Convert(SfxProtocol.ConnectionErrorCode code)
        {
            switch (code)
            {
                case SfxProtocol.ConnectionErrorCode.None:
                    return ConnectionErrorCode.None;
                case SfxProtocol.ConnectionErrorCode.Unknown:
                    return ConnectionErrorCode.Unknown;
                case SfxProtocol.ConnectionErrorCode.NetworkError:
                    return ConnectionErrorCode.NetworkError;
                case SfxProtocol.ConnectionErrorCode.Timeout:
                    return ConnectionErrorCode.Timeout;
                case SfxProtocol.ConnectionErrorCode.BlockedAccount:
                    return ConnectionErrorCode.BlockedAccount;
                case SfxProtocol.ConnectionErrorCode.ClientInitiated:
                    return ConnectionErrorCode.ClientInitiated;
                case SfxProtocol.ConnectionErrorCode.InvalidCredentials:
                    return ConnectionErrorCode.InvalidCredentials;
                case SfxProtocol.ConnectionErrorCode.SlowConnection:
                    return ConnectionErrorCode.SlowConnection;
                case SfxProtocol.ConnectionErrorCode.ServerError:
                    return ConnectionErrorCode.ServerError;
                case SfxProtocol.ConnectionErrorCode.LoginDeleted:
                    return ConnectionErrorCode.LoginDeleted;
                case SfxProtocol.ConnectionErrorCode.ServerLogout:
                    return ConnectionErrorCode.ServerLogout;
                case SfxProtocol.ConnectionErrorCode.Canceled:
                    return ConnectionErrorCode.Canceled;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
