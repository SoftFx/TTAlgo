namespace TickTrader.BotAgent.BA.Exceptions
{
    public class InvalidStateException : BAException
    {
        public InvalidStateException(string message): base(message)
        {
            Code = ExceptionCodes.InvalidState;
        }
    }
}
