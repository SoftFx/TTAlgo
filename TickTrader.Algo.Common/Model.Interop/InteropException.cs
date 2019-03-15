using System;
using System.Runtime.Serialization;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Interop
{
    [Serializable]
    public class InteropException : Exception, ISerializable
    {
        public string Reason { get; private set; } = string.Empty;

        public InteropException()
        {
            ErrorCode = ConnectionErrorCodes.Unknown;
        }

        public InteropException(string message)
            : base(message)
        {
            ErrorCode = ConnectionErrorCodes.Unknown;
        }

        public InteropException(string message, ConnectionErrorCodes errorCode)
        {
            Reason = message;
            ErrorCode = errorCode;
        }

        protected InteropException(SerializationInfo info, StreamingContext context)
            : base(info.GetString(nameof(Message)))
        {
            ErrorCode = (ConnectionErrorCodes)info.GetInt32(nameof(ErrorCode));
        }

        public ConnectionErrorCodes ErrorCode { get; }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Message), Message);
            info.AddValue(nameof(ErrorCode), (int)ErrorCode);
        }
    }
}
