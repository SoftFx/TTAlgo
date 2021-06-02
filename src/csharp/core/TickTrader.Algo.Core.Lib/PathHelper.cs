using System.IO;

namespace TickTrader.Algo.Core.Lib
{
    public static class PathHelper
    {
        public static string GetSafeFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }

        public static void EnsureDirectoryCreated(string path)
        {
            if (!Directory.Exists(path))
            {
                var dinfo = Directory.CreateDirectory(path);
            }
        }
    }
}
