using System;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Protocol.Sfx;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    internal static class ToProtocol
    {
        internal static BotState Convert(BotStates state)
        {
            switch (state)
            {
                case BotStates.Offline:
                    return BotState.Offline;
                case BotStates.Starting:
                    return BotState.Starting;
                case BotStates.Faulted:
                    return BotState.Faulted;
                case BotStates.Online:
                    return BotState.Online;
                case BotStates.Stopping:
                    return BotState.Stopping;
                case BotStates.Broken:
                    return BotState.Broken;
                case BotStates.Reconnecting:
                    return BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static PluginType Convert(Algo.Core.Metadata.AlgoTypes type)
        {
            switch (type)
            {
                case Algo.Core.Metadata.AlgoTypes.Indicator:
                    return PluginType.Indicator;
                case Algo.Core.Metadata.AlgoTypes.Robot:
                    return PluginType.Robot;
                case Algo.Core.Metadata.AlgoTypes.Unknown:
                    return PluginType.Unknown;
                default:
                    return PluginType.Unknown;
            }
        }

        internal static UpdateType Convert(ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    return UpdateType.Added;
                case ChangeAction.Removed:
                    return UpdateType.Removed;
                case ChangeAction.Modified:
                    return UpdateType.Updated;
                default:
                    throw new ArgumentException();
            }
        }

        internal static ConnectionState Convert(ConnectionStates state)
        {
            switch (state)
            {
                case ConnectionStates.Offline:
                    return ConnectionState.Offline;
                case ConnectionStates.Connecting:
                    return ConnectionState.Connecting;
                case ConnectionStates.Online:
                    return ConnectionState.Online;
                case ConnectionStates.Disconnecting:
                    return ConnectionState.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static ConnectionErrorCode Convert(ConnectionErrorCodes code)
        {
            switch (code)
            {
                case ConnectionErrorCodes.None:
                    return ConnectionErrorCode.None;
                case ConnectionErrorCodes.Unknown:
                    return ConnectionErrorCode.Unknown;
                case ConnectionErrorCodes.NetworkError:
                    return ConnectionErrorCode.NetworkError;
                case ConnectionErrorCodes.Timeout:
                    return ConnectionErrorCode.Timeout;
                case ConnectionErrorCodes.BlockedAccount:
                    return ConnectionErrorCode.BlockedAccount;
                case ConnectionErrorCodes.ClientInitiated:
                    return ConnectionErrorCode.ClientInitiated;
                case ConnectionErrorCodes.InvalidCredentials:
                    return ConnectionErrorCode.InvalidCredentials;
                case ConnectionErrorCodes.SlowConnection:
                    return ConnectionErrorCode.SlowConnection;
                case ConnectionErrorCodes.ServerError:
                    return ConnectionErrorCode.ServerError;
                case ConnectionErrorCodes.LoginDeleted:
                    return ConnectionErrorCode.LoginDeleted;
                case ConnectionErrorCodes.ServerLogout:
                    return ConnectionErrorCode.ServerLogout;
                case ConnectionErrorCodes.Canceled:
                    return ConnectionErrorCode.Canceled;
                default:
                    throw new ArgumentException();
            }
        }
    }
}