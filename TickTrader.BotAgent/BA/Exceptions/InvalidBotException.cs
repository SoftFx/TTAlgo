namespace TickTrader.BotAgent.BA.Exceptions
{
    public class InvalidBotException: BAException
    {
        public InvalidBotException(string message):base(message)
        {
            Code = ExceptionCodes.InvalidBot;
        }
    }
}
