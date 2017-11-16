namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        void Connected(int sessionId);

        void Disconnected(int sessionId, string reason);

        AccountListReportEntity GetAccountList();
    }
}
