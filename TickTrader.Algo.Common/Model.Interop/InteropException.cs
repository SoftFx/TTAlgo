using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Model.Interop
{
    public class InteropException : Exception
    {
        public InteropException(string message, ConnectionErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }

        public ConnectionErrorCodes ErrorCode { get; }
    }

    public class ConnectionErrorInfo
    {
        private static readonly ConnectionErrorInfo okSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.None);
        private static readonly ConnectionErrorInfo unknownSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Unknown);
        private static readonly ConnectionErrorInfo canceledSingleton = new ConnectionErrorInfo(ConnectionErrorCodes.Canceled);

        public static ConnectionErrorInfo Ok => okSingleton;
        public static ConnectionErrorInfo UnknownNoText => unknownSingleton;
        public static ConnectionErrorInfo Canceled => canceledSingleton;

        public ConnectionErrorInfo(ConnectionErrorCodes code, string message = null)
        {
            Code = code;
            TextMessage = message;
        }

        public ConnectionErrorCodes Code { get; }
        public string TextMessage { get; }
    }

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
        Canceled
    }
}
