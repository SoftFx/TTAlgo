using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Metadata
{
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
