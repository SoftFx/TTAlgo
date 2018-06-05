using System;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal static class InfoExtensions
    {
        public static BotStates ToInfo(this BotModelStates state)
        {
            switch (state)
            {
                case BotModelStates.Stopped:
                    return BotStates.Offline;
                case BotModelStates.Running:
                    return BotStates.Online;
                case BotModelStates.Stopping:
                    return BotStates.Stopping;
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
                State = tradeBot.State.ToInfo(),
            };
        }
    }
}
