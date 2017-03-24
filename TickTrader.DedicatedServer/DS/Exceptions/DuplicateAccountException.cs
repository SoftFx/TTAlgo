namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class DuplicateAccountException : DSException
    {
        public DuplicateAccountException(string message):base(message)
        {
            Code = ExceptionCodes.DuplicateAccount;
        }
    }
}
