using System;
using TickTrader.Algo.Protocol;
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
    }
}