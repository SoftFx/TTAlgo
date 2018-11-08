using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal static class InfoExtensions
    {
        public static ConnectionStates ToInfo(this ConnectionModel.States state)
        {
            switch (state)
            {
                case ConnectionModel.States.Offline:
                case ConnectionModel.States.OfflineRetry:
                    return ConnectionStates.Offline;
                case ConnectionModel.States.Connecting:
                    return ConnectionStates.Connecting;
                case ConnectionModel.States.Online:
                    return ConnectionStates.Online;
                case ConnectionModel.States.Disconnecting:
                    return ConnectionStates.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }

        public static BotModelInfo ToInfo(this TradeBotModel tradeBot)
        {
            return new BotModelInfo
            {
                Account = null,
                Config = tradeBot.Config,
                Descriptor = tradeBot.PluginRef?.Metadata.Descriptor,
                FaultMessage = "",
                InstanceId = tradeBot.InstanceId,
                State = tradeBot.State,
            };
        }
    }
}
