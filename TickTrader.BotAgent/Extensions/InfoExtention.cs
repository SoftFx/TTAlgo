using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.BotAgent.BA.Entities;
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

        public static AccountInfo GetInfoCopy(this ClientModel acc)
        {
            return new AccountInfo(acc.Address, acc.Username, acc.UseNewProtocol);
        }

        public static List<AccountInfo> GetInfoCopy(this IEnumerable<ClientModel> accList)
        {
            return accList.Select(GetInfoCopy).ToList();
        }

        public static List<TradeBotInfo> GetInfoCopy(this IEnumerable<TradeBotModel> botList)
        {
            return botList.Select(GetInfoCopy).ToList();
        }

        public static TradeBotInfo GetInfoCopy(this TradeBotModel model)
        {
            return new TradeBotInfo()
            {
                Account = model.Account,
                BotName = model.BotName,
                Id = model.Id,
                State = model.State,
                Config = model.GetConfigInfo(),
                Descriptor = model.AlgoRef?.Metadata,
                FaultMessage = model.FaultMessage
            };
        }

        public static TradeBotConfig GetConfigInfo(this TradeBotModel model)
        {
            return new TradeBotConfig()
            {
                Plugin = model.PluginId,
                PluginConfig = model.Config
            };
        }
    }
}
