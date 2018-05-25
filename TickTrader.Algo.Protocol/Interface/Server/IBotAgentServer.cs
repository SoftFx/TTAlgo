using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        #region Initialization

        bool ValidateCreds(string login, string password);

        List<PackageInfo> GetPackageList();

        List<AccountKey> GetAccountList();

        List<string> GetBotList();

        #endregion Initialization


        #region Updates

        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<AccountKey>> AccountUpdated;

        event Action<UpdateInfo<string>> BotUpdated;

        //event Action<BotStateUpdateEntity> BotStateUpdated;

        //event Action<AccountStateUpdateEntity> AccountStateUpdated;

        #endregion Updates
    }
}
