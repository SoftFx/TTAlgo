using System.Collections.Generic;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentClient
    {
        #region Initialization

        void InitPackageList(List<PackageInfo> report);

        void InitAccountList(List<AccountKey> report);

        void InitBotList(List<string> report);

        #endregion Initialization


        #region Updates

        void UpdatePackage(UpdateInfo<PackageInfo> update);

        void UpdateAccount(UpdateInfo<AccountKey> update);

        void UpdateBot(UpdateInfo<string> update);

        //void UpdateBotState(BotStateUpdateEntity update);

        //void UpdateAccountState(AccountStateUpdateEntity update);

        #endregion Updates
    }
}
