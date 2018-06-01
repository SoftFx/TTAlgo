using System.Collections.Generic;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentClient
    {
        #region Initialization

        void InitPackageList(List<PackageInfo> report);

        void InitAccountList(List<AccountModelInfo> report);

        void InitBotList(List<BotModelInfo> report);

        #endregion Initialization


        #region Updates

        void UpdatePackage(UpdateInfo<PackageInfo> update);

        void UpdateAccount(UpdateInfo<AccountModelInfo> update);

        void UpdateBot(UpdateInfo<BotModelInfo> update);

        void UpdateAccountState(UpdateInfo<AccountModelInfo> update);

        void UpdateBotState(UpdateInfo<BotModelInfo> update);

        #endregion Updates
    }
}
