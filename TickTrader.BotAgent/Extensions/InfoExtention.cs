using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.Extensions
{
    public static class InfoExtention
    {
        public static AccountModelInfo GetInfoCopy(this ClientModel acc)
        {
            return new AccountModelInfo
            {
                Key = new AccountKey(acc.Address, acc.Username),
                UseNewProtocol = acc.UseNewProtocol,
                ConnectionState = acc.ConnectionState,
                LastError = acc.LastError ?? ConnectionErrorInfo.Ok,
            };
        }

        public static List<AccountModelInfo> GetInfoCopy(this IEnumerable<ClientModel> accList)
        {
            return accList.Select(GetInfoCopy).ToList();
        }

        public static List<BotModelInfo> GetInfoCopy(this IEnumerable<TradeBotModel> botList)
        {
            return botList.Select(GetInfoCopy).ToList();
        }

        public static BotModelInfo GetInfoCopy(this TradeBotModel model)
        {
            return new BotModelInfo()
            {
                InstanceId = model.Id,
                Account = model.Account,
                State = model.State,
                FaultMessage = model.FaultMessage,
                Config = model.Config,
                Descriptor = model.AlgoRef?.Metadata.Descriptor,
            };
        }

        public static ChangeAction Convert(this UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.Added:
                    return ChangeAction.Added;
                case UpdateType.Replaced:
                    return ChangeAction.Modified;
                case UpdateType.Removed:
                    return ChangeAction.Removed;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
