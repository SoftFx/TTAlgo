using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public enum ConnectionStates
    {
        Offline,
        Connecting,
        Online,
        Disconnecting,
    }


    public class AccountModelInfo
    {
        public AccountKey Key { get; set; }

        public ConnectionStates ConnectionState { get; set; }

        public ConnectionErrorInfo LastError { get; set; }
    }
}
