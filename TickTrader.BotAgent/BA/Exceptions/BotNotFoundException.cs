namespace TickTrader.BotAgent.BA.Exceptions
{
    public class BotNotFoundException: BAException
    {
        public BotNotFoundException(string message): base(message)
        {
            Code = ExceptionCodes.BotNotFound;
        }
    }
}
