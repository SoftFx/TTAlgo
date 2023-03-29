using System.IO;

namespace TickTrader.Algo.AppCommon
{
    public static class InstallPathHelper
    {
        public static string GetCurrentVersionFolder(string installPath) => Path.Combine(installPath, "bin", "current");

        public static string GetUpdateVersionFolder(string installPath) => Path.Combine(installPath, "bin", "update");

        public static string GetFallbackVersionFolder(string installPath) => Path.Combine(installPath, "bin", "fallback");
    }
}
