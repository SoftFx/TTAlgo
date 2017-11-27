namespace TickTrader.BotAgent.BA.Exceptions
{
    public class DuplicateAccountException : BAException
    {
        public DuplicateAccountException(string message):base(message)
        {
            Code = ExceptionCodes.DuplicateAccount;
        }
    }
}
