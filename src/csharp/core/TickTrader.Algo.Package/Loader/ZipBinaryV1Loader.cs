using System.IO;

namespace TickTrader.Algo.Package
{
    public class ZipBinaryV1Loader : IPackageLoader
    {
        private readonly byte[] _zipBinary;
        private V1.Package _algoPackage;


        public string PackagePath => "in-memory";

        public string MainAssemblyName { get; private set; }


        public ZipBinaryV1Loader(byte[] zipBinary)
        {
            _zipBinary = zipBinary;
        }


        public void Init()
        {
            using (var stream = new MemoryStream(_zipBinary))
                _algoPackage = V1.Package.Load(stream, PackageLoader.MaxRawPkgSize);
            MainAssemblyName = _algoPackage.Metadata.MainBinaryFile;
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            return _algoPackage.GetFile(packageLocalPath);
        }
    }
}
