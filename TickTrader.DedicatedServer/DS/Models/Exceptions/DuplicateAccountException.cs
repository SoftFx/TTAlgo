namespace TickTrader.DedicatedServer.DS.Models.Exceptions
{
    public class DuplicateAccountException : DSException
    {
        public DuplicateAccountException(string message):base(message)
        {
            Code = ExceptionCodes.DuplicateAccount;
        }
    }
}
