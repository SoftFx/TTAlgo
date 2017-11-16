namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentClient
    {
        void Connected();

        void ConnectionError();

        void Disconnected();

        void SetAccountList(AccountListReportEntity report);
    }
}
