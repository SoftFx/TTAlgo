namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class InvalidAccountException : DSException
    {
        public InvalidAccountException(string message):base(message)
        {
            Code = ExceptionCodes.InvalidAccount;
        }
    }
}
