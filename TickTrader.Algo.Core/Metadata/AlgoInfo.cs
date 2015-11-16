using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Metadata
{
    /// <summary>
    /// This is a serializable light copy of AlgoDescriptor.
    /// It is used to bypass descriptor to another application domain.
    /// </summary>
    [Serializable]
    public class AlgoInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public AlgoTypes AlgoLogicType { get; set; }
        public AlgoMetadataErrors? Error { get; set; }
        public bool IsValid { get { return Error == null; } }
        public IEnumerable<AlgoPropertyInfo> AllProperties { get; set; }
        public IEnumerable<ParameterInfo> Parameters { get; set; }
        public IEnumerable<InputInfo> Inputs { get; set; }
        public IEnumerable<OutputInfo> Outputs { get; set; }
    }
}
