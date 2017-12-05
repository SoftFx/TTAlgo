namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        bool ValidateCreds(string login, string password);

        AccountListReportEntity GetAccountList(string requestId);
    }
}
