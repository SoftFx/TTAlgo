namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentClient
    {
        #region Initialization

        void SetAccountList(AccountListReportEntity report);

        void SetPackageList(PackageListReportEntity report);

        #endregion Initialization


        #region Updates

        void UpdateAccount(AccountModelUpdateEntity update);

        void UpdateBot(BotModelUpdateEntity update);

        void UpdatePackage(PackageModelUpdateEntity update);

        #endregion Updates
    }
}
