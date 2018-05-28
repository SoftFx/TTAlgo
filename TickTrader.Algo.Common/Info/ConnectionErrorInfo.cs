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
        private static readonly ConnectionErrorInfo okSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.None);
        private static readonly ConnectionErrorInfo unknownSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Unknown);
        private static readonly ConnectionErrorInfo canceledSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Canceled);


        public static ConnectionErrorInfo Ok => okSingleton;
        public static ConnectionErrorInfo UnknownNoText => unknownSingleton;
        public static ConnectionErrorInfo Canceled => canceledSingleton;


        public ConnectionErrorCodes Code { get; }
        public string TextMessage { get; }


        public ConnectionErrorInfo(ConnectionErrorCodes code, string message = null)
        {
            Code = code;
            TextMessage = message;
        }
    }
}
