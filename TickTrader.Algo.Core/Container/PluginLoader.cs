using System;
using System.IO;

namespace TickTrader.Algo.Core.Container
{
    internal interface IPluginLoader
    {
        string FilePath { get; }
        string MainAssemblyName { get; }
        byte[] GetFileBytes(string packageLocalPath);
        void Init();
    }


    internal static class PluginLoader
    {
        public static IPluginLoader CreateForPath(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            switch(ext)
            {
                case ".ttalgo":
                    return new PackageLoader(filePath);
                case ".dll":
                    return new DllLoader(filePath);
                default:
                    throw new ArgumentException("Unrecognized file type: " + ext);
            }
        }

        [Serializable]
        private class DllLoader : IPluginLoader
        {
            private string _dllFolderPath;

            public string FilePath { get; }

            public string MainAssemblyName { get; }

            public DllLoader(string filePath)
            {
                FilePath = filePath;
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

        [Serializable]
        private class PackageLoader : IPluginLoader
        {
            private Package _algoPackage;
            private string _packagePath;

            public string FilePath { get; }

            public string MainAssemblyName { get; private set; }

            public PackageLoader(string packagePath)
            {
                FilePath = packagePath;
            }

            public void Init()
            {
                using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    _algoPackage = Package.Load(stream);
                MainAssemblyName = _algoPackage.Metadata.MainBinaryFile;
            }

            public byte[] GetFileBytes(string packageLocalPath)
            {
                return _algoPackage.GetFile(packageLocalPath);
            }
        }
    }
}
