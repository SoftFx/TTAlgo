using System.IO;

namespace TickTrader.Algo.Core.Lib
{
    public static class IoHelper
    {
        private const long ERROR_SHARING_VIOLATION = 0x20;
        private const long ERROR_LOCK_VIOLATION = 0x21;

        public static bool IsLockException(this IOException ex)
        {
            long win32ErrorCode = ex.HResult & 0xFFFF;
            return win32ErrorCode == ERROR_SHARING_VIOLATION || win32ErrorCode == ERROR_LOCK_VIOLATION;
        }
    }
}
