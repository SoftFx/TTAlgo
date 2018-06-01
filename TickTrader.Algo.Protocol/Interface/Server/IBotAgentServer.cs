using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        #region Initialization

        bool ValidateCreds(string login, string password);

        List<PackageInfo> GetPackageList();

        List<AccountModelInfo> GetAccountList();

        List<BotModelInfo> GetBotList();

        #endregion Initialization


        #region Updates

        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<AccountModelInfo>> AccountUpdated;

        event Action<UpdateInfo<BotModelInfo>> BotUpdated;

        event Action<BotModelInfo> BotStateUpdated;

        event Action<AccountModelInfo> AccountStateUpdated;

        #endregion Updates


        #region Requests

        void StartBot(string botId);

        void StopBot(string botId);

        void AddBot(AccountKey account, PluginConfig config);

        void RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        void ChangeBotConfig(string botId, PluginConfig newConfig);

        #endregion Requests
    }
}
