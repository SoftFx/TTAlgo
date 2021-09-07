namespace TickTrader.Algo.Domain
{
    public partial class AccountStateUpdate
    {
        public AccountStateUpdate(string id, AccountModelInfo.Types.ConnectionState connectionState, ConnectionErrorInfo lastError)
        {
            Id = id;
            ConnectionState = connectionState;
            LastError = lastError;
        }
    }
}
