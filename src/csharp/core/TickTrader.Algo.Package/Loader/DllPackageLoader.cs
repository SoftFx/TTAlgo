using System.IO;

namespace TickTrader.Algo.Package
{
    public class DllPackageLoader : IPackageLoader
    {
        private readonly string _dllFolderPath;


        public string PackagePath { get; }

        public string MainAssemblyName { get; }


        public DllPackageLoader(string filePath)
        {
            PackagePath = filePath;
            _dllFolderPath = Path.GetDirectoryName(filePath);
            MainAssemblyName = Path.GetFileName(filePath);
        }


        public void Init()
        {
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            var fullPath = Path.Combine(_dllFolderPath, packageLocalPath);

            try
            {
                return File.ReadAllBytes(fullPath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
