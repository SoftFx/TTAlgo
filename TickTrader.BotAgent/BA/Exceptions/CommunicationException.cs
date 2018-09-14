using TickTrader.Algo.Common.Info;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class CommunicationException : BAException
    {
        public CommunicationException(string message, ConnectionErrorCodes fdkErrorCode) : base(message)
        {
            Code = ExceptionCodes.CommunicationError;
            FdkCode = fdkErrorCode;
        }

        public ConnectionErrorCodes FdkCode { get; }
    }
}
