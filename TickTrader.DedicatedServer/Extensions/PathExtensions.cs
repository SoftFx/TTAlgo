using System.IO;

namespace TickTrader.DedicatedServer.Extensions
{
    public static class PathExtensions
    {
        public static bool IsPathAbsolute(string path)
        {
            return path == Path.GetFullPath(path);
        }
    }
}
