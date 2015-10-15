using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    internal class AlgoSandbox : MarshalByRefObject
    {
        public IEnumerable<AlgoInfo> LoadAndInspect(string filePath)
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
            var metadata = AlgoDescriptor.InspectAssembly(algoAssembly);
            return metadata.Select(d => d.GetInteropCopy()).ToList();
        }

        public IndicatorBuilder CreateIndicator(string id)
        {
            return new IndicatorBuilder(id);
        }
    }
}
