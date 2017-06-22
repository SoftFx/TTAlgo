namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class ExceptionCodes
    {
        public const int InvalidCredentials = 100;

        public const int PackageNotFound = 1001;
        public const int PackageIsLocked = 1002;

        public const int DuplicateAccount = 2000;
        public const int AccountNotFound = 2001;
        public const int InvalidAccount = 2002;
        public const int AccountIsLocked = 2003;

        public const int DuplicateBot = 3000;
        public const int BotNotFound = 3001;

        public const int CommunicationError = 4000;
		
        public const int InvalidState = 10000; // Needs to be decomposed into more detailed errors
    }
}
