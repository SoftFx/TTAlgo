using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class CommunicationException : BAException
    {
        public CommunicationException(string message, ConnectionErrorInfo.Types.ErrorCode fdkErrorCode) : base(message)
        {
            Code = ExceptionCodes.CommunicationError;
            FdkCode = fdkErrorCode;
        }

        public ConnectionErrorInfo.Types.ErrorCode FdkCode { get; }
    }
}
