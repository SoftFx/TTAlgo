namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class BotNotFoundException: DSException
    {
        public BotNotFoundException(string message): base(message)
        {
            Code = ExceptionCodes.BotNotFound;
        }
    }
}
