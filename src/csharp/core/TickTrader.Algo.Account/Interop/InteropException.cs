using System;
using System.Runtime.Serialization;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class InteropException : Exception, ISerializable
    {
        public InteropException()
        {
            ErrorCode = ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
        }

        public InteropException(string message)
            : base(message)
        {
            ErrorCode = ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
        }

        public InteropException(string message, ConnectionErrorInfo.Types.ErrorCode errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        protected InteropException(SerializationInfo info, StreamingContext context)
            : base(info.GetString(nameof(Message)))
        {
            ErrorCode = (ConnectionErrorInfo.Types.ErrorCode)info.GetInt32(nameof(ErrorCode));
        }

        public ConnectionErrorInfo.Types.ErrorCode ErrorCode { get; }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Message), Message);
            info.AddValue(nameof(ErrorCode), (int)ErrorCode);
        }
    }
}
