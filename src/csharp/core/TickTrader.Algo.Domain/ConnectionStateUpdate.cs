namespace TickTrader.Algo.Domain
{
    public partial class ConnectionStateUpdate
    {
        public ConnectionStateUpdate(Account.Types.ConnectionState oldState, Account.Types.ConnectionState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public ConnectionStateUpdate(string accountId, Account.Types.ConnectionState oldState, Account.Types.ConnectionState newState)
        {
            AccountId = accountId;
            OldState = oldState;
            NewState = newState;
        }
    }


    public static class ConnectionStateUpdateExtensions
    {
        public static bool IsOffline(this Account.Types.ConnectionState state) => state == Account.Types.ConnectionState.Offline;

        public static bool IsConnecting(this Account.Types.ConnectionState state) => state == Account.Types.ConnectionState.Connecting;

        public static bool IsDisconnecting(this Account.Types.ConnectionState state) => state == Account.Types.ConnectionState.Disconnecting;

        public static bool IsOnline(this Account.Types.ConnectionState state) => state == Account.Types.ConnectionState.Online;
    }
}
