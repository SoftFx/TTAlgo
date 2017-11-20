namespace TickTrader.BotAgent.BA.Exceptions
{
    public class PackageLockedException : BAException
    {
        public PackageLockedException(string message) : base(message)
        {
            Code = ExceptionCodes.PackageIsLocked;
        }
    }
}
