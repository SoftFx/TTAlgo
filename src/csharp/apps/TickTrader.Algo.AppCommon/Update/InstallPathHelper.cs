using System.IO;

namespace TickTrader.Algo.AppCommon
{
    public static class InstallPathHelper
    {
        public static string GetCurrentVersionFolder(string installPath) => Path.Combine(installPath, "bin", "current");

        public static string GetRollbackVersionFolder(string installPath) => Path.Combine(installPath, "bin", "rollback");
    }
}
