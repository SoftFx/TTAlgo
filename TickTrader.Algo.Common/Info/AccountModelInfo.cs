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
        public string Login { get; set; }

        public string Server { get; set; }

        public bool UseNewProtocol { get; set; }

        public ConnectionStates ConnectionState { get; set; }

        public ConnectionErrorInfo LastError { get; set; }
    }
}
