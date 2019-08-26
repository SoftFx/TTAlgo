using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public static class IoHelper
    {
        const long ERROR_SHARING_VIOLATION = 0x20;
        const long ERROR_LOCK_VIOLATION = 0x21;

        public static bool IsLockExcpetion(this IOException ex)
        {
            long win32ErrorCode = ex.HResult & 0xFFFF;
            return win32ErrorCode == ERROR_SHARING_VIOLATION || win32ErrorCode == ERROR_LOCK_VIOLATION;
        }
    }
}
