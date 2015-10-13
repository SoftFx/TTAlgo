using System;
using System.Collections.Generic;
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
            Assembly algoAssembly = Assembly.LoadFile(filePath);
            var metadata = AlgoDescriptor.InspectAssembly(algoAssembly);
            return metadata.Select(d => d.GetInteropCopy()).ToList();
        }

        public IndicatorProxy CreateIndicatorProxy(string algoId,  AlgoHost data, IEnumerable<AlgoProxyParam> parameters)
        {
            return new IndicatorProxy(algoId, data, parameters);
        }
    }
}
