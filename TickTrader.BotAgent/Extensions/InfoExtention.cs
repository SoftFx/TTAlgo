using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.Extensions
{
    public static class InfoExtention
    {
        public static AccountModelInfo GetInfoCopy(this ClientModel acc)
        {
            return new AccountModelInfo
            {
                AccountId = acc.AccountId,
                ConnectionState = acc.ConnectionState,
                LastError = acc.LastError ?? ConnectionErrorInfo.Ok,
                DisplayName = acc.DisplayName,
            };
        }

        public static AccountStateUpdate GetStateUpdate(this ClientModel acc)
        {
            return new AccountStateUpdate
            {
                Id = acc.AccountId,
                ConnectionState = acc.ConnectionState,
                LastError = acc.LastError ?? ConnectionErrorInfo.Ok,
            };
        }

        public static List<AccountModelInfo> GetInfoCopy(this IEnumerable<ClientModel> accList)
        {
            return accList.Select(GetInfoCopy).ToList();
        }

        public static List<PluginModelInfo> GetInfoCopy(this IEnumerable<TradeBotModel> botList)
        {
            return botList.Select(GetInfoCopy).ToList();
        }

        public static PluginModelInfo GetInfoCopy(this TradeBotModel model)
        {
            return new PluginModelInfo()
            {
                InstanceId = model.Id,
                AccountId = model.AccountId,
                State = model.State,
                FaultMessage = model.FaultMessage,
                Config = model.Config,
                Descriptor_ = model.Info?.Descriptor_,
            };
        }

        public static PluginStateUpdate GetStateUpdate(this TradeBotModel model)
        {
            return new PluginStateUpdate
            {
                Id = model.Id,
                State = model.State,
                FaultMessage = model.FaultMessage,
            };
        }
    }
}
