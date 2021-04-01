using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
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
                AccountId = acc.AccountId,
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
                PluginId = model.Id,
                State = model.State,
                FaultMessage = model.FaultMessage,
            };
        }

        public static ChangeAction Convert(this UpdateInfo.Types.UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateInfo.Types.UpdateType.Added:
                    return ChangeAction.Added;
                case UpdateInfo.Types.UpdateType.Replaced:
                    return ChangeAction.Modified;
                case UpdateInfo.Types.UpdateType.Removed:
                    return ChangeAction.Removed;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
