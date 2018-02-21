using System;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        #region Initialization

        bool ValidateCreds(string login, string password);

        AccountListReportEntity GetAccountList();

        BotListReportEntity GetBotList();

        PackageListReportEntity GetPackageList();

        #endregion Initialization


        #region Updates

        event Action<AccountModelUpdateEntity> AccountUpdated;

        event Action<BotModelUpdateEntity> BotUpdated;

        event Action<PackageModelUpdateEntity> PackageUpdated;

        event Action<BotStateUpdateEntity> BotStateUpdated;

        event Action<AccountStateUpdateEntity> AccountStateUpdated;

        #endregion Updates
    }
}
