using System;
using TickTrader.Algo.Account;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal static class InfoExtensions
    {
        public static AccountModelInfo.Types.ConnectionState ToInfo(this ConnectionModel.States state)
        {
            switch (state)
            {
                case ConnectionModel.States.Offline:
                case ConnectionModel.States.OfflineRetry:
                    return AccountModelInfo.Types.ConnectionState.Offline;
                case ConnectionModel.States.Connecting:
                    return AccountModelInfo.Types.ConnectionState.Connecting;
                case ConnectionModel.States.Online:
                    return AccountModelInfo.Types.ConnectionState.Online;
                case ConnectionModel.States.Disconnecting:
                    return AccountModelInfo.Types.ConnectionState.Disconnecting;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
