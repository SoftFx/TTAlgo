namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class DuplicatePackageException : DSException
    {
        public DuplicatePackageException(string message):base(message)
        {
            Code = ExceptionCodes.DuplicatePackage;
        }
    }
}
