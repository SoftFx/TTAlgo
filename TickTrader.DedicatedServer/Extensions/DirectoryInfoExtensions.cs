using System.IO;

namespace TickTrader.DedicatedServer.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static void Clean(this DirectoryInfo dInfo)
        {
            if (!dInfo.Exists)
                return;

            foreach (FileInfo fi in dInfo.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dInfo.GetDirectories())
            {
                di.Clean();
                di.Delete();
            }
        }
    }
}
