namespace TickTrader.DedicatedServer.DS.Models.Exceptions
{
    public class DuplicatePackageException : DSException
    {
        public DuplicatePackageException(string message):base(message)
        {
            Code = ExceptionCodes.DuplicatePackage;
        }
    }
}
