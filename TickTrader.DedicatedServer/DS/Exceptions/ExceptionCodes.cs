namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class ExceptionCodes
    {
        public const int InvalidCredentials = 100;

        public const int DuplicatePackage = 1000;
        public const int PackageNotFound = 1001;
        public const int PackageIsLocked = 1002;

        public const int DuplicateAccount = 2000;
        public const int AccountNotFound = 2001;

        public const int InvalidState = 3000; // Needs to be decomposed into more detailed errors
    }
}
