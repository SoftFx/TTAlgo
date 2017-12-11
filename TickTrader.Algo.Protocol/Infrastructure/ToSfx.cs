using System;
using Sfx = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    internal static class ToSfx
    {
        internal static Sfx.BotState Convert(BotState state)
        {
            switch (state)
            {
                case BotState.Offline:
                    return Sfx.BotState.Offline;
                case BotState.Starting:
                    return Sfx.BotState.Starting;
                case BotState.Faulted:
                    return Sfx.BotState.Faulted;
                case BotState.Online:
                    return Sfx.BotState.Online;
                case BotState.Stopping:
                    return Sfx.BotState.Stopping;
                case BotState.Broken:
                    return Sfx.BotState.Broken;
                case BotState.Reconnecting:
                    return Sfx.BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        internal static Sfx.LoginRejectReason Convert(LoginRejectReason reason)
        {
            switch (reason)
            {
                case LoginRejectReason.InvalidCredentials:
                    return Sfx.LoginRejectReason.InvalidCredentials;
                case LoginRejectReason.VersionMismatch:
                    return Sfx.LoginRejectReason.VersionMismatch;
                case LoginRejectReason.InternalServerError:
                    return Sfx.LoginRejectReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static Sfx.LogoutReason Convert(LogoutReason reason)
        {
            switch (reason)
            {
                case LogoutReason.ClientRequest:
                    return Sfx.LogoutReason.ClientRequest;
                case LogoutReason.ServerLogout:
                    return Sfx.LogoutReason.ServerLogout;
                case LogoutReason.InternalServerError:
                    return Sfx.LogoutReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static Sfx.PluginType Convert(PluginType type)
        {
            switch (type)
            {
                case PluginType.Indicator:
                    return Sfx.PluginType.Indicator;
                case PluginType.Robot:
                    return Sfx.PluginType.Robot;
                default:
                    return Sfx.PluginType.Unknown;
            }
        }

        internal static Sfx.UpdateType Convert(UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Added:
                    return Sfx.UpdateType.Added;
                case UpdateType.Updated:
                    return Sfx.UpdateType.Updated;
                case UpdateType.Removed:
                    return Sfx.UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }
    }
}