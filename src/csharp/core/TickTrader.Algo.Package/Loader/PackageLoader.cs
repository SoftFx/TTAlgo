using System;
using System.IO;

namespace TickTrader.Algo.Package
{
    public interface IPackageLoader
    {
        string PackagePath { get; }
        string MainAssemblyName { get; }

        void Init();
        byte[] GetFileBytes(string packageLocalPath);
    }

    public static class PackageLoader
    {
        public static IPackageLoader CreateForPath(string packagePath)
        {
            var ext = Path.GetExtension(packagePath).ToLowerInvariant();

            switch (ext)
            {
                case ".ttalgo":
                    return new PackageV1Loader(packagePath);
                case ".dll":
                    return new DllPackageLoader(packagePath);
                default:
                    throw new NotSupportedException($"Unknown file type: {ext}");
            }
        }
    }
}
