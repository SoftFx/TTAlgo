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
