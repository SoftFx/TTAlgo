using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Container
{
    [Serializable]
    internal class DllLoader : IPluginLoader
    {
        private string dllFolderPath;

        public string MainAssemblyName { get; private set; }

        public DllLoader(string filePath)
        {
            dllFolderPath = Path.GetDirectoryName(filePath);
            MainAssemblyName = Path.GetFileName(filePath);
        }

        public void Init()
        {
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            string fullPath = Path.Combine(dllFolderPath, packageLocalPath);

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
