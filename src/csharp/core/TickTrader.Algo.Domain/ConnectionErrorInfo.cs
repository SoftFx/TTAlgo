namespace TickTrader.Algo.Domain
{
    public partial class ConnectionErrorInfo
    {
        private static readonly ConnectionErrorInfo _okSingleton = new ConnectionErrorInfo(Types.ErrorCode.NoConnectionError);
        private static readonly ConnectionErrorInfo _unknownSingleton = new ConnectionErrorInfo(Types.ErrorCode.UnknownConnectionError);
        private static readonly ConnectionErrorInfo _canceledSingleton = new ConnectionErrorInfo(Types.ErrorCode.Canceled);


        public static ConnectionErrorInfo Ok => _okSingleton;

        public static ConnectionErrorInfo UnknownNoText => _unknownSingleton;

        public static ConnectionErrorInfo Canceled => _canceledSingleton;


        public bool IsOk => Code == Types.ErrorCode.NoConnectionError;

        public string ErrMsg => Code == Types.ErrorCode.UnknownConnectionError
            ? $"{Code} ({TextMessage})" : Code.ToString();


        public ConnectionErrorInfo(Types.ErrorCode code, string message = null)
        {
            Code = code;
            TextMessage = message;
        }
    }
}
