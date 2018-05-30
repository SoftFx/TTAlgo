namespace TickTrader.Algo.Common.Info
{
    public enum ConnectionErrorCodes
    {
        None,
        Unknown,
        NetworkError,
        Timeout,
        BlockedAccount,
        ClientInitiated,
        InvalidCredentials,
        SlowConnection,
        ServerError,
        LoginDeleted,
        ServerLogout,
        Canceled,
        RejectedByServer
    }


    public class ConnectionErrorInfo
    {
        private static readonly ConnectionErrorInfo _okSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.None);
        private static readonly ConnectionErrorInfo _unknownSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Unknown);
        private static readonly ConnectionErrorInfo _canceledSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Canceled);


        public static ConnectionErrorInfo Ok => _okSingleton;

        public static ConnectionErrorInfo UnknownNoText => _unknownSingleton;

        public static ConnectionErrorInfo Canceled => _canceledSingleton;


        public ConnectionErrorCodes Code { get; }

        public string TextMessage { get; }


        public ConnectionErrorInfo(ConnectionErrorCodes code, string message = null)
        {
            Code = code;
            TextMessage = message;
        }
    }
}
