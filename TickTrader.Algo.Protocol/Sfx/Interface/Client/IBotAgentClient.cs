namespace TickTrader.Algo.Protocol.Sfx
{
    public interface IBotAgentClient
    {
        #region Initialization

        void InitAccountList(AccountListReportEntity report);

        void InitBotList(BotListReportEntity report);

        void InitPackageList(PackageListReportEntity report);

        #endregion Initialization


        #region Updates

        void UpdateAccount(AccountModelUpdateEntity update);

        void UpdateBot(BotModelUpdateEntity update);

        void UpdatePackage(PackageModelUpdateEntity update);

        void UpdateBotState(BotStateUpdateEntity update);

        void UpdateAccountState(AccountStateUpdateEntity update);

        #endregion Updates
    }
}
