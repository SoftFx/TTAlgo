using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Container
{
    [Serializable]
    internal class PackageLoader : IPluginLoader
    {
        private Package algoPackage;
        private string packagePath;

        public PackageLoader(string packagePath)
        {
            this.packagePath = packagePath;
        }

        public string MainAssemblyName { get; private set; }

        public void Init()
        {
            using (var stream = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                algoPackage = Package.Load(stream);
            MainAssemblyName = algoPackage.Metadata.MainBinaryFile;
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            return algoPackage.GetFile(packageLocalPath);
        }
    }
}
