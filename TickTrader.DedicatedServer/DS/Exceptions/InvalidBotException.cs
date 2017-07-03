namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class InvalidBotException: DSException
    {
        public InvalidBotException(string message):base(message)
        {
            Code = ExceptionCodes.InvalidBot;
        }
    }
}
