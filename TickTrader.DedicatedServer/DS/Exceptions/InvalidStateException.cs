namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class InvalidStateException : DSException
    {
        public InvalidStateException(string message): base(message)
        {
            Code = ExceptionCodes.InvalidState;
        }
    }
}
