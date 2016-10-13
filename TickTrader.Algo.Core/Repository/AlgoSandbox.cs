using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    internal class AlgoSandbox : CrossDomainObject
    {
        public AlgoSandbox()
        {
        }

        internal T Activate<T>() where T : MarshalByRefObject, new()
        {
            return new T();
        }

        public PluginExecutor CreateExecutor(string pluginId)
        {
            return new PluginExecutor(pluginId);
        }

        public IEnumerable<AlgoPluginDescriptor> LoadAndInspect(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string pdbFileName = Path.GetFileNameWithoutExtension(filePath) + ".pdb";
            string pdbPath = Path.Combine(directory, pdbFileName);

            byte[] assemblyBytes = File.ReadAllBytes(filePath);
            byte[] symbolsBytes = null;

            try
            {
                symbolsBytes = File.ReadAllBytes(pdbPath);
            }
            catch (FileNotFoundException) { }

            Assembly algoAssembly = Assembly.Load(assemblyBytes, symbolsBytes);
            return AlgoPluginDescriptor.InspectAssembly(algoAssembly);
        }
    }
}
