using System.IO;

namespace TickTrader.Algo.Package
{
    public class PackageV1Loader : IPackageLoader
    {
        private V1.Package _algoPackage;

        public string PackagePath { get; }

        public string MainAssemblyName { get; private set; }


        public PackageV1Loader(string packagePath)
        {
            PackagePath = packagePath;
        }


        public void Init()
        {
            using (var stream = File.Open(PackagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                _algoPackage = V1.Package.Load(stream);
            MainAssemblyName = _algoPackage.Metadata.MainBinaryFile;
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            return _algoPackage.GetFile(packageLocalPath);
        }
    }
}
