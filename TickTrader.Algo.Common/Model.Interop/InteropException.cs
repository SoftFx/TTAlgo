using System;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Interop
{
    [Serializable]
    public class InteropException : Exception
    {
        public InteropException(string message, ConnectionErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }

        public ConnectionErrorCodes ErrorCode { get; }
    }
}
