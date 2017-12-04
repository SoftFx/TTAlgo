namespace TickTrader.BotAgent.BA.Exceptions
{
    public class InvalidAccountException : BAException
    {
        public InvalidAccountException(string message):base(message)
        {
            Code = ExceptionCodes.InvalidAccount;
        }
    }
}
