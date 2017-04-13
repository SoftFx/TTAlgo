namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class PackageLockedException : DSException
    {
        public PackageLockedException(string message) : base(message)
        {
            Code = ExceptionCodes.PackageIsLocked;
        }
    }
}
