using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.Extensions
{
    public static class InfoExtention
    {
        public static PackageInfo GetInfoCopy(this PackageModel package)
        {
            return new PackageInfo(package.Name, package.Created, package.IsValid, package.GetPlugins());
        }

        public static List<PackageInfo> GetInfo(this PackageStorage storage)
        {
            return storage.Packages.Select(GetInfoCopy).ToList();
        }

        public static AccountModelInfo GetInfoCopy(this ClientModel acc)
        {
            return new AccountModelInfo
            {
                Server = acc.Address,
                Login = acc.Username,
                UseNewProtocol = acc.UseNewProtocol,
                ConnectionState = acc.ConnectionState,
                LastError = acc.LastError,
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
                Config = model.Config,
                State = model.State,
                FaultMessage = model.FaultMessage
            };
        }
    }
}
