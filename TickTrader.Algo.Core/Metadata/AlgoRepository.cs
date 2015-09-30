using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Metadata
{
    public class AlgoRepository : IDisposable
    {
        private const string AlgoFilesPattern = "*.tta";

        private FileSystemWatcher watcher;
        private string repPath;
        private Dictionary<string, RepAssembly> assemblies = new Dictionary<string, RepAssembly>();

        public AlgoRepository(string repPath)
        {
            new FileSystemWatcher(repPath, AlgoFilesPattern);
        }

        private void Scan()
        {
            string[] fileList = Directory.GetFiles(repPath, AlgoFilesPattern, SearchOption.AllDirectories);
            foreach (string file in fileList)
            {
                FileInfo info = new FileInfo(file);
                RepAssembly assemblyMetadata;
                if (assemblies.TryGetValue(file, out assemblyMetadata))
                {
                    if (info.LastWriteTime == assemblyMetadata.FileInfo.LastWriteTime
                        && info.Length == assemblyMetadata.FileInfo.Length)
                        continue;
                }
            }
        }

        internal class RepAssembly : IDisposable
        {
            private Isolated<AlgoSandbox> isolatedSandbox = new Isolated<AlgoSandbox>();

            public RepAssembly(FileInfo file)
            {
                this.FileInfo = file;
                //Assembly.LoadFile(repPath);
            }

            public FileInfo FileInfo { get; private set; }

            public void Dispose()
            {
                isolatedSandbox.Dispose();
            }
        }

        public void Dispose()
        {
        }
    }

    internal class AlgoSandbox : MarshalByRefObject
    {
        public AlgoSandbox(string assemblyName)
        {
            Assembly algoAssembly = Assembly.LoadFile(assemblyName);
            AlgoList = Api.AlgoMetadata.InspectAssembly(algoAssembly);
        }

        public IEnumerable<Api.AlgoMetadata> AlgoList { get; private set; }
    }
}
